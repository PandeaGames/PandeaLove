// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SECTR_Chunk))]
[CanEditMultipleObjects]
public class SECTR_ChunkEditor : SECTR_Editor
{
	enum ModifierMode
	{
		None,
		Import,
		Export,
		Revert,
	}

	private Dictionary<Renderer, bool> checkState = new Dictionary<Renderer, bool>();
	private Dictionary<Transform, bool> hierarchyFoldouts = new Dictionary<Transform, bool>();
	private bool proxyFoldout = false;
	private ModifierMode modiferMode = ModifierMode.None;

	public override void OnInspectorGUI()
	{
		SECTR_Chunk myChunk = (SECTR_Chunk)target;
		SECTR_Sector mySector = myChunk.GetComponent<SECTR_Sector>();

		EditorGUILayout.BeginHorizontal();
		bool editMode = !EditorApplication.isPlaying && !EditorApplication.isPaused;
		bool alreadyExported = myChunk && System.IO.File.Exists(SECTR_Asset.UnityToOSPath(myChunk.NodeName));
		GUI.enabled = editMode;
		if(mySector.Frozen)
		{
			// Import
			if(alreadyExported && 
			   GUILayout.Button(new GUIContent("Import", "Imports this Sector into the scene.")))
			{
				modiferMode = ModifierMode.Import;
			}
			// Export
			GUI.enabled = false;
			GUILayout.Button(new GUIContent("Export", "Exports this Sector into a Chunk scene."));
			GUI.enabled = editMode;
		}
		else
		{
			// Revert
			if(alreadyExported && 
			   GUILayout.Button(new GUIContent("Revert", "Discards changes to this Sector.")))
			{
				modiferMode = ModifierMode.Revert;
			}
			// Export
			if(GUILayout.Button(new GUIContent("Export", "Exports this Sector into a Chunk scene.")))
			{
				modiferMode = ModifierMode.Export;
			}
		}
		EditorGUILayout.EndHorizontal();

		base.OnInspectorGUI();

		if(!mySector.Frozen)
		{
			proxyFoldout = EditorGUILayout.Foldout(proxyFoldout, "Proxy Mesh Tool");
			if(proxyFoldout)
			{
				EditorGUILayout.BeginVertical();

				_BuildChildControls(myChunk.transform, true);

				if(GUILayout.Button("Create Proxy Mesh"))
				{
					Dictionary<Material, List<CombineInstance>> meshHash = new Dictionary<Material, List<CombineInstance>>();
					Matrix4x4 chunkWorldToLocal = myChunk.transform.worldToLocalMatrix;
					foreach(Renderer renderer in checkState.Keys)
					{
						if(checkState[renderer])
						{
							MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
							int numSubMeshes = meshFilter.sharedMesh.subMeshCount;
							for(int submeshIndex = 0; submeshIndex < numSubMeshes; ++submeshIndex)
							{
								Material material = renderer.sharedMaterials[submeshIndex];
								List<CombineInstance> materialMeshes = null;
								if(!meshHash.TryGetValue(material, out materialMeshes))
								{
									materialMeshes = new List<CombineInstance>();
									meshHash[material] = materialMeshes;
								}

								CombineInstance instance = new CombineInstance();
								instance.transform = chunkWorldToLocal * renderer.transform.localToWorldMatrix;
								instance.mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
								instance.subMeshIndex = submeshIndex;
								materialMeshes.Add(instance);
							}
						}
					}
					if(meshHash.Count > 0)
					{
						List<CombineInstance> combinedMeshes = new List<CombineInstance>();
						List<Material> combinedMaterials = new List<Material>();
						foreach(Material material in meshHash.Keys)
						{
							CombineInstance instance = new CombineInstance();
							instance.mesh = new Mesh();
							instance.mesh.CombineMeshes(meshHash[material].ToArray(), true, true);
							combinedMeshes.Add(instance);
							combinedMaterials.Add(material);
						}

						string sceneDir;
						string sceneName;
						string exportFolder = SECTR_Asset.MakeExportFolder("Proxies", false, out sceneDir, out sceneName);
						myChunk.ProxyMesh = SECTR_Asset.Create<Mesh>(exportFolder, myChunk.name + "_Proxy", new Mesh());
						myChunk.ProxyMesh.CombineMeshes(combinedMeshes.ToArray(), false, false);
						myChunk.ProxyMaterials = combinedMaterials.ToArray();

						int numCombined = combinedMeshes.Count;
						for(int combinedIndex = 0; combinedIndex < numCombined; ++combinedIndex)
						{
							Mesh.DestroyImmediate(combinedMeshes[combinedIndex].mesh);
						}
						SECTR_VC.WaitForVC();
					}
					else
					{
						EditorUtility.DisplayDialog("Proxy Error", "Must have at least one mesh selected to create a proxy.", "Ok");
					}
				}
				EditorGUILayout.EndVertical();
			}
		}
		GUI.enabled = true;

		if(modiferMode != ModifierMode.None)
		{
			EditorApplication.update += ExportUpdate;
		}
	}

	public void OnDisable()
	{
		if(modiferMode != ModifierMode.None)
		{
			EditorApplication.update -= ExportUpdate;
		}
	}

	private void ExportUpdate()
	{
		SECTR_Chunk myChunk = (SECTR_Chunk)target;
		if(myChunk)
		{
			SECTR_Sector mySector = myChunk.GetComponent<SECTR_Sector>();
			if(mySector)
			{
				switch(modiferMode)
				{
				case ModifierMode.Export:
					SECTR_StreamExport.ExportToChunk(mySector);
					break;
				case ModifierMode.Import:
					SECTR_StreamExport.ImportFromChunk(mySector);
					break;
				case ModifierMode.Revert:
					SECTR_StreamExport.RevertChunk(mySector);
					break;
				case ModifierMode.None:
				default:
					break;
				}
			}
			modiferMode = ModifierMode.None;
		}
		EditorApplication.update -= ExportUpdate;
	}

	private void _BuildChildControls(Transform transform, bool rootTransform)
	{
		if(transform)
		{
			int numChildren = transform.childCount;
			Renderer transformRenderer = transform.GetComponent<Renderer>();
			bool hasRenderer = 
				transformRenderer && 
				transformRenderer.GetType() == typeof(MeshRenderer) && 
				transformRenderer.GetComponent<MeshFilter>() &&
				transformRenderer.GetComponent<MeshFilter>().sharedMesh;

			if(numChildren > 0)
			{
				bool foldout = rootTransform;
				if(!rootTransform)
				{
					hierarchyFoldouts.TryGetValue(transform, out foldout);
					foldout = EditorGUILayout.Foldout(foldout, transform.name);
					hierarchyFoldouts[transform] = foldout;
				}
				if(foldout)
				{
					++EditorGUI.indentLevel;
					if(hasRenderer)
					{
						bool included = false;
						checkState.TryGetValue(transformRenderer, out included);
						checkState[transformRenderer] = EditorGUILayout.Toggle(transformRenderer.name, included);
					}

					for(int childIndex = 0; childIndex < numChildren; ++childIndex)
					{
						_BuildChildControls(transform.GetChild(childIndex), false);
					}
					--EditorGUI.indentLevel;
				}
			}
			else if(hasRenderer)
			{
				bool included = false;
				checkState.TryGetValue(transformRenderer, out included);
				checkState[transformRenderer] = EditorGUILayout.Toggle(transformRenderer.name, included);
			}
		}
	}
}
