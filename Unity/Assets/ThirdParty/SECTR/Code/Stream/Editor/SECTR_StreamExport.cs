// Copyright (c) 2014 Make Code Now! LLC
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
#define UNITY_4
#endif

#if !(UNITY_4 || UNITY_5_0 || UNITY_5_1)
#define UNITY_STREAM_ENLIGHTEN
#endif

#if !(UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
#define UNITY_MSE
#endif


using UnityEngine;
using UnityEditor;
#if UNITY_MSE
using UnityEditor.SceneManagement;
#endif
using System.IO;
using System.Collections.Generic;
using System.Reflection;

/// \ingroup Stream
/// A set of static utility functions for exporting scenes and doing other stream related processing.
/// 
/// In order to stream a scene, we need to split the base scene up into multiple levels. We use levels
/// and additive addition instead of Resource Bundles because they take less memory during load and
/// do not cause assets to be duplicated on disk.
public static class SECTR_StreamExport
{
	#region Public Interface
	/// Re-adds the data from the specified Sector to the current scene. Safe to call from command line. 
	/// <param name="sector">The Sector to import.</param>
	/// <returns>Returns true if Sector was successfully imported, false otherwise.</returns>
	public static bool ImportFromChunk(SECTR_Sector sector)
	{
		if(sector == null)
		{
			Debug.LogError("Cannot import invalid Sector.");
			return false;
		}

		if(!sector.Frozen)
		{
			Debug.Log("Skipping import of unfrozen Sector");
			return true;
		}

		if(!sector.gameObject.isStatic)
		{
			Debug.Log("Skipping import of dynamic Sector " + sector.name + ".");
			return true;
		}

		SECTR_Chunk chunk = sector.GetComponent<SECTR_Chunk>();
		if(chunk)
		{
			#if UNITY_MSE
			EditorSceneManager.OpenScene(chunk.NodeName, OpenSceneMode.Additive);
			#else
			EditorApplication.OpenSceneAdditive(chunk.NodeName);
			#endif
			GameObject newNode = GameObject.Find(chunk.NodeName);
			if(newNode == null)
			{
				Debug.LogError("Exported data does not match scene. Skipping import of " + sector.name + ".");
				return false;
			}

			SECTR_ChunkRef chunkRef = newNode.GetComponent<SECTR_ChunkRef>();
			if(chunkRef && chunkRef.RealSector)
			{
				newNode = chunkRef.RealSector.gameObject;
				if(chunkRef.Recentered)
				{
					newNode.transform.parent = sector.transform;
					newNode.transform.localPosition = Vector3.zero;
					newNode.transform.localRotation = Quaternion.identity;
					newNode.transform.localScale = Vector3.one;
				}
				newNode.transform.parent = null;
				GameObject.DestroyImmediate(chunkRef.gameObject);
			}

			while(newNode.transform.childCount > 0)
			{
				newNode.transform.GetChild(0).parent = sector.transform;
			}


			// Merge lightmaps into the scene
#if !UNITY_STREAM_ENLIGHTEN
			SECTR_LightmapRef newRef = newNode.GetComponent<SECTR_LightmapRef>();
			if(newRef)
			{
				int numLightmaps = LightmapSettings.lightmaps.Length;
				LightmapData[] newLightmaps = new LightmapData[numLightmaps];
				for(int lightmapIndex = 0; lightmapIndex < numLightmaps; ++lightmapIndex)
				{
					newLightmaps[lightmapIndex] = LightmapSettings.lightmaps[lightmapIndex];
				}
				foreach(SECTR_LightmapRef.RefData refData in newRef.LightmapRefs)
				{
					if(refData.index >= 0 && refData.index < numLightmaps)
					{
						LightmapData newData = new LightmapData();
						newData.lightmapNear = refData.NearLightmap;
						newData.lightmapFar = refData.FarLightmap;
						newLightmaps[refData.index] = newData;
					}
				}
				LightmapSettings.lightmaps = newLightmaps;

#if !UNITY_4
				foreach(SECTR_LightmapRef.RenderData indexData in newRef.LightmapRenderers)
				{
					if(indexData.renderer)
					{
						indexData.renderer.lightmapIndex = indexData.rendererLightmapIndex;
						indexData.renderer.lightmapScaleOffset = indexData.rendererLightmapScaleOffset;
						GameObjectUtility.SetStaticEditorFlags(indexData.renderer.gameObject, GameObjectUtility.GetStaticEditorFlags(indexData.renderer.gameObject) | StaticEditorFlags.BatchingStatic);
					}
					if(indexData.terrain)
					{
						indexData.terrain.lightmapIndex = indexData.terrainLightmapIndex;
					}
				}
#endif

				GameObject.DestroyImmediate(newRef);
			}
#endif

			
			// Copy terrain component specially because the generic routine doesn't work for some reason.
			Terrain terrain = newNode.GetComponent<Terrain>();
			if(terrain)
			{
				Terrain terrainClone = sector.gameObject.AddComponent<Terrain>();
				terrainClone.terrainData = terrain.terrainData;
				terrainClone.basemapDistance = terrain.basemapDistance;
				terrainClone.castShadows = terrain.castShadows;
				terrainClone.detailObjectDensity = terrain.detailObjectDensity;
				terrainClone.detailObjectDistance = terrain.detailObjectDistance;
				terrainClone.heightmapMaximumLOD = terrain.heightmapMaximumLOD;
				terrainClone.heightmapPixelError = terrain.heightmapPixelError;
				terrainClone.lightmapIndex = terrain.lightmapIndex;
				terrainClone.treeBillboardDistance = terrain.treeBillboardDistance;
				terrainClone.treeCrossFadeLength = terrain.treeCrossFadeLength;
				terrainClone.treeDistance = terrain.treeDistance;
				terrainClone.treeMaximumFullLODCount = terrain.treeMaximumFullLODCount;
				terrainClone.Flush();
			}

			// Destroy the placeholder Member if there is one.
			// It's theoretically possible to have multiple members, so remove them all.
			SECTR_Member[] oldMembers = newNode.GetComponents<SECTR_Member>();
			int numOldMembers = oldMembers.Length;
			for(int oldIndex = 0; oldIndex < numOldMembers; ++oldIndex)
			{
				GameObject.DestroyImmediate(oldMembers[oldIndex]);
			}
			
			// Copy all remaining components over
			Component[] remainingComponents = newNode.GetComponents<Component>();
			int numRemaining = remainingComponents.Length;
			for(int componentIndex = 0; componentIndex < numRemaining; ++componentIndex)
			{
				Component component = remainingComponents[componentIndex];
				if(component != newNode.transform && component.GetType() != typeof(Terrain))
				{
					Component componentClone = sector.gameObject.AddComponent(component.GetType());
					EditorUtility.CopySerialized(component, componentClone);
				}
			}

			// Enable a TerrainComposer node if there is one.
			MonoBehaviour terrainNeighbors = sector.GetComponent("TerrainNeighbors") as MonoBehaviour;
			if(terrainNeighbors)
			{
				terrainNeighbors.enabled = true;
			}

			GameObject.DestroyImmediate(newNode);
			sector.Frozen = false;
			sector.ForceUpdate(true);
			chunk.enabled = false;
			return true;
		}
		return false;
	}
	
