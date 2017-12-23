// Copyright (c) 2014 Make Code Now! LLC

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
#define UNITY_4
#endif

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;

/// \ingroup Stream
/// Stores the references to lightmap textures in an exported Chunk.
/// 
/// This class is meant for internal use only and should not be added by
/// users, though every attempt has been made to ensure nothing bad happens
/// in the case that one is added accidentally.
[AddComponentMenu("")]
public class SECTR_LightmapRef : MonoBehaviour 
{
	#region Private Details
	[SerializeField] [HideInInspector]
	private List<RefData> lightmapRefs = new List<RefData>();

#if !UNITY_4
	[SerializeField] [HideInInspector]
	private List<RenderData> lightmapRenderRefs = new List<RenderData>();
#endif

	private static int[] globalLightmapRefCount;
	#endregion

	#region Public Interface
	[System.Serializable]
	public class RefData
	{
		public Texture2D FarLightmap = null;
		public Texture2D NearLightmap = null;
		public int index = -1;
	}

#if !UNITY_4
	[System.Serializable]
	public class RenderData
	{
		public Renderer renderer = null;
		public int rendererLightmapIndex = -1;
		public Vector4 rendererLightmapScaleOffset = Vector4.zero;
		public Terrain terrain = null;
		public int terrainLightmapIndex = -1;
	}
#endif

	/// Read-only accessor for the LightmapRefs. Intended primarily for debugging,
	/// and fixup during imports.
	public List<RefData> LightmapRefs { get { return lightmapRefs; } }

#if !UNITY_4
	/// Read-only accessor for the LightmapIndices.
	public List<RenderData> LightmapRenderers { get { return lightmapRenderRefs; } }
#endif

	/// Initializes the global/static lightmap ref count array. Can be called multiple
	/// times, but should only be called at the start of the level and only by
	/// SECTR_Chunk.
	public static void InitRefCounts()
	{
		int numLightmaps = LightmapSettings.lightmaps.Length;
		if(globalLightmapRefCount == null || globalLightmapRefCount.Length != numLightmaps)
		{
			globalLightmapRefCount = new int[numLightmaps];
		}
		for(int lightmapIndex = 0; lightmapIndex < numLightmaps; ++lightmapIndex)
		{
			LightmapData lightmap = LightmapSettings.lightmaps[lightmapIndex];
#if UNITY_2017_1_OR_NEWER
			globalLightmapRefCount[lightmapIndex] = (lightmap.lightmapColor || lightmap.lightmapDir) ? 1 : 0;
#elif UNITY_5_5_OR_NEWER
			globalLightmapRefCount[lightmapIndex] = (lightmap.lightmapLight || lightmap.lightmapDir) ? 1 : 0;
#else
			globalLightmapRefCount[lightmapIndex] = (lightmap.lightmapFar || lightmap.lightmapNear) ? 1 : 0;
#endif
		}
	}

#if UNITY_EDITOR
	public void ReferenceLightmaps(List<int> lightmapIndices)
	{
		lightmapRefs.Clear();
		int numIndices = lightmapIndices.Count;
		for(int index = 0; index < numIndices; ++index)
		{
			int lightmapIndex = lightmapIndices[index];
			if(lightmapIndex >= 0 && lightmapIndex < LightmapSettings.lightmaps.Length)
			{
				RefData newRef = new RefData();
				newRef.index = lightmapIndex;
#if UNITY_2017_1_OR_NEWER
				newRef.NearLightmap = LightmapSettings.lightmaps[lightmapIndex].lightmapDir;
				newRef.FarLightmap = LightmapSettings.lightmaps[lightmapIndex].lightmapColor;
#elif UNITY_5_5_OR_NEWER
				newRef.NearLightmap = LightmapSettings.lightmaps[lightmapIndex].lightmapDir;
				newRef.FarLightmap = LightmapSettings.lightmaps[lightmapIndex].lightmapLight;
#else
				newRef.NearLightmap = LightmapSettings.lightmaps[lightmapIndex].lightmapNear;
				newRef.FarLightmap = LightmapSettings.lightmaps[lightmapIndex].lightmapFar;
#endif
				lightmapRefs.Add(newRef);
			}
		}

#if !UNITY_4
		lightmapRenderRefs.Clear();
		Transform[] transforms = GetComponentsInChildren<Transform>();
		int numTransforms = transforms.Length;
		for(int transformIndex = 0; transformIndex < numTransforms; ++transformIndex)
		{
			Renderer renderer = transforms[transformIndex].GetComponent<Renderer>();
			Terrain terrain = transforms[transformIndex].GetComponent<Terrain>();
			if((renderer != null && renderer.lightmapIndex >= 0) ||
			   (terrain != null && terrain.lightmapIndex >= 0))
			{
				RenderData newIndex = new RenderData();
				if(renderer)
				{
					newIndex.renderer = renderer;
					newIndex.rendererLightmapIndex = renderer.lightmapIndex;
					newIndex.rendererLightmapScaleOffset = renderer.lightmapScaleOffset;
					GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, GameObjectUtility.GetStaticEditorFlags(renderer.gameObject) & ~StaticEditorFlags.BatchingStatic);
				}
				if(terrain)
				{
					newIndex.terrain = terrain;
					newIndex.terrainLightmapIndex = terrain.lightmapIndex;
				}
				lightmapRenderRefs.Add(newIndex);
			}
		}
#endif
	}
