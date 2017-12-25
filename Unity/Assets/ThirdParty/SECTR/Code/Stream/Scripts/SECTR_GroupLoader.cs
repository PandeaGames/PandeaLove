// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System.Collections.Generic;

/// \ingroup Stream
/// Allows users to group a set of Sectors, loading and unloading them
/// as if they were a single Sector.
/// 
/// There are occasions where a section of the scene needs to be split into
/// multiple Sectors (perhaps for occlusion culling or game logic) but they
/// need be loaded as if they were part of a single Sectors. Group Loader
/// takes care of this, by automatically incrementing and decrementing
/// reference counts whenever one of the Sectors in the list is loaded
/// or unloaded.
public class SECTR_GroupLoader : SECTR_Loader
{
	#region Public Interface
	[SECTR_ToolTip("The Sectors to load and unload together.")]
	public List<SECTR_Sector> Sectors = new List<SECTR_Sector>();

	/// Returns true if all Sectors in the group are loaded.
	public override bool Loaded
	{
		get
		{
			int numSectors = Sectors.Count;
			bool loaded = numSectors > 0;
			for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
			{
				SECTR_Sector sector = Sectors[sectorIndex];
				if(sector && sector.Frozen)
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
	void OnEnable()
	{
		int numSectors = Sectors.Count;
		for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
		{
			SECTR_Sector sector = Sectors[sectorIndex];
			if(sector)
			{
				SECTR_Chunk chunk = sector.GetComponent<SECTR_Chunk>();
				if(chunk)
				{
					chunk.ReferenceChange += ChunkChanged;
				}
			}
		}
	}

	void OnDisable()
	{
		int numSectors = Sectors.Count;
		for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
		{
			SECTR_Sector sector = Sectors[sectorIndex];
			if(sector)
			{
				SECTR_Chunk chunk = sector.GetComponent<SECTR_Chunk>();
				if(chunk)
				{
					chunk.ReferenceChange -= ChunkChanged;
				}
			}
		}
	}

	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		Gizmos.color = SECTR_Sector.SectorColor;
		int numSectors = Sectors.Count;
		for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
		{
			SECTR_Sector sector = Sectors[sectorIndex];
			if(sector)
			{
				Gizmos.DrawLine(transform.position, sector.TotalBounds.center);
			}
		}
	}
	#endif
	#endregion

	#region Private Methods
	private void ChunkChanged(SECTR_Chunk source, bool loaded)
	{
		int numSectors = Sectors.Count;
		for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
		{
			SECTR_Sector sector = Sectors[sectorIndex];
			if(sector)
			{
				SECTR_Chunk chunk = sector.GetComponent<SECTR_Chunk>();
				if(chunk && chunk != source)
				{
					// We need to temporarily remove our callback so
					// that we don't get infinite loops.
					chunk.ReferenceChange -= ChunkChanged;
					if(loaded)
					{
						chunk.AddReference();
					}
					else
					{
						chunk.RemoveReference();
					}
					chunk.ReferenceChange += ChunkChanged;
				}
			}
		}
	}
	#endregion
}