	/// Exports the specific Sector into an external level file, deleting the current scene copy in the process. Safe to call from command line. 
	/// <param name="sector">The Sector to export.</param>
	/// <returns>Returns true if Sector was successfully exported, false otherwise.</returns>
	public static bool ExportToChunk(SECTR_Sector sector)
	{
		if(string.IsNullOrEmpty(SECTR_Asset.CurrentScene()))
		{
			Debug.LogError("Scene must be saved befor export.");
			return false;
		}

		if(sector == null)
		{
			Debug.LogError("Cannot export null Sector.");
			return false;
		}

		if(!sector.gameObject.activeInHierarchy)
		{
			Debug.LogError("Cannot export inactive Sectors.");
			return false;
		}

		if(!sector.gameObject.isStatic)
		{
			Debug.Log("Skipping export of dynamic sector" + sector.name + ".");
			return true;
		}

		if(sector.Frozen)
		{
			// Already exported
			Debug.Log("Skipping frozen sector " + sector.name);
			return true;
		}

		string sceneDir;
		string sceneName;
		string exportDir = SECTR_Asset.MakeExportFolder("Chunks", false, out sceneDir, out sceneName);
		if(string.IsNullOrEmpty(exportDir))
		{
			Debug.LogError("Could not create Chunks folder.");
			return false;
		}

		// Delete the previous export, if there is one.
		// Prevents duplicate names piling up.
		SECTR_Chunk oldChunk = sector.GetComponent<SECTR_Chunk>();
		if(oldChunk)
		{
			AssetDatabase.DeleteAsset(oldChunk.NodeName);
			SECTR_VC.WaitForVC();
		}

		// Sectors are not guaranteed to be uniquely named, so always generate a unique name. 
		string originalSectorName = sector.name;
		string newAssetPath = AssetDatabase.GenerateUniqueAssetPath(exportDir + sceneName + "_" + originalSectorName + ".unity");
		sector.name = newAssetPath;

		// Make sure the current scene is saved, preserving all changes.
		#if UNITY_MSE
		EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
		#else
		EditorApplication.SaveScene();
		#endif
		SECTR_VC.WaitForVC();

		string originalScene = SECTR_Asset.CurrentScene();
		List<EditorBuildSettingsScene> sceneSettings = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

		// SaveScene can cause crashes w/ version control, so we work around it with a copy.
		AssetDatabase.CopyAsset(originalScene, newAssetPath);
		SECTR_VC.WaitForVC();

		#if UNITY_MSE
		EditorSceneManager.OpenScene(newAssetPath, OpenSceneMode.Single);
		#else
		EditorApplication.OpenScene(newAssetPath);
		#endif
		SECTR_VC.WaitForVC();

		sector = _FindSectorByName(newAssetPath);

		// Make sure to force update all members so that membership info is correct.
		List<SECTR_Member> allMembers = FindAllOfType<SECTR_Member>();
		for(int memberIndex = 0; memberIndex < allMembers.Count; ++memberIndex)
		{
			allMembers[memberIndex].ForceUpdate(true);
		}

		// Multi-sector members need to stay in the master scene.
		foreach(SECTR_Member member in allMembers)
		{
			if(member.Sectors.Count > 1 && member.transform.IsChildOf(sector.transform))
			{
				bool unparentMember = true;

				// Only affect the first member in the hierarchy below the sector
				Transform parent = member.transform.parent;
				while(parent != sector.transform)
				{
					if(parent.GetComponent<SECTR_Member>() != null)
					{
						unparentMember = false;
						break;
					}
					parent = parent.parent;
				}

				if(unparentMember)
				{
					if(PrefabUtility.GetPrefabType(sector.gameObject) != PrefabType.None)
					{
						Debug.LogWarning("Export is unparenting shared member " + member.name + " from prefab Sector " + sector.name + ". This will break the prefab.");
					}
					member.transform.parent = null;
				}
			}
		}

		// Unparent the sector from anything
		sector.transform.parent = null;

		// Any children of this sector should be exported.
		// The rest should be destroyed.
		List<Transform> allXforms = FindAllOfType<Transform>();

#if !UNITY_STREAM_ENLIGHTEN
		List<int> referencedLightmaps = new List<int>(LightmapSettings.lightmaps.Length);
#endif
		foreach(Transform transform in allXforms)
		{
			if(transform && transform.IsChildOf(sector.transform))
			{
#if !UNITY_STREAM_ENLIGHTEN
				Renderer childRenderer = transform.GetComponent<Renderer>();
				if(childRenderer && childRenderer.lightmapIndex >= 0 && !referencedLightmaps.Contains(childRenderer.lightmapIndex))
				{
					referencedLightmaps.Add(childRenderer.lightmapIndex);
				}

				Terrain childTerrain = transform.GetComponent<Terrain>();;
				if(childTerrain && childTerrain.lightmapIndex >= 0 && !referencedLightmaps.Contains(childTerrain.lightmapIndex))
				{
					referencedLightmaps.Add(childTerrain.lightmapIndex);
				}
#endif
			}
			else if(transform)
			{
				GameObject.DestroyImmediate(transform.gameObject);
			}
		}

#if !UNITY_STREAM_ENLIGHTEN
		if(referencedLightmaps.Count > 0)
		{
			SECTR_LightmapRef newRef = sector.GetComponent<SECTR_LightmapRef>();
			if(!newRef)
			{
				newRef = sector.gameObject.AddComponent<SECTR_LightmapRef>();
			}
			newRef.ReferenceLightmaps(referencedLightmaps);
		}

		// Nuke global data like nav meshes and lightmaps
		// Lightmap indexes will be preserved on export.
		NavMeshBuilder.ClearAllNavMeshes();
#if !UNITY_4
		SerializedObject serialObj = new SerializedObject(GameObject.FindObjectOfType<LightmapSettings>());
		SerializedProperty snapshotProp = serialObj.FindProperty("m_LightmapSnapshot");
		snapshotProp.objectReferenceValue = null;
		serialObj.ApplyModifiedProperties();
#endif
		LightmapSettings.lightmaps = new LightmapData[0];
		LightmapSettings.lightProbes = new LightProbes();
#endif

		GameObject dummyParent = new GameObject(newAssetPath);
		SECTR_ChunkRef chunkRef = dummyParent.AddComponent<SECTR_ChunkRef>();
		chunkRef.RealSector = sector.transform;
		sector.transform.parent = dummyParent.transform;

		// If the sector has a chunk marked for re-use, perform some special work.
		SECTR_Chunk originalChunk = sector.GetComponent<SECTR_Chunk>();
		if(originalChunk && originalChunk.ExportForReuse)
		{
			chunkRef.Recentered = true;
			sector.transform.localPosition = Vector3.zero;
			sector.transform.localRotation = Quaternion.identity;
			sector.transform.localScale = Vector3.one;
			sector.gameObject.SetActive(false);
		}

		// Rename the real chunk root with a clear name.
		sector.name = originalSectorName + "_Chunk";

		// Strip off any functional objects that will be preserved in the root scene.
		// Destroy the chunk first because it has dependencies on Sector.
		GameObject.DestroyImmediate(originalChunk);
		Component[] components = sector.GetComponents<Component>();
		foreach(Component component in components)
		{
			if(component.GetType().IsSubclassOf(typeof(MonoBehaviour)) &&
			   component.GetType() != typeof(Terrain) && component.GetType() != typeof(SECTR_LightmapRef))
			{
				GameObject.DestroyImmediate(component);
			}
		}

		// Re-add a member that will persist all of the references and save us work post load.
		SECTR_Member refMember = chunkRef.RealSector.gameObject.AddComponent<SECTR_Member>();
		refMember.NeverJoin = true;
		refMember.BoundsUpdateMode = SECTR_Member.BoundsUpdateModes.Static;
		refMember.ForceUpdate(true);

		// Save scene and append it to the build settings.
		#if UNITY_MSE
		EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
		#else
		EditorApplication.SaveScene();
		#endif
		SECTR_VC.WaitForVC();

		EditorBuildSettingsScene sectorSceneSettings = new EditorBuildSettingsScene(newAssetPath, true);
		bool sceneExists = false;
		foreach(EditorBuildSettingsScene oldScene in sceneSettings)
		{
			if(oldScene.path == newAssetPath)
			{
				sceneExists = true;
				oldScene.enabled = true;
				break;
			}
		}
		if(!sceneExists)
		{
			sceneSettings.Add(sectorSceneSettings);
		}
		string[] pathParts = newAssetPath.Split('/');
		string sectorPath = pathParts[pathParts.Length - 1].Replace(".unity", "");

		// Update the master scene with exported info.
		#if UNITY_MSE
		EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
		#else
		EditorApplication.OpenScene(originalScene);
		#endif
		SECTR_VC.WaitForVC();

		sector = _FindSectorByName(newAssetPath);
		sector.name = originalSectorName;

		DeleteExportedSector(sector);

		// Make sure Sectors has a Chunk
		SECTR_Chunk newChunk = sector.GetComponent<SECTR_Chunk>();
		if(!newChunk)
		{
			newChunk = sector.gameObject.AddComponent<SECTR_Chunk>();
		}
		newChunk.ScenePath = sectorPath;
		newChunk.NodeName = newAssetPath;
		newChunk.enabled = true;

		// Disable a TerrainComposer node if there is one.
		MonoBehaviour terrainNeighbors = sector.GetComponent("TerrainNeighbors") as MonoBehaviour;

		if(terrainNeighbors)
		{
			terrainNeighbors.enabled = false;
		}

		// Save off the accumulated build settings
		EditorBuildSettings.scenes = sceneSettings.ToArray();
		AssetDatabase.Refresh();

		#if UNITY_MSE
		EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
		#else
		EditorApplication.SaveScene();
		#endif
		SECTR_VC.WaitForVC();

		return true;
	}