#endif
	#endregion

	#region Unity Interface
	void Start()
	{
		if((!Application.isEditor || Application.isPlaying) && globalLightmapRefCount != null)
		{
#if UNITY_4
			int numLightmaps = LightmapSettings.lightmaps.Length;
			int numRefs = lightmapRefs.Count;
			for(int refIndex = 0; refIndex < numRefs; ++refIndex)
			{
				RefData refData = lightmapRefs[refIndex];
				if(refData.index >= 0 && refData.index < globalLightmapRefCount.Length)
				{
					if(globalLightmapRefCount[refData.index] == 0)
					{
						LightmapData newData = new LightmapData();
						newData.lightmapNear = refData.NearLightmap;
						newData.lightmapFar = refData.FarLightmap;

						LightmapData[] newLightmaps = new LightmapData[numLightmaps];
						for(int lightmapIndex = 0; lightmapIndex < numLightmaps; ++lightmapIndex)
						{
							if(refData.index == lightmapIndex)
							{
								newLightmaps[lightmapIndex] = newData;
							}
							else
							{
								newLightmaps[lightmapIndex] = LightmapSettings.lightmaps[lightmapIndex];
							}
						}
						LightmapSettings.lightmaps = newLightmaps;
					}
					++globalLightmapRefCount[refData.index];
				}
			}
#else
			int numMeshIndeces = lightmapRenderRefs.Count;
			for(int meshIndex = 0; meshIndex < numMeshIndeces; ++meshIndex)
			{
				RenderData indexData = lightmapRenderRefs[meshIndex];
				if(indexData.renderer)
				{
					indexData.renderer.lightmapIndex = indexData.rendererLightmapIndex;
					indexData.renderer.lightmapScaleOffset = indexData.rendererLightmapScaleOffset;
				}
				if(indexData.terrain)
				{
					indexData.terrain.lightmapIndex = indexData.terrainLightmapIndex;
				}
			}
#endif
		}
	}

	void OnDestroy()
	{
#if UNITY_4
		if((!Application.isEditor || Application.isPlaying) && globalLightmapRefCount != null)
		{
			int numLightmaps = LightmapSettings.lightmaps.Length;
			int numRefs = lightmapRefs.Count;
			for(int refIndex = 0; refIndex < numRefs; ++refIndex)
			{
				RefData refData = lightmapRefs[refIndex];
				if(refData.index >= 0 && refData.index < globalLightmapRefCount.Length)
				{
					--globalLightmapRefCount[refData.index];
					if(globalLightmapRefCount[refData.index] == 0)
					{
						LightmapData[] newLightmaps = new LightmapData[numLightmaps];
						for(int lightmapIndex = 0; lightmapIndex < numLightmaps; ++lightmapIndex)
						{
							if(refData.index == lightmapIndex)
							{
								newLightmaps[lightmapIndex] = new LightmapData();
							}
							else
							{
								newLightmaps[lightmapIndex] = LightmapSettings.lightmaps[lightmapIndex];
							}
						}
						LightmapSettings.lightmaps = newLightmaps;
					}
				}
			}
		}
#endif
	}
	#endregion
}
