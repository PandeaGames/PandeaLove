// Copyright (c) 2014 Make Code Now! LLC
#if !(UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1)
#define UNITY_5_LATE
#endif

#if (UNITY_5_LATE && !UNITY_5_2)
#define UNITY_MSE
#endif

using UnityEngine;
#if UNITY_MSE
using UnityEngine.SceneManagement;
#endif
using System.Collections.Generic;
using System.Collections;

/// \ingroup Stream
/// Chunk is the loadable/streamable version of a SECTR_Sector. The
/// Chunk manages loading and unloading that data, usually at
/// the request of a Loader component.
/// 
/// Chunk stores the data needed to load (and unload) a Sector
/// that has been exported into a separate scene file. Loading will
/// happen asynchronously if the user has any Unity 5 or a Pro Unity 4 Pro license,
/// synchronously otherwise.
/// 
/// Chunk uses a reference counted loading scheme, so multiple
/// clients may safely request loading the same Chunk, provided that
/// they equally match their Load requests with their Unload requests.
/// Data for the Sector will be loaded when the reference count goes up
/// from 0, and unloaded when it returns to 0.
[RequireComponent(typeof(SECTR_Sector))]
[AddComponentMenu("SECTR/Stream/SECTR Chunk")]
public class SECTR_Chunk : MonoBehaviour
{
	#region Private Members
	private enum LoadState
	{
		Unloaded,
		Loading,
		Loaded,
		Unloading,
		Active,
	}

	private AsyncOperation asyncLoadOp;
	private LoadState loadState = LoadState.Unloaded;
	private int refCount = 0;
	private GameObject chunkRoot = null;
	private GameObject chunkSector = null;
	private bool recenterChunk = false;
	private SECTR_Sector cachedSector = null;
	private GameObject proxy = null;
	private bool quitting = false;

	private static SECTR_Chunk chunkActivating = null;
	private static LinkedList<SECTR_Chunk> activationQueue = new LinkedList<SECTR_Chunk>();
	private static bool requestedDeferredUnload = false;
	#endregion

	#region Public Interface
	[SECTR_ToolTip("The path of the scene to load")]
	public string ScenePath;
	[SECTR_ToolTip("The unique name of the root object in the exported Sector.")]
	public string NodeName;
	[SECTR_ToolTip("Exports the Chunk in a way that allows it to be shared by multiple Sectors, but may take more CPU to load.")]
	public bool ExportForReuse = false;
	[SECTR_ToolTip("A mesh to display when this Chunk is unloaded. Will be hidden when loaded.")]
	public Mesh ProxyMesh;
	[SECTR_ToolTip("The per-submesh materials for the proxy.")]
	public Material[] ProxyMaterials;

	/// Returns the Sector associated with this Chunk.
	public SECTR_Sector Sector
	{
		get { return cachedSector; }
	}

	/// Add a reference to this Chunk. If this is the first reference,
    /// the data associated with the SectorChunk will be loaded.
	/// If you call AddReference, make sure to eventually call RemoveReference.
	public void AddReference()
	{
		if(refCount == 0)
		{
			_Load();
			if(Changed != null)
			{
				Changed(this, true);
			}
		}
		++refCount;
		if(ReferenceChange != null)
		{
			ReferenceChange(this, true);
		}
	}

	/// Add a reference to this Chunk. If this is the first reference,
	/// the data associated with the SectorChunk will be loaded.
	public void RemoveReference()
	{
		if(ReferenceChange != null)
		{
			ReferenceChange(this, false);
		}
		--refCount;
		if(refCount <= 0)
		{
			if(Changed != null)
			{
				Changed(this, false);
			}
			_Unload();
			// Guard against underflows.
			refCount = 0;
		}
	}

	/// Determines whether the Chunk data is currently loaded.
	/// <returns>True if this instance is loaded; otherwise false.</returns>
	public bool IsLoaded()
	{
		return loadState == LoadState.Active;
	}

	/// Determines whether the Chunk data is currently unloaded.
	/// <returns>True if this instance is unloaded; otherwise false.</returns>
	public bool IsUnloaded()
	{
		return loadState == LoadState.Unloaded;
	}
	
	/// Returns the progress of the load, perhaps for use in an in-game display.
	/// <returns>The progress as a float between 0 and 1.</returns>
	public float LoadProgress()
	{
		switch(loadState)
		{
		case LoadState.Loading:
			return asyncLoadOp != null ? asyncLoadOp.progress * 0.8f : 0.5f;
		case LoadState.Loaded:
			return 0.9f;
		case LoadState.Active:
			return 1f;
		case LoadState.Unloaded:
		case LoadState.Unloading:
		default:
			return 0f;
		}
	}