	public static void DeleteExportedSector(SECTR_Sector sector)
	{
		// Force update all members
		List<SECTR_Member> allMembers = FindAllOfType<SECTR_Member>();
		for(int memberIndex = 0; memberIndex < allMembers.Count; ++memberIndex)
		{
			allMembers[memberIndex].ForceUpdate(true);
		}

		// Remove everything from the Sector except for the transform and any MonoBehavior
		// (but also nuke Terrain which is a MonoBehavior but really isn't)
		Component[] components = sector.GetComponents<Component>();
		foreach(Component component in components)
		{
			if(component != sector.transform && 
			   (!component.GetType().IsSubclassOf(typeof(MonoBehaviour)) || component.GetType() == typeof(Terrain) || component.GetType() == typeof(SECTR_LightmapRef)))
			{
				GameObject.DestroyImmediate(component);
			}
		}
		
		// Multi-sector members stay in the master scene, so unparent them.
		List<SECTR_Member> sharedMembers = new List<SECTR_Member>();
		foreach(SECTR_Member member in allMembers)
		{
			if(member.Sectors.Count > 1 && member.transform.parent && member.transform.parent.IsChildOf(sector.transform))
			{
				bool unparentMember = true;

				// Only unparent the first member in the heirarchy below the transform.
				Transform parent = member.transform.parent;
				while(parent != sector.transform)
				{
					if(parent.GetComponent<SECTR_Member>() != null)
					{
						unparentMember = false;
						break;
					}
					parent = parent.parent;
				}

				if(unparentMember)
				{
					member.transform.parent = null;
					sharedMembers.Add(member);
				}
			}
		}
		
		// Destroy all exported children
		List<Transform> allXforms = FindAllOfType<Transform>();
		foreach(Transform transform in allXforms)
		{
			if(transform && transform.IsChildOf(sector.transform) && transform != sector.transform)
			{
				GameObject.DestroyImmediate(transform.gameObject);
			}
		}
		
		// Now reparent the global objects
		foreach(SECTR_Member member in sharedMembers)
		{
			member.transform.parent = sector.transform;
		}

#if !UNITY_STREAM_ENLIGHTEN
		List<int> referencedLightmaps = new List<int>(LightmapSettings.lightmaps.Length);
		
		List<Renderer> renderers = FindAllOfType<Renderer>();
		foreach(Renderer renderer in renderers)
		{
			if(renderer && renderer.lightmapIndex >= 0 && !referencedLightmaps.Contains(renderer.lightmapIndex))
			{
				referencedLightmaps.Add(renderer.lightmapIndex);
			}
		}
		
		List<Terrain> terrains = FindAllOfType<Terrain>();
		foreach(Terrain terrain in terrains)
		{
			if(terrain && terrain.lightmapIndex >= 0 & !referencedLightmaps.Contains(terrain.lightmapIndex))
			{
				referencedLightmaps.Add(terrain.lightmapIndex);
			}
		}
		
		int numLightmaps = LightmapSettings.lightmaps.Length;
		LightmapData[] newLightmaps = new LightmapData[numLightmaps];
		for(int lightmapIndex = 0; lightmapIndex < numLightmaps; ++lightmapIndex)
		{
			if(!referencedLightmaps.Contains(lightmapIndex))
			{
				newLightmaps[lightmapIndex] = new LightmapData();
			}
			else
			{
				newLightmaps[lightmapIndex] = LightmapSettings.lightmaps[lightmapIndex];
			}
		}
		LightmapSettings.lightmaps = newLightmaps;
#endif

		Resources.UnloadUnusedAssets();
		
		// Freeze the sector to preserve bounds but prevent updates.
		sector.Frozen = true;
	}
	
