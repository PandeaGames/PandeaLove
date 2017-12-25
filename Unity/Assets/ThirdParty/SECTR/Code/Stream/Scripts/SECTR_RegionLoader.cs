// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System.Collections.Generic;

/// \ingroup Stream
/// (Un)loads Chunks within a given volume. Can be set to optionally not touch Sectors
/// that are not part of the terrain grid.
[AddComponentMenu("SECTR/Stream/SECTR Region Loader")]
public class SECTR_RegionLoader : SECTR_Loader 
{
	#region Private Details
	private List<SECTR_Sector> sectors = new List<SECTR_Sector>(16);
	private List<SECTR_Sector> loadSectors = new List<SECTR_Sector>(16);
	private List<SECTR_Sector> unloadSectors = new List<SECTR_Sector>(16);
	private bool updated = false;
	#endregion

	#region Public Interface
	[SECTR_ToolTip("The dimensions of the volume in which terrain chunks should be loaded.")]
	public Vector3 LoadSize = new Vector3(20f, 10f, 20f);
	[SECTR_ToolTip("The distance from the load size that you need to move for a Sector to unload (as a percentage).", 0f, 1f)]
	public float UnloadBuffer = 0.1f;
	[SECTR_ToolTip("If set, will only load Sectors in matching layers.")]
	public LayerMask LayersToLoad = -1;

	/// Returns true if all referenced Chunks in region are loaded. False, otherwise.
	public override bool Loaded
	{
		get
		{
			int numSectors = sectors.Count;
			bool loaded = updated || numSectors == 0;
			for(int sectorIndex = 0; sectorIndex < numSectors && loaded; ++sectorIndex)
			{
				SECTR_Sector sector = sectors[sectorIndex];
				if(sector.Frozen)
				{
					SECTR_Chunk chunk = sector.GetComponent<SECTR_Chunk>();
					if(chunk && !chunk.IsLoaded())
					{
						loaded = false;
						break;
					}
				}
			}
			return loaded;
		}
	}
	#endregion

	#region Unity Interface
	void Start()
	{
		LockSelf(true);
	}

	void OnDisable()
	{
		int numSectors = sectors.Count;
		for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
		{
			SECTR_Sector sector = sectors[sectorIndex];
			if(sector)
			{
				SECTR_Chunk chunk = sector.GetComponent<SECTR_Chunk>();
				if(chunk)
				{
					chunk.RemoveReference();
				}
			}
		}
		sectors.Clear();
		updated = false;
	}

	void Update()
	{
		Vector3 position = transform.position;
		Bounds loadBounds = new Bounds(position, LoadSize);
		Bounds unloadBounds = new Bounds(position, LoadSize * (1f + UnloadBuffer));

		SECTR_Sector.GetContaining(ref loadSectors, loadBounds);
		SECTR_Sector.GetContaining(ref unloadSectors, unloadBounds);

		int sectorIndex = 0;
		int numSectors = sectors.Count;
		while(sectorIndex < numSectors)
		{
			SECTR_Sector oldSector = sectors[sectorIndex];
			if(loadSectors.Contains(oldSector))
			{
				loadSectors.Remove(oldSector);
				++sectorIndex;
			}
			else if(!unloadSectors.Contains(oldSector))
			{
				SECTR_Chunk oldChunk = oldSector != null ? oldSector.GetComponent<SECTR_Chunk>() : null;
				if(oldChunk)
				{
					oldChunk.RemoveReference();
				}
				sectors.RemoveAt(sectorIndex);
				--numSectors;
			}
			else
			{
				++sectorIndex;
			}
		}
		
		numSectors = loadSectors.Count;
		int layerMaskValue = LayersToLoad.value;
		if(numSectors > 0)
		{
			for(sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
			{
				SECTR_Sector newSector = loadSectors[sectorIndex];
				if(newSector && newSector.Frozen && ((layerMaskValue & (1 << newSector.gameObject.layer)) != 0))
				{
					SECTR_Chunk newChunk = newSector.GetComponent<SECTR_Chunk>();
					if(newChunk)
					{
						newChunk.AddReference();
					}
					sectors.Add(newSector);
				}
			}
		}

		if(locked && Loaded)
		{
			LockSelf(false);
		}

		updated = true;
	}

	#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(transform.position, LoadSize);
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(transform.position, LoadSize * (1f + UnloadBuffer));
	}
	#endif
	#endregion
}