	public delegate void LoadCallback(SECTR_Chunk source, bool loaded);

	/// Event handler for load/unload callbacks.
	public event LoadCallback Changed;
	public event LoadCallback ReferenceChange;
	#endregion

	#region Unity Interface
	void Awake()
	{
		SECTR_LightmapRef.InitRefCounts();
	}

	void OnEnable()
	{
		cachedSector = GetComponent<SECTR_Sector>();
		if(cachedSector.Frozen)
		{
			_CreateProxy();
		}
	}

	void OnDisable()
	{
		if(!quitting && asyncLoadOp != null && !asyncLoadOp.isDone)
		{
			Debug.LogError("Chunk unloaded with async operation active. " +
						   "Do not disable chunks until async operations are complete or Unity will likely crash.");
		}

		if(loadState != LoadState.Unloaded)
		{
			_FindChunkRoot();
			if(chunkRoot)
			{
				_DestoryChunk(false, true);
			}
		}
		cachedSector = null;
	}

	void OnApplicationQuit()
	{
		quitting = true;
	}

	void FixedUpdate()
	{
		switch(loadState)
		{
		case LoadState.Loading:
			_TrySceneActivation();
			if(asyncLoadOp == null || asyncLoadOp.isDone)
			{
				if(asyncLoadOp != null)
				{
					chunkActivating = null;
					activationQueue.RemoveFirst();
					asyncLoadOp = null;
				}
				loadState = LoadState.Loaded;
				// Run update again to try to parent the chunk right away.
				FixedUpdate();
			}
			break;
		case LoadState.Loaded:
			// Unity takes a frame to create the objects, so fix them up here.
			_SetupChunk();
			break;
		case LoadState.Active:
			// Do nothing.
			break;
		case LoadState.Unloading:
			_TrySceneActivation();
			_FindChunkRoot();
			if(chunkRoot)
			{
				_DestoryChunk(true, false);
			}
			break;
		}
	}
	#endregion

	#region Private Methods
	private void _Load()
	{
		if(ScenePath != null && enabled && (loadState == LoadState.Unloaded || loadState == LoadState.Unloading))
		{
			if(loadState == LoadState.Unloaded)
			{
				loadState = LoadState.Loading;
				if(!SECTR_Modules.HasPro())
				{
					#if UNITY_MSE
					SceneManager.LoadScene(ScenePath, LoadSceneMode.Additive);
					#else
					Application.LoadLevelAdditive(ScenePath);
					#endif
					_SetupChunk();
				}
				else
				{
					#if UNITY_MSE
					asyncLoadOp = SceneManager.LoadSceneAsync(ScenePath, LoadSceneMode.Additive);
					#else
					asyncLoadOp = Application.LoadLevelAdditiveAsync(ScenePath);
					#endif
					activationQueue.AddLast(this);
				}
				chunkRoot = null;
				chunkSector = null;
				recenterChunk = false;
			}
			else
			{
				loadState = LoadState.Loading;
			}
		}
	}

	private void _Unload()
	{
		if(enabled && loadState != LoadState.Unloaded)
		{
			if(cachedSector)
			{
				cachedSector.Frozen = true;
			}

			if(chunkRoot)
			{
				_DestoryChunk(true, false);
			}
			else
			{
				loadState = LoadState.Unloading;
			}
		}
	}

	private void _DestoryChunk(bool createProxy, bool fromDisable)
	{
		if(cachedSector && (cachedSector.TopTerrain || cachedSector.BottomTerrain || cachedSector.RightTerrain || cachedSector.LeftTerrain))
		{
			cachedSector.DisonnectTerrainNeighbors();
		}
#if UNITY_MSE
  #if UNITY_5_4 // Unity 5_4 specific bug workaround
		if(!fromDisable)
		{
			StartCoroutine(_UnloadScene(ScenePath));
		}
		else
  #endif
		{
#if UNITY_5_5_OR_NEWER
			SceneManager.UnloadSceneAsync(ScenePath);
#else
			SceneManager.UnloadScene(ScenePath);
#endif
		}
#elif UNITY_5_LATE
		Application.UnloadLevel(ScenePath);
#else
		GameObject.Destroy(chunkRoot);
#endif
		chunkRoot = null;
		chunkSector = null;
		recenterChunk = false;
		if(asyncLoadOp != null)
		{
			if(chunkActivating == this)
			{
				chunkActivating = null;
			}
			activationQueue.Remove(this);
			asyncLoadOp = null;
		}
		if(fromDisable || quitting)
		{
			_UnloadResources();
		}
		else if(!requestedDeferredUnload)
		{
			requestedDeferredUnload = true;
			StartCoroutine("_DeferredUnload");
		}
		loadState = LoadState.Unloaded;
		if(createProxy && ProxyMesh)
		{
			_CreateProxy();
		}
	}