	/// Exports all of the Sectors in the scene, with user prompts and other helpful dialogs.
	public static void ExportSceneChunksUI()
	{
		if(string.IsNullOrEmpty(SECTR_Asset.CurrentScene()))
		{
			EditorUtility.DisplayDialog("Export Error", "Cannot export from a scene that's never been saved.", "Ok");
		}

		if(!SECTR_VC.CheckOut(SECTR_Asset.CurrentScene()))
		{
			EditorUtility.DisplayDialog("Export Error", "Could not check out " + SECTR_Asset.CurrentScene() + ". Export aborted.", "Ok");
			return;
		}

		string sceneDir;
		string sceneName;
		string exportDir = SECTR_Asset.MakeExportFolder("Chunks", false, out sceneDir, out sceneName);
		if(string.IsNullOrEmpty(exportDir))
		{
			EditorUtility.DisplayDialog("Export Error", "Could not create Chunks folder. Aborting Export.", "Ok");
			return;
		}

		SECTR_Loader[] loaders = (SECTR_Loader[])GameObject.FindObjectsOfType(typeof(SECTR_Loader));
		if(loaders.Length == 0 && !EditorUtility.DisplayDialog("No Loaders", "This scene has no loaders. Are you sure you wish to export?", "Ok", "Cancel"))
		{
			return;
		}

		int backupValue = _ShowBackupPrompt();
		if(backupValue != 1)
		{
			ExportSceneChunks();
			#if UNITY_4
			EditorUtility.DisplayDialog("Streaming Export Complete", "If this is the first time you've exported this level since launching Unity, you will need to build the project before you can use the _Streaming level.", "Ok");
			#endif
		}
	}

