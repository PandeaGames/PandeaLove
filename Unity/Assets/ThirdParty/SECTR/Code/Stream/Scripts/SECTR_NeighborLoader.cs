// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System.Collections.Generic;

/// \ingroup Stream
/// Loads SECTR_Chunk components that are in the current or adjacent SECTR_Sector.
///
/// Neighbor Loader determines which SECTR_Chunk objects to load/unload by performing
/// a breadth-first walk of the Sector/Portal graph. The depth of this walk is limited by
/// the MaxDepth property, where a value of zero means "only the current Sector", a depth
/// of one means "current and each adjacent sector", etc. Many games will want to place
/// a Neighbor Loader on the player to ensure that the Sectors they are in or near
/// are always loaded.
[RequireComponent(typeof(SECTR_Member))]
[AddComponentMenu("SECTR/Stream/SECTR Neighbor Loader")]
public class SECTR_NeighborLoader : SECTR_Loader 
{
	#region Private Members
	// Cached ref to the neighbor member.
	private SECTR_Member cachedMember;

	// List of all sectors to whom we've added a reference.
	private List<SECTR_Sector> currentSectors = new List<SECTR_Sector>(4);
	// Container to be re-used betwen searches to cut down on garbage generation
	private List<SECTR_Graph.Node> neighbors = new List<SECTR_Graph.Node>(8);
	#endregion

	#region Public Interface
	[SECTR_ToolTip("Determines how far out to load neighbor sectors from the current sector. Depth of 0 means only the current Sector.")]
	public int MaxDepth = 1;

	/// Returns true if all referenced Chunks are loaded. False, otherwise.
	public override bool Loaded
	{
		get
		{
			int numSectors = currentSectors.Count;
			bool loaded = numSectors > 0;
			for(int sectorIndex = 0; sectorIndex < numSectors && loaded; ++sectorIndex)
			{
				SECTR_Sector sector = currentSectors[sectorIndex];
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
	void OnEnable()
	{
		cachedMember = GetComponent<SECTR_Member>();
		cachedMember.Changed += new SECTR_Member.MembershipChanged(_MembershipChanged);
	}

	void OnDisable()
	{
		if(cachedMember)
		{
			cachedMember.Changed -= new SECTR_Member.MembershipChanged(_MembershipChanged);
			cachedMember = null;
		}

		// It's possible for this component to get destroyed before the relevant sector,
		// so make sure to release all relevant references
		if(currentSectors.Count > 0)
		{
			_MembershipChanged(currentSectors, null);
		}
	}

	void Start()
	{
		LockSelf(true);
	}

	void Update()
	{
		if(locked && Loaded)
		{
			LockSelf(false);
		}
	}
	#endregion

	#region Private Members
	// Add and removes references to current and neighboring sectors
	// as this component moves around the world.
	private void _MembershipChanged(List<SECTR_Sector> left, List<SECTR_Sector> joined)
	{
		// Add ref to all of the new objects first so that we don't unload and then immeditately load again.
		if(joined != null)
		{
			int numJoined = joined.Count;
			for(int sectorIndex = 0; sectorIndex < numJoined; ++sectorIndex)
			{
				SECTR_Sector sector = joined[sectorIndex];
				if(sector && !currentSectors.Contains(sector))
				{
					SECTR_Graph.BreadthWalk(ref neighbors, sector, 0, MaxDepth);
					int numNeighbors = neighbors.Count;
					for(int neighborIndex = 0; neighborIndex < numNeighbors; ++neighborIndex)
					{
						SECTR_Chunk neighborChunk = neighbors[neighborIndex].Sector.GetComponent<SECTR_Chunk>();
						if(neighborChunk)
						{
							neighborChunk.AddReference();
						}
					}
					currentSectors.Add(sector);
				}
			}
		}

		// Dec ref any sectors we're no longer in.
		if(left != null)
		{
			int numLeft = left.Count;
			for(int sectorIndex = 0; sectorIndex < numLeft; ++sectorIndex)
			{
				SECTR_Sector sector = left[sectorIndex];
				// We have to be careful about double-removing on shutdown b/c we don't control
				// order of destruction.
				if(sector && currentSectors.Contains(sector))
				{
					SECTR_Graph.BreadthWalk(ref neighbors, sector, 0, MaxDepth);
					int numNeighbors = neighbors.Count;
					for(int neighborIndex = 0; neighborIndex < numNeighbors; ++neighborIndex)
					{
						SECTR_Chunk neighborChunk = neighbors[neighborIndex].Sector.GetComponent<SECTR_Chunk>();
						if(neighborChunk)
						{
							neighborChunk.RemoveReference();
						}
					}
					currentSectors.Remove(sector);
				}
			}
		}
	}
	#endregion
}