	private void _FindChunkRoot()
	{
		if(chunkRoot == null && !quitting)
		{
			SECTR_ChunkRef chunkRef = SECTR_ChunkRef.FindChunkRef(NodeName);
			if(chunkRef && chunkRef.RealSector)
			{
				recenterChunk = chunkRef.Recentered;
				if(recenterChunk)
				{
					chunkRef.RealSector.parent = transform;
					chunkRoot = chunkRef.RealSector.gameObject;
					chunkSector = chunkRoot;
					GameObject.Destroy(chunkRef.gameObject);
				}
				else
				{
					chunkRoot = chunkRef.gameObject;
					chunkSector = chunkRef.RealSector.gameObject;
					GameObject.Destroy(chunkRef);
				}
			}
			else
			{
				chunkRoot = GameObject.Find(NodeName);
				chunkSector = chunkRoot;
				recenterChunk = false;
			}
		}
	}

	private void _SetupChunk()
	{
		_FindChunkRoot();
		if(chunkRoot)
		{
			// Activate the root if inactive (due to backwards compat or recentering
			if(!chunkRoot.activeSelf)
			{
				chunkRoot.SetActive(true);
			}

			// Recenter chunk under ourselves
			if(recenterChunk)
			{
				Transform chunkTransform = chunkRoot.transform;
				chunkTransform.localPosition = Vector3.zero;
				chunkTransform.localRotation = Quaternion.identity;
				chunkTransform.localScale = Vector3.one;
			}

			// Hook up the child proxy
			SECTR_Member rootMember = chunkSector.GetComponent<SECTR_Member>();
			if(!rootMember)
			{
				rootMember = chunkSector.gameObject.AddComponent<SECTR_Member>();
				rootMember.BoundsUpdateMode = SECTR_Member.BoundsUpdateModes.Static;
				rootMember.ForceUpdate(true);
			}
			else if(recenterChunk)
			{
				rootMember.ForceUpdate(true);
			}
			cachedSector.ChildProxy = rootMember;

			// Unfreeze our sector
			cachedSector.Frozen = false;
			if(cachedSector.TopTerrain || cachedSector.BottomTerrain ||
			   cachedSector.LeftTerrain || cachedSector.RightTerrain)
			{
				cachedSector.ConnectTerrainNeighbors();
				if(cachedSector.TopTerrain)
				{
					cachedSector.TopTerrain.ConnectTerrainNeighbors();
				}
				if(cachedSector.BottomTerrain)
				{
					cachedSector.BottomTerrain.ConnectTerrainNeighbors();
				}
				if(cachedSector.LeftTerrain)
				{
					cachedSector.LeftTerrain.ConnectTerrainNeighbors();
				}
				if(cachedSector.RightTerrain)
				{
					cachedSector.RightTerrain.ConnectTerrainNeighbors();
				}
			}

			// Remove the proxy if there is one
			if(proxy)
			{
				GameObject.Destroy(proxy);
			}

			loadState = LoadState.Active;
		}
	}

	private void _CreateProxy()
	{
		if(proxy == null && ProxyMesh && !quitting)
		{
			proxy = new GameObject(name + " Proxy");
			MeshFilter newFilter = proxy.AddComponent<MeshFilter>();
			newFilter.sharedMesh = ProxyMesh;
			MeshRenderer proxyRenderer = proxy.AddComponent<MeshRenderer>();
			proxyRenderer.sharedMaterials = ProxyMaterials;
			proxy.transform.position = transform.position;
			proxy.transform.rotation = transform.rotation;
			proxy.transform.localScale = transform.lossyScale;
		}
	}

	private void _TrySceneActivation()
	{
		if(chunkActivating == null &&
			asyncLoadOp != null && !asyncLoadOp.allowSceneActivation && asyncLoadOp.progress >= 0.9f && 
		    activationQueue.Count > 0 && activationQueue.First.Value == this)
		{
			chunkActivating = this;
			asyncLoadOp.allowSceneActivation = true;
		}
	}

	private void _UnloadResources()
	{
		Resources.UnloadUnusedAssets();
		requestedDeferredUnload = false;
	}

	private IEnumerator _DeferredUnload()
	{
		yield return new WaitForEndOfFrame();
		_UnloadResources();
		yield return null;
	}
	
#if UNITY_MSE
	private IEnumerator _UnloadScene(string scenePath)
	{
		yield return new WaitForEndOfFrame();
#if UNITY_5_5_OR_NEWER
		SceneManager.UnloadSceneAsync(ScenePath);
#else
		SceneManager.UnloadScene(ScenePath);
#endif
	}
#endif
	#endregion
}