	/// Exports all Sectors in the scene. Safe to call from the command line.
	public static void ExportSceneChunks()
	{
		// Create a progress bar, because we're friendly like that.
		string progressTitle = "Chunking Level For Streaming";
		EditorUtility.DisplayProgressBar(progressTitle, "Preparing", 0);

		List<EditorBuildSettingsScene> sceneSettings = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
		EditorBuildSettingsScene rootSceneSettings = new EditorBuildSettingsScene(SECTR_Asset.CurrentScene(), true);
		bool sceneExists = false;
		foreach(EditorBuildSettingsScene oldScene in sceneSettings)
		{
			if(oldScene.path == SECTR_Asset.CurrentScene())
			{
				sceneExists = true;
				oldScene.enabled = true;
				break;
			}
		}
		if(!sceneExists)
		{
			sceneSettings.Add(rootSceneSettings);
			EditorBuildSettings.scenes = sceneSettings.ToArray();
		}
		
		// Export each sector to an individual file.
		// Inner loop reloads the scene, and Sector creation order is not deterministic, 
		// so it requires multiple passes through the list.
		int numSectors = SECTR_Sector.All.Count;
		int progress = 0;
		int unfrozenSectors = 0;

		// Figure out how many sectors we should be exporting.
		for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
		{
			SECTR_Sector sector = SECTR_Sector.All[sectorIndex];
			if(!sector.Frozen)
			{
				++unfrozenSectors;
			}
		}
			
		while(progress < unfrozenSectors)
		{
			for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
			{
				SECTR_Sector sector = SECTR_Sector.All[sectorIndex];
				if(!sector.Frozen)
				{
					EditorUtility.DisplayProgressBar(progressTitle, "Exporting " + sector.name, (float)progress / (float)unfrozenSectors);
					ExportToChunk(sector);
					++progress;
				}
			}
		}

		#if UNITY_MSE
		EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
		#else
		EditorApplication.SaveScene();
		#endif
		SECTR_VC.WaitForVC();

		// Cleanup
		EditorUtility.ClearProgressBar();
	}
	
