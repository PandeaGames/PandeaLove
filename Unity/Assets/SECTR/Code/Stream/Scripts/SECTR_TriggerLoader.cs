// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System.Collections.Generic;

/// \ingroup Stream
/// (Un)loads a list of SECTR_Chunk objects based on Unity Trigger events.
///
/// The component allows developers to load a list of Sectors when the
/// player enters a particluar region of space. TriggerLoader uses
/// standard Unity trigger events, so any Collider can be used, provided
/// its marked as a trigger.
[AddComponentMenu("SECTR/Stream/SECTR Trigger Loader")]
public class SECTR_TriggerLoader : SECTR_Loader 
{
	#region Private Members
	// Number of times we've loaded our chunks.
	// May be > 1 depending on number of objects that can activate the trigger.
	protected int loadedCount = 0;
	protected bool chunksReferenced = false;
	#endregion

	#region Public Interface
	[SECTR_ToolTip("List of Sectors to load when entering this trigger.")]
	public List<SECTR_Sector> Sectors = new List<SECTR_Sector>();
	[SECTR_ToolTip("Should the Sectors be unloaded when trigger is exited.")]
	public bool UnloadOnExit = true;

	/// Returns true if all referenced Chunks are loaded. False, otherwise.
	public override bool Loaded
	{
		get
		{
			int numSectors = Sectors.Count;
			bool loaded = numSectors > 0;
			for(int sectorIndex = 0; sectorIndex < numSectors && loaded; ++sectorIndex)
			{
				SECTR_Sector sector = Sectors[sectorIndex];
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
	void OnDisable()
	{
		_UnrefChunks();
		loadedCount = 0;
	}

	void OnTriggerEnter(Collider other)
	{
		if(loadedCount == 0)
		{
			_RefChunks();
		}
		++loadedCount;
	}

	void OnTriggerExit(Collider other)
	{
		if(loadedCount > 0) // guard against order of destruction issues.
		{
			--loadedCount;
			if(loadedCount == 0 && UnloadOnExit)
			{
				_UnrefChunks();
			}
		}
	}
	#endregion

	#region Private Members
	private void _RefChunks()
	{
		if(!chunksReferenced)
		{
			int numChunks = Sectors.Count;
			for(int chunkIndex = 0; chunkIndex < numChunks; ++chunkIndex)
			{
				SECTR_Sector sector = Sectors[chunkIndex];
				if(sector)
				{
					SECTR_Chunk chunk = sector.GetComponent<SECTR_Chunk>();
					if(chunk)
					{
						chunk.AddReference();
					}
				}
			}
			chunksReferenced = true;
		}
	}

	private void _UnrefChunks()
	{
		if(chunksReferenced)
		{
			int numChunks = Sectors.Count;
			for(int chunkIndex = 0; chunkIndex < numChunks; ++chunkIndex)
			{
				SECTR_Sector sector = Sectors[chunkIndex];
				if(sector)
				{
					SECTR_Chunk chunk = sector.GetComponent<SECTR_Chunk>();
					if(chunk)
					{
						chunk.RemoveReference();
					}
				}
			}
			chunksReferenced = false;
		}
	}
	#endregion
}
