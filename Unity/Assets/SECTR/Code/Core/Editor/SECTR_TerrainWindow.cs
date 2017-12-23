// Copyright (c) 2014 Make Code Now! LLC
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
#define UNITY_4
#endif

#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
#define UNITY_4_LATE
#endif

#if UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3
#define UNITY_OLD_PARTICLES
#endif

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SECTR_TerrainWindow : SECTR_Window
{
	#region Private Details
	private Vector2 scrollPosition;
	private string sectorSearch = "";
	private Terrain selectedTerrain = null;
	private int sectorsWidth = 4;
	private int sectorsHeight = 1;
	private int sectorsLength = 4;
	private bool sectorizeConnected = false;
	private bool splitTerrain = false;
	private bool createPortalGeo = false;
	private bool groupStaticObjects = false;
	private bool groupDynamicObjects = false;
	#endregion

	#region Public Interface
	public static void SectorizeTerrain(Terrain terrain, int sectorsWidth, int sectorsLength, int sectorsHeight, bool splitTerrain, bool createPortalGeo, bool includeStatic, bool includeDynamic)
	{
		if(!terrain)
		{
			Debug.LogWarning("Cannot sectorize null terrain.");
			return;
		}

		if(terrain.transform.root.GetComponentsInChildren<SECTR_Sector>().Length > 0)
		{
			Debug.LogWarning("Cannot sectorize terrain that is already part of a Sector."); 
		}

		string undoString = "Sectorized " + terrain.name;

		if(sectorsWidth == 1 && sectorsLength == 1)
		{
			SECTR_Sector newSector = terrain.gameObject.AddComponent<SECTR_Sector>();
			SECTR_Undo.Created(newSector, undoString);
			newSector.ForceUpdate(true);
			return;
		}

		if(splitTerrain && (!Mathf.IsPowerOfTwo(sectorsWidth) || !Mathf.IsPowerOfTwo(sectorsLength)))
		{
			Debug.LogWarning("Splitting terrain requires power of two sectors in width and length.");
			splitTerrain = false;
		}
		else if(splitTerrain && sectorsWidth != sectorsLength)
		{
			Debug.LogWarning("Splitting terrain requires same number of sectors in width and length.");
			splitTerrain = false;
		}

		int terrainLayer = terrain.gameObject.layer;
		Vector3 terrainSize = terrain.terrainData.size;
		float sectorWidth = terrainSize.x / sectorsWidth;
		float sectorHeight = terrainSize.y / sectorsHeight;
		float sectorLength = terrainSize.z / sectorsLength;

		int heightmapWidth = (terrain.terrainData.heightmapWidth / sectorsWidth);
		int heightmapLength = (terrain.terrainData.heightmapHeight / sectorsLength);
		int alphaWidth = terrain.terrainData.alphamapWidth / sectorsWidth;
		int alphaLength = terrain.terrainData.alphamapHeight / sectorsLength;
		int detailWidth = terrain.terrainData.detailWidth / sectorsWidth;
		int detailLength = terrain.terrainData.detailHeight / sectorsLength;

		string sceneDir = "";
		string sceneName = "";
		string exportFolder = splitTerrain ? SECTR_Asset.MakeExportFolder("TerrainSplits", false, out sceneDir, out sceneName) : "";

		Transform baseTransform = null;
		if(splitTerrain)
		{
			GameObject baseObject = new GameObject(terrain.name);
			baseTransform = baseObject.transform;
			SECTR_Undo.Created(baseObject, undoString);
		}

		List<Transform> rootTransforms = new List<Transform>();
		List<Bounds> rootBounds = new List<Bounds>();
		_GetRoots(includeStatic, includeDynamic, rootTransforms, rootBounds);

		// Create Sectors
		string progressTitle = "Sectorizing Terrain";
		int progressCounter = 0;
		EditorUtility.DisplayProgressBar(progressTitle, "Preparing", 0);

		SECTR_Sector[,,] newSectors = new SECTR_Sector[sectorsWidth,sectorsLength,sectorsHeight];
		Terrain[,] newTerrains = splitTerrain ? new Terrain[sectorsWidth,sectorsLength] : null;
		for(int widthIndex = 0; widthIndex < sectorsWidth; ++widthIndex)
		{
			for(int lengthIndex = 0; lengthIndex < sectorsLength; ++lengthIndex)
			{
				for(int heightIndex = 0; heightIndex < sectorsHeight; ++heightIndex)
				{
					string newName = terrain.name + " " + widthIndex + "-" + lengthIndex + "-" + heightIndex;

					EditorUtility.DisplayProgressBar(progressTitle, "Creating sector " + newName, progressCounter++ / (float)(sectorsWidth * sectorsLength * sectorsHeight));

					GameObject newSectorObject = new GameObject("SECTR " + newName + " Sector");
					newSectorObject.transform.parent = baseTransform;
					Vector3 sectorCorner = new Vector3(widthIndex * sectorWidth,
						heightIndex * sectorHeight,
						lengthIndex * sectorLength) + terrain.transform.position;
					newSectorObject.transform.position = sectorCorner;
					newSectorObject.isStatic = true;
					SECTR_Sector newSector = newSectorObject.AddComponent<SECTR_Sector>();
					newSector.OverrideBounds = !splitTerrain && (sectorsWidth > 1 || sectorsLength > 1);
					newSector.BoundsOverride = new Bounds(sectorCorner + new Vector3(sectorWidth * 0.5f, sectorHeight * 0.5f, sectorLength * 0.5f),
						new Vector3(sectorWidth, sectorHeight, sectorLength));
					newSectors[widthIndex,lengthIndex,heightIndex] = newSector;

					if(splitTerrain && heightIndex == 0)
					{
						GameObject newTerrainObject = new GameObject(newName + " Terrain");
						newTerrainObject.layer = terrainLayer;
						newTerrainObject.tag = terrain.tag;
						newTerrainObject.transform.parent = newSectorObject.transform;
						newTerrainObject.transform.localPosition = Vector3.zero;
						newTerrainObject.transform.localRotation = Quaternion.identity;
						newTerrainObject.transform.localScale = Vector3.one;
						newTerrainObject.isStatic = true;
						Terrain newTerrain = newTerrainObject.AddComponent<Terrain>();
						newTerrain.terrainData = SECTR_Asset.Create<TerrainData>(exportFolder, newName, new TerrainData());
						EditorUtility.SetDirty(newTerrain.terrainData);
						SECTR_VC.WaitForVC();

						// Copy properties
						// Basic terrain properties
						newTerrain.editorRenderFlags = terrain.editorRenderFlags;
						newTerrain.castShadows = terrain.castShadows;
						newTerrain.heightmapMaximumLOD = terrain.heightmapMaximumLOD;
						newTerrain.heightmapPixelError = terrain.heightmapPixelError;
						newTerrain.lightmapIndex = -1; // Can't set lightmap UVs on terrain.
						newTerrain.materialTemplate = terrain.materialTemplate;
						#if !UNITY_4
						newTerrain.bakeLightProbesForTrees = terrain.bakeLightProbesForTrees;
						newTerrain.legacyShininess = terrain.legacyShininess;
						newTerrain.legacySpecular = terrain.legacySpecular;
						#endif

						// Copy geometric data
						int heightmapBaseX = widthIndex * heightmapWidth;
						int heightmapBaseY = lengthIndex * heightmapLength;
						int heightmapWidthX = heightmapWidth + (sectorsWidth > 1 ? 1 : 0);
						int heightmapWidthY = heightmapLength + (sectorsLength > 1 ? 1 : 0);	
						newTerrain.terrainData.heightmapResolution = terrain.terrainData.heightmapResolution / sectorsWidth;
						newTerrain.terrainData.size = new Vector3(sectorWidth, terrainSize.y, sectorLength);
						newTerrain.terrainData.SetHeights(0, 0, terrain.terrainData.GetHeights(heightmapBaseX, heightmapBaseY, heightmapWidthX, heightmapWidthY));
						#if !UNITY_4
						newTerrain.terrainData.thickness = terrain.terrainData.thickness;
						#endif

						// Copy alpha maps
						int alphaBaseX = alphaWidth * widthIndex;
						int alphaBaseY = alphaLength * lengthIndex;
						newTerrain.terrainData.splatPrototypes = terrain.terrainData.splatPrototypes;
						newTerrain.basemapDistance = terrain.basemapDistance;
						newTerrain.terrainData.baseMapResolution = terrain.terrainData.baseMapResolution / sectorsWidth;
						newTerrain.terrainData.alphamapResolution = terrain.terrainData.alphamapResolution / sectorsWidth;
						newTerrain.terrainData.SetAlphamaps(0, 0, terrain.terrainData.GetAlphamaps(alphaBaseX, alphaBaseY, alphaWidth, alphaLength));

						// Copy detail info
						newTerrain.detailObjectDensity = terrain.detailObjectDensity;
						newTerrain.detailObjectDistance = terrain.detailObjectDistance;
						newTerrain.terrainData.detailPrototypes = terrain.terrainData.detailPrototypes;
						newTerrain.terrainData.SetDetailResolution(terrain.terrainData.detailResolution / sectorsWidth, 8); // TODO: extract detailResolutionPerPatch
						#if !UNITY_4
						newTerrain.collectDetailPatches = terrain.collectDetailPatches;
						#endif

						int detailBaseX = detailWidth * widthIndex;
						int detailBaseY = detailLength * lengthIndex;
						int numLayers = terrain.terrainData.detailPrototypes.Length;
						for(int layer = 0; layer < numLayers; ++layer)
						{
							newTerrain.terrainData.SetDetailLayer(0, 0, layer, terrain.terrainData.GetDetailLayer(detailBaseX, detailBaseY, detailWidth, detailLength, layer)); 
						}

						// Copy grass and trees
						newTerrain.terrainData.wavingGrassAmount = terrain.terrainData.wavingGrassAmount;
						newTerrain.terrainData.wavingGrassSpeed = terrain.terrainData.wavingGrassSpeed;
						newTerrain.terrainData.wavingGrassStrength = terrain.terrainData.wavingGrassStrength;
						newTerrain.terrainData.wavingGrassTint = terrain.terrainData.wavingGrassTint;
						newTerrain.treeBillboardDistance = terrain.treeBillboardDistance;
						newTerrain.treeCrossFadeLength = terrain.treeCrossFadeLength;
						newTerrain.treeDistance = terrain.treeDistance;
						newTerrain.treeMaximumFullLODCount = terrain.treeMaximumFullLODCount;
						newTerrain.terrainData.treePrototypes = terrain.terrainData.treePrototypes;
						newTerrain.terrainData.RefreshPrototypes();

						foreach(TreeInstance treeInstace in terrain.terrainData.treeInstances)
						{
							if(treeInstace.prototypeIndex >= 0 && treeInstace.prototypeIndex < newTerrain.terrainData.treePrototypes.Length &&
								newTerrain.terrainData.treePrototypes[treeInstace.prototypeIndex].prefab)
							{
								Vector3 worldSpaceTreePos = Vector3.Scale(treeInstace.position, terrainSize) + terrain.transform.position;
								if(newSector.BoundsOverride.Contains(worldSpaceTreePos))
								{
									Vector3 localSpaceTreePos = new Vector3((worldSpaceTreePos.x - newTerrain.transform.position.x) / sectorWidth,
										treeInstace.position.y,
										(worldSpaceTreePos.z - newTerrain.transform.position.z) / sectorLength);
									TreeInstance newInstance = treeInstace;
									newInstance.position = localSpaceTreePos;
									newTerrain.AddTreeInstance(newInstance);
								}
							}
						}

						// Copy physics
						#if UNITY_4_LATE
						newTerrain.terrainData.physicMaterial = terrain.terrainData.physicMaterial;
						#endif

						// Force terrain to rebuild
						newTerrain.Flush();

						UnityEditor.EditorUtility.SetDirty(newTerrain.terrainData);
						SECTR_VC.WaitForVC();
						newTerrain.enabled = false;
						newTerrain.enabled = true;

						TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
						if(terrainCollider)
						{
							TerrainCollider newCollider = newTerrainObject.AddComponent<TerrainCollider>();	
							#if !UNITY_4_LATE
							newCollider.sharedMaterial = terrainCollider.sharedMaterial;
							#endif
							newCollider.terrainData = newTerrain.terrainData;
						}

						newTerrains[widthIndex,lengthIndex] = newTerrain;
						SECTR_Undo.Created(newTerrainObject, undoString);
					}
					newSector.ForceUpdate(true);
					SECTR_Undo.Created(newSectorObject, undoString);

					_Encapsulate(newSector, rootTransforms, rootBounds, undoString);
				}
			}
		}

		// Create portals and neighbors
		progressCounter = 0;
		for(int widthIndex = 0; widthIndex < sectorsWidth; ++widthIndex)
		{
			for(int lengthIndex = 0; lengthIndex < sectorsLength; ++lengthIndex)
			{
				for(int heightIndex = 0; heightIndex < sectorsHeight; ++heightIndex)
				{
					EditorUtility.DisplayProgressBar(progressTitle, "Creating portals...", progressCounter++ / (float)(sectorsWidth * sectorsLength * sectorsHeight));

					if(widthIndex < sectorsWidth - 1)
					{
						_CreatePortal(createPortalGeo, newSectors[widthIndex + 1, lengthIndex, heightIndex], newSectors[widthIndex, lengthIndex, heightIndex], baseTransform, undoString);
					}

					if(lengthIndex < sectorsLength - 1)
					{
						_CreatePortal(createPortalGeo, newSectors[widthIndex, lengthIndex + 1, heightIndex], newSectors[widthIndex, lengthIndex, heightIndex], baseTransform, undoString);
					}

					if(heightIndex > 0)						
					{
						_CreatePortal(createPortalGeo, newSectors[widthIndex, lengthIndex, heightIndex], newSectors[widthIndex, lengthIndex, heightIndex - 1], baseTransform, undoString);
					}
				}
			}
		}

		if(splitTerrain)
		{
			progressCounter = 0;
			for(int widthIndex = 0; widthIndex < sectorsWidth; ++widthIndex)
			{
				for(int lengthIndex = 0; lengthIndex < sectorsLength; ++lengthIndex)
				{
					EditorUtility.DisplayProgressBar(progressTitle, "Smoothing split terrain...", progressCounter++ / (float)(sectorsWidth * sectorsLength * sectorsHeight));

					// Blend together the seams of the alpha maps, which requires
					// going through all of the mip maps of all of the layer textures.
					// We have to blend here rather than when we set the alpha data (above)
					// because Unity computes mips and we need to blend all of the mips.
					Terrain newTerrain = newTerrains[widthIndex, lengthIndex];

					SECTR_Sector terrainSector = newSectors[widthIndex, lengthIndex, 0];
					terrainSector.LeftTerrain = widthIndex > 0 ? newSectors[widthIndex - 1, lengthIndex, 0] : null;
					terrainSector.RightTerrain = widthIndex < sectorsWidth - 1 ? newSectors[widthIndex + 1, lengthIndex, 0] : null;
					terrainSector.BottomTerrain = lengthIndex > 0 ? newSectors[widthIndex, lengthIndex - 1, 0] : null;
					terrainSector.TopTerrain = lengthIndex < sectorsLength - 1 ? newSectors[widthIndex, lengthIndex + 1, 0] : null;
					terrainSector.ConnectTerrainNeighbors();

					// Use reflection trickery to get at the raw texture values.
					System.Reflection.PropertyInfo alphamapProperty = newTerrain.terrainData.GetType().GetProperty("alphamapTextures",
						System.Reflection.BindingFlags.NonPublic | 
						System.Reflection.BindingFlags.Public |
						System.Reflection.BindingFlags.Instance |
						System.Reflection.BindingFlags.Static);
					// Get the texture we'll write into
					Texture2D[] alphaTextures = (Texture2D[])alphamapProperty.GetValue(newTerrain.terrainData, null);
					int numTextures = alphaTextures.Length;

					// Get the textures we'll read from
					Texture2D[] leftNeighborTextures = terrainSector.LeftTerrain != null ? (Texture2D[])alphamapProperty.GetValue(newTerrains[widthIndex - 1, lengthIndex].terrainData, null) : null;
					Texture2D[] rightNeighborTextures = terrainSector.RightTerrain != null ? (Texture2D[])alphamapProperty.GetValue(newTerrains[widthIndex + 1, lengthIndex].terrainData, null) : null;
					Texture2D[] topNeighborTextures = terrainSector.TopTerrain != null ? (Texture2D[])alphamapProperty.GetValue(newTerrains[widthIndex, lengthIndex + 1].terrainData, null) : null;
					Texture2D[] bottomNeighborTextures = terrainSector.BottomTerrain != null ? (Texture2D[])alphamapProperty.GetValue(newTerrains[widthIndex, lengthIndex - 1].terrainData, null) : null;

					for(int textureIndex = 0; textureIndex < numTextures; ++textureIndex)
					{
						Texture2D alphaTexture = alphaTextures[textureIndex];
						Texture2D leftTexture = leftNeighborTextures != null ? leftNeighborTextures[textureIndex] : null;
						Texture2D rightTexture = rightNeighborTextures != null ? rightNeighborTextures[textureIndex] : null;
						Texture2D topTexture = topNeighborTextures != null ? topNeighborTextures[textureIndex] : null;
						Texture2D bottomTexture = bottomNeighborTextures != null ? bottomNeighborTextures[textureIndex] : null;
						int numMips = alphaTexture.mipmapCount;
						for(int mipIndex = 0; mipIndex < numMips; ++mipIndex)
						{
							Color[] alphaTexels = alphaTexture.GetPixels(mipIndex);
							int width = (int)Mathf.Sqrt(alphaTexels.Length);
							int height = width;
							for(int texelWidthIndex = 0; texelWidthIndex < width; ++texelWidthIndex)
							{
								for(int texelHeightIndex = 0; texelHeightIndex < height; ++texelHeightIndex)
								{
									// We can take advantage of the build order to average on the leading edges (right and top)
									// and then copy form the trailing edges (left and bottom)
									if(texelWidthIndex == 0 && leftTexture)
									{
										Color[] neighborTexels = leftTexture.GetPixels(mipIndex);
										alphaTexels[texelWidthIndex + texelHeightIndex * width] = neighborTexels[(width - 1) + (texelHeightIndex * width)];
									}
									else if(texelWidthIndex == width - 1 && rightTexture)
									{
										Color[] neighborTexels = rightTexture.GetPixels(mipIndex);
										alphaTexels[texelWidthIndex + texelHeightIndex * width] += neighborTexels[0 + (texelHeightIndex * width)];
										alphaTexels[texelWidthIndex + texelHeightIndex * width] *= 0.5f;
									}
									else if(texelHeightIndex == 0 && bottomTexture)
									{
										Color[] neighborTexels = bottomTexture.GetPixels(mipIndex);
										alphaTexels[texelWidthIndex + texelHeightIndex * width] = neighborTexels[texelWidthIndex + ((height - 1) * width)];
									}
									else if(texelHeightIndex == height - 1 && topTexture)
									{
										Color[] neighborTexels = topTexture.GetPixels(mipIndex);
										alphaTexels[texelWidthIndex + texelHeightIndex * width] += neighborTexels[texelWidthIndex + (0 * width)];
										alphaTexels[texelWidthIndex + texelHeightIndex * width] *= 0.5f;
									}
								}
							}
							alphaTexture.SetPixels(alphaTexels, mipIndex);
						}
						alphaTexture.wrapMode = TextureWrapMode.Clamp;
						alphaTexture.Apply(false);
					}

					newTerrain.Flush();
				}
			}
		}

		EditorUtility.ClearProgressBar();

		// destroy original terrain
		if(splitTerrain)
		{
			SECTR_Undo.Destroy(terrain.gameObject, undoString);
		}
	}

	public static void SectorizeConnected(Terrain terrain, bool createPortalGeo, bool includeStatic, bool includeDynamic)
	{
		Dictionary<Terrain, Terrain> processedTerrains = new Dictionary<Terrain, Terrain>();
		List<Transform> rootTransforms = new List<Transform>();
		List<Bounds> rootBounds = new List<Bounds>();
		_GetRoots(includeStatic, includeDynamic, rootTransforms, rootBounds);
		_SectorizeConnected(terrain, createPortalGeo, includeStatic, includeDynamic, processedTerrains, rootTransforms, rootBounds);
	}
	#endregion

	#region Unity Interface
	protected override void OnGUI()
	{
		base.OnGUI();

		Terrain[] terrains = (Terrain[])GameObject.FindObjectsOfType(typeof(Terrain));
		int numTerrains = terrains.Length;
		bool sceneHasTerrains = numTerrains > 0;
		bool selectedInSector = false;
		bool hasTerrainComposer = false;

		EditorGUILayout.BeginVertical();
		DrawHeader("TERRAINS", ref sectorSearch, 100, true);
		Rect r = EditorGUILayout.BeginVertical();
		r.y -= lineHeight;
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		bool wasEnabled = GUI.enabled;
		GUI.enabled = false;
		GUI.Button(r, sceneHasTerrains ? "" : "Current Scene Has No Terrains");
		GUI.enabled = wasEnabled;
		Terrain newSelectedTerrain = Selection.activeGameObject ?  Selection.activeGameObject.GetComponent<Terrain>() : null;
		if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
		{
			newSelectedTerrain = null;
		}

		for(int terrainIndex = 0; terrainIndex < numTerrains; ++terrainIndex)
		{
			Terrain terrain = terrains[terrainIndex];
			if(terrain.name.ToLower().Contains(sectorSearch.ToLower()))
			{
				bool selected = terrain == selectedTerrain;
				bool inSector = false;
				Transform parent = terrain.transform;
				while(parent != null)
				{
					if(parent.GetComponent<SECTR_Sector>())
					{
						inSector = true;
						if(selected)
						{
							selectedInSector = true;
						}
						break;
					}
					parent = parent.parent;
				}

				hasTerrainComposer |= terrain.GetComponent("TerrainNeighbors") != null;

				Rect clipRect = EditorGUILayout.BeginHorizontal();
				if(selected)
				{
					Rect selectionRect = clipRect;
					selectionRect.y += 1;
					selectionRect.height += 1;
					GUI.Box(selectionRect, "", selectionBoxStyle);
				}

				GUILayout.FlexibleSpace();
				elementStyle.normal.textColor = selected ? Color.white : UnselectedItemColor;
				elementStyle.alignment = TextAnchor.MiddleCenter;
				EditorGUILayout.LabelField(terrain.name, elementStyle);
				GUILayout.FlexibleSpace();

				EditorGUILayout.EndHorizontal();

				if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && 
					clipRect.Contains(Event.current.mousePosition) )
				{
					newSelectedTerrain = terrain;
					selectedInSector = inSector;
				}
			}
		}
		EditorGUILayout.EndScrollView();
		EditorGUILayout.EndVertical();

		bool doRepaint = false;
		if(newSelectedTerrain != selectedTerrain && SceneView.lastActiveSceneView)
		{
			selectedTerrain = newSelectedTerrain;
			Selection.activeGameObject = selectedTerrain ? selectedTerrain.gameObject : null;
			SceneView.lastActiveSceneView.FrameSelected();
			doRepaint = true;
		}

		bool sectorizableSelection = sceneHasTerrains && selectedTerrain != null && !selectedInSector;
		string nullSearch = null;

		DrawHeader("SETTINGS", ref nullSearch, 0, true);

		EditorGUILayout.BeginVertical();
		sectorsWidth = EditorGUILayout.IntField(new GUIContent("Sectors Width", "Number of Sectors to create across terrain width."), sectorsWidth);
		sectorsWidth = Mathf.Max(sectorsWidth, 1);
		sectorsLength = EditorGUILayout.IntField(new GUIContent("Sectors Length", "Number of Sectors to create across terrain length."), sectorsLength);
		sectorsLength = Mathf.Max(sectorsLength, 1);
		sectorsHeight = EditorGUILayout.IntField(new GUIContent("Sectors Height", "Number of Sectors to create across terrain height."), sectorsHeight);
		sectorsHeight = Mathf.Max(sectorsHeight, 1);

		if(hasTerrainComposer)
		{
			sectorizeConnected = EditorGUILayout.Toggle(new GUIContent("Include Connected", "Sectorizes all terrains directly or indirectly connected to selected terrain."), sectorizeConnected);
		}

		bool canSplitTerrain = selectedTerrain != null &&
			sectorsWidth > 1 && sectorsLength > 1 && sectorsWidth == sectorsLength &&
			Mathf.IsPowerOfTwo(sectorsWidth) && Mathf.IsPowerOfTwo(sectorsLength) &&
			(selectedTerrain.terrainData.heightmapResolution - 1) / sectorsWidth >= 32;
		splitTerrain = EditorGUILayout.Toggle(new GUIContent("Split Terrain", "Splits terrain into multiple objects (for streaming or culling)."), splitTerrain);
		#if !UNITY_4_0 && !UNITY_4_1
		createPortalGeo = EditorGUILayout.Toggle(new GUIContent("Create Mesh for Portals", "Creates a mesh for the portal for games that need it. Not required."), createPortalGeo);
		#endif
		groupStaticObjects = EditorGUILayout.Toggle(new GUIContent("Group Static Objects", "Make all static game objects on the terrain children of the Sector."), groupStaticObjects);
		groupDynamicObjects = EditorGUILayout.Toggle(new GUIContent("Group Dynamic Objects", "Make all dynamic game objects on the terrain children of the Sector."), groupDynamicObjects);
		EditorGUILayout.EndVertical();

		if(!selectedTerrain)
		{
			GUI.enabled = false;
			GUILayout.Button("Select Terrain To Sectorize");
			GUI.enabled = true;
		}
		else if(!sectorizableSelection && selectedInSector)
		{
			GUI.enabled = false;
			GUILayout.Button("Cannot Sectorize Terrain That Is Already In a Sector");
			GUI.enabled = false;
		}
		else if(sectorizeConnected && splitTerrain)
		{
			GUI.enabled = false;
			GUILayout.Button("Cannot both Split and Sectorize Connected Terrains");
			GUI.enabled = false;
		}
		else if(sectorizeConnected && (sectorsWidth != 1 || sectorsLength != 1 || sectorsHeight != 1))
		{
			GUI.enabled = false;
			GUILayout.Button("Width/Length/Height Must be 1 to Sectorize Connected Terrains");
			GUI.enabled = false;
		}
		else if(splitTerrain && sectorsWidth != sectorsLength)
		{
			GUI.enabled = false;
			GUILayout.Button("Cannot split terrain unless Sectors Width and Length match.");
			GUI.enabled = true;
		}
		else if(splitTerrain && !Mathf.IsPowerOfTwo(sectorsWidth))
		{
			GUI.enabled = false;
			GUILayout.Button("Cannot split terrain unless Sectors Width and Length are powers of 2.");
			GUI.enabled = true;
		}
		else if(splitTerrain && (selectedTerrain.terrainData.heightmapResolution - 1) / sectorsWidth < 32)
		{
			GUI.enabled = false;
			GUILayout.Button("Cannot split terrain into chunks less than 32 x 32.");
			GUI.enabled = true;
		}
		else if(GUILayout.Button("Sectorize Terrain"))
		{
			if(sectorizeConnected)
			{
				SectorizeConnected(selectedTerrain, createPortalGeo, groupStaticObjects, groupDynamicObjects);
				doRepaint = true;
			}
			else if(!splitTerrain || selectedTerrain.lightmapIndex < 0 || LightmapSettings.lightmaps.Length == 0 || EditorUtility.DisplayDialog("Lightmap Warning", "Splitting terrain will not preserve lightmaps. They will need to be rebaked. Continue sectorization?", "Yes", "No"))
			{
				SectorizeTerrain(selectedTerrain, sectorsWidth, sectorsLength, sectorsHeight, canSplitTerrain && splitTerrain, createPortalGeo, groupStaticObjects, groupDynamicObjects);
				doRepaint = true;
			}
		}
		GUI.enabled = wasEnabled;

		EditorGUILayout.EndVertical();

		if(doRepaint)
		{
			Repaint();
		}
	}
	#endregion

	#region Private Interface
	private static void _CreatePortal(bool createGeo, SECTR_Sector front, SECTR_Sector back, Transform parent, string undoString)
	{
		if(front && back)
		{
			string portalName = "SECTR Terrain Portal";
			GameObject newPortalObject;
			SECTR_Portal newPortal;
			#if !UNITY_4_0 && !UNITY_4_1
			if(createGeo)
			{
				newPortalObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
				newPortalObject.name = portalName;
				Mesh quadResource = newPortalObject.GetComponent<MeshFilter>().sharedMesh;
				GameObject.DestroyImmediate(newPortalObject.GetComponent<MeshFilter>());
				GameObject.DestroyImmediate(newPortalObject.GetComponent<MeshRenderer>());
				GameObject.DestroyImmediate(newPortalObject.GetComponent<Collider>());
				newPortal = newPortalObject.AddComponent<SECTR_Portal>();
				newPortal.HullMesh = quadResource;
			}
			else
			#endif
			{
				newPortalObject = new GameObject(portalName);
				newPortal = newPortalObject.AddComponent<SECTR_Portal>();
			}
			newPortal.SetFlag(SECTR_Portal.PortalFlags.PassThrough, true);
			newPortal.FrontSector = front;
			newPortal.BackSector = back;
			newPortal.transform.parent = parent;
			newPortal.transform.position = (front.TotalBounds.center + back.TotalBounds.center) * 0.5f;
			if(createGeo)
			{
				newPortal.transform.LookAt(back.TotalBounds.center);
				Vector3 orientation = newPortal.transform.forward;
				if(Mathf.Abs(orientation.x) >= Mathf.Abs(orientation.y) && Mathf.Abs(orientation.x) >= Mathf.Abs(orientation.z))
				{
					newPortal.transform.localScale = new Vector3(front.TotalBounds.size.z, front.TotalBounds.size.y, 1f);
				}
				else if(Mathf.Abs(orientation.y) >= Mathf.Abs(orientation.x) && Mathf.Abs(orientation.y) >= Mathf.Abs(orientation.z))
				{
					newPortal.transform.localScale = new Vector3(front.TotalBounds.size.x, front.TotalBounds.size.z, 1f);
				}
				else if(Mathf.Abs(orientation.z) >= Mathf.Abs(orientation.x) && Mathf.Abs(orientation.z) >= Mathf.Abs(orientation.y))
				{
					newPortal.transform.localScale = new Vector3(front.TotalBounds.size.x, front.TotalBounds.size.y, 1f);
				}
			}
			else
			{
				newPortal.transform.LookAt(front.TotalBounds.center);
			}
			SECTR_Undo.Created(newPortalObject, undoString);
		}
	}

	private static void _GetRoots(bool includeStatic, bool includeDynamic, List<Transform> rootTransforms, List<Bounds> rootBounds)
	{
		if(includeStatic || includeDynamic)
		{
			Transform[] allTransforms = (Transform[])GameObject.FindObjectsOfType(typeof(Transform));
			foreach(Transform transform in allTransforms)
			{
				if(transform.parent == null &&
					((transform.gameObject.isStatic && includeStatic) || !transform.gameObject.isStatic && includeDynamic))
				{
					rootTransforms.Add(transform);
					Bounds aggregateBounds = new Bounds();
					bool initBounds = false;
					Renderer[] childRenderers = transform.GetComponentsInChildren<Renderer>();
					foreach(Renderer renderer in childRenderers)
					{
						Bounds renderBounds = renderer.bounds;

						// Particle bounds are unreliable in editor, so use a unit sized box as a proxy.
						if(renderer.GetType() == typeof(ParticleSystemRenderer)
							#if UNITY_OLD_PARTICLES
							|| renderer.GetType() == typeof(ParticleRenderer)
							#endif
						)
						{
							renderBounds = new Bounds(transform.position, Vector3.one);
						}

						if(!initBounds)
						{
							aggregateBounds = renderBounds;
							initBounds = true;
						}
						else
						{
							aggregateBounds.Encapsulate(renderBounds);
						}
					}
					Light[] childLights = transform.GetComponentsInChildren<Light>();
					foreach(Light light in childLights)
					{
						if(!initBounds)
						{
							aggregateBounds = SECTR_Geometry.ComputeBounds(light);
							initBounds = true;
						}
						else
						{
							aggregateBounds.Encapsulate(SECTR_Geometry.ComputeBounds(light));
						}
					}
					rootBounds.Add(aggregateBounds);
				}
			}
		}
	}

	private static void _Encapsulate(SECTR_Sector newSector, List<Transform> rootTransforms, List<Bounds> rootBounds, string undoString)
	{
		int numRoots = rootTransforms.Count;
		for(int rootIndex = numRoots - 1; rootIndex >= 0; --rootIndex)
		{
			Transform rootTransform = rootTransforms[rootIndex];
			if(rootTransform != newSector.transform && SECTR_Geometry.BoundsContainsBounds(newSector.TotalBounds, rootBounds[rootIndex]))
			{
				SECTR_Undo.Parent(newSector.gameObject, rootTransform.gameObject, undoString);
				rootTransforms.RemoveAt(rootIndex);
				rootBounds.RemoveAt(rootIndex);
			}
		}
	}

	private static void _SectorizeConnected(Terrain terrain, bool createPortalGeo, bool includeStatic, bool includeDynamic, Dictionary<Terrain, Terrain> processedTerrains, List<Transform> rootTransforms, List<Bounds> rootBounds)
	{
		if(terrain && !processedTerrains.ContainsKey(terrain))
		{
			string undoString = "Sectorize Connected";
			processedTerrains[terrain] = terrain;
			terrain.gameObject.isStatic = true;
			GameObject newSectorObject = new GameObject(terrain.name + " Sector");
			newSectorObject.isStatic = true;
			newSectorObject.transform.parent = terrain.transform.parent;
			newSectorObject.transform.localPosition = terrain.transform.localPosition;
			newSectorObject.transform.localRotation = terrain.transform.localRotation;
			newSectorObject.transform.localScale = terrain.transform.localScale;
			terrain.transform.parent = newSectorObject.transform;
			SECTR_Sector newSector = newSectorObject.AddComponent<SECTR_Sector>();
			newSector.ForceUpdate(true);
			SECTR_Undo.Created(newSectorObject, undoString);
			_Encapsulate(newSector, rootTransforms, rootBounds, undoString);

			Component terrainNeighbors = terrain.GetComponent("TerrainNeighbors");
			if(terrainNeighbors)
			{
				System.Type neighborsType = terrainNeighbors.GetType();
				Terrain topTerrain = neighborsType.GetField("top").GetValue(terrainNeighbors) as Terrain;
				if(topTerrain)
				{
					SECTR_Sector neighborSector = topTerrain.transform.parent ? topTerrain.transform.parent.GetComponent<SECTR_Sector>() : null;
					if(neighborSector)
					{
						newSector.TopTerrain = neighborSector;
						neighborSector.BottomTerrain = newSector;
						_CreatePortal(createPortalGeo, newSector, neighborSector, newSectorObject.transform.parent, undoString);
					}
					_SectorizeConnected(topTerrain, createPortalGeo, includeStatic, includeDynamic, processedTerrains, rootTransforms, rootBounds);
				}
				Terrain bottomTerrain = neighborsType.GetField("bottom").GetValue(terrainNeighbors) as Terrain;
				if(bottomTerrain)
				{
					SECTR_Sector neighborSector = bottomTerrain.transform.parent ? bottomTerrain.transform.parent.GetComponent<SECTR_Sector>() : null;
					if(neighborSector)
					{
						newSector.BottomTerrain = neighborSector;
						neighborSector.TopTerrain = newSector;
						_CreatePortal(createPortalGeo, newSector, neighborSector, newSectorObject.transform.parent, undoString);
					}
					_SectorizeConnected(bottomTerrain, createPortalGeo, includeStatic, includeDynamic, processedTerrains, rootTransforms, rootBounds);
				}
				Terrain leftTerrain = neighborsType.GetField("left").GetValue(terrainNeighbors) as Terrain;
				if(leftTerrain)
				{
					SECTR_Sector neighborSector = leftTerrain.transform.parent ? leftTerrain.transform.parent.GetComponent<SECTR_Sector>() : null;
					if(neighborSector)
					{
						newSector.LeftTerrain = neighborSector;
						neighborSector.RightTerrain = newSector;
						_CreatePortal(createPortalGeo, newSector, neighborSector, newSectorObject.transform.parent, undoString);
					}
					_SectorizeConnected(leftTerrain, createPortalGeo, includeStatic, includeDynamic, processedTerrains, rootTransforms, rootBounds);
				}
				Terrain rightTerrain = neighborsType.GetField("right").GetValue(terrainNeighbors) as Terrain;
				if(rightTerrain)
				{
					SECTR_Sector neighborSector = rightTerrain.transform.parent ? rightTerrain.transform.parent.GetComponent<SECTR_Sector>() : null;
					if(neighborSector)
					{
						newSector.RightTerrain = neighborSector;
						neighborSector.LeftTerrain = newSector;
						_CreatePortal(createPortalGeo, newSector, neighborSector, newSectorObject.transform.parent, undoString);
					}
					_SectorizeConnected(rightTerrain, createPortalGeo, includeStatic, includeDynamic, processedTerrains, rootTransforms, rootBounds);
				}
			}
		}
	}
	#endregion
}