	/// Imports all of the Sectors in the scene, with user prompts and other helpful dialogs.
	public static void ImportSceneChunksUI()
	{
		if(string.IsNullOrEmpty(SECTR_Asset.CurrentScene()))
		{
			EditorUtility.DisplayDialog("Import Error", "Cannot import into scene that's never been saved.", "Ok");
		}

		int backupValue = _ShowBackupPrompt();
		if(backupValue != 1)
		{
			ImportSceneChunks();
		}
	}
	
	/// Imports all exported Sectors into the scene. Safe to call from the command line.
	public static void ImportSceneChunks()
	{
		int numSectors = SECTR_Sector.All.Count;
		for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
		{
			SECTR_Sector sector = SECTR_Sector.All[sectorIndex];
			if(sector.Frozen)
			{
				EditorUtility.DisplayProgressBar("Importing Scene Chunks", "Importing " + sector.name, (float)sectorIndex / (float)numSectors);
				ImportFromChunk(sector);
			}
		}
		if(SECTR_VC.CheckOut(SECTR_Asset.CurrentScene()))
		{
			#if UNITY_MSE
			EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
			#else
			EditorApplication.SaveScene();
			#endif
			SECTR_VC.WaitForVC();
		}
		EditorUtility.ClearProgressBar();
	}

	/// Reverts all of the imported Sectors in the scene, with user prompts and other helpful dialogs.
	public static void RevertSceneChunksUI()
	{
		int backupValue = _ShowBackupPrompt();
		if(backupValue != 1)
		{
			RevertSceneChunks();
		}
	}
	
	/// Reverts all imported Sectors into the scene. Safe to call from the command line.
	public static void RevertSceneChunks()
	{
		int numSectors = SECTR_Sector.All.Count;
		for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
		{
			SECTR_Sector sector = SECTR_Sector.All[sectorIndex];
			EditorUtility.DisplayProgressBar("Reverting Scene Chunks", "Reverting " + sector.name, (float)sectorIndex / (float)numSectors);
			RevertChunk(sector);
		}
		if(SECTR_VC.CheckOut(SECTR_Asset.CurrentScene()))
		{
			#if UNITY_MSE
			EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
			#else
			EditorApplication.SaveScene();
			#endif
			SECTR_VC.WaitForVC();
		}
		EditorUtility.ClearProgressBar();
	}

	public static void RevertChunk(SECTR_Sector sector)
	{
		SECTR_Chunk chunk = sector.GetComponent<SECTR_Chunk>();
		if(!sector.Frozen && chunk &&
			System.IO.File.Exists(SECTR_Asset.UnityToOSPath(chunk.NodeName)))
		{
			DeleteExportedSector(sector);
			chunk.enabled = true;
			#if UNITY_MSE
			EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByPath(chunk.NodeName), true);
			#endif
		}
	}
	
	/// Writes out the current scene's Sector/Portal graph as a .dot file
	/// which can be visualized in programs like GraphVis and the like.
	public static void WriteGraphDot()
	{
		if(!string.IsNullOrEmpty(SECTR_Asset.CurrentScene()))
		{
			string sceneDir;
			string sceneName;
			SECTR_Asset.GetCurrentSceneParts(out sceneDir, out sceneName);
			sceneName = sceneName.Replace(".unity", "");
			
			string graphFile = SECTR_Graph.GetGraphAsDot(sceneName);
			
			string path = sceneDir + sceneName + "_SECTR_Graph.dot";
			File.WriteAllText(SECTR_Asset.UnityToOSPath(path), graphFile);
			AssetDatabase.Refresh();
			Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
			EditorUtility.FocusProjectWindow();
		}
	}
	#endregion

	#region Private Methods
	private static int _ShowBackupPrompt()
	{
		int dialogValue = EditorUtility.DisplayDialogComplex("Make Backup?", "This operation will significantly modify your scene. Would you like to make a backup first?", "Yes", "Cancel", "No");
		if(dialogValue == 0)
		{
			string sceneDir;
			string sceneName;
			SECTR_Asset.SplitPath(SECTR_Asset.CurrentScene(), out sceneDir, out sceneName);
			sceneName = sceneName.Replace(".unity", "");
			AssetDatabase.CopyAsset(SECTR_Asset.CurrentScene(), sceneDir + sceneName + "_Backup.unity");
			AssetDatabase.Refresh();
			SECTR_VC.WaitForVC();
		}
		return dialogValue;
	}

	private static List<T> FindAllOfType<T>() where T : Component
	{
		List<T> sceneObjects = new List<T>();
		T[] everything = (T[])Resources.FindObjectsOfTypeAll(typeof(T));
		foreach(T item in everything)
		{
			// Be very sure that we're not destroying a resource.
			if((item.gameObject.hideFlags & HideFlags.NotEditable) == 0 &&
				!EditorUtility.IsPersistent(item.gameObject) &&
			   	!EditorUtility.IsPersistent(item.transform.gameObject) &&
			   	string.IsNullOrEmpty(AssetDatabase.GetAssetPath(item.gameObject)) &&
			   	string.IsNullOrEmpty(AssetDatabase.GetAssetPath(item.transform.root.gameObject)))
			{
				sceneObjects.Add(item);
			}
		}
		return sceneObjects;
	}

	private static SECTR_Sector _FindSectorByName(string name)
	{
		int numSectors = SECTR_Sector.All.Count;
		for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
		{
			SECTR_Sector sector = SECTR_Sector.All[sectorIndex];
			if(sector.name == name)
			{
				return sector;
			}
		}
		return null;
	}
	#endregion
}
