// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// \ingroup Core
/// Sectors represent discrete sections of the world, connected to one another by SECTR_Portal objects.

/// Sectors are roughly analagous to rooms in a building, with a unique shape, size and
/// location. Objects that overlap the bounds of the Sector are considered to be contained in it, 
/// in the same way that a table or stove would be thought of as "in the kitchen". Sector 
/// bounds can overlap and membership is not exclusive; a SECTR_Member may be in multiple Sectors at once. 
/// 
/// Like the rest of the system, Sectors are be completely dyanamic, and can transform
/// and be enabled/disabled dynamically. Because the rooms in many games are completely static,
/// marking a Sector as isStatic will enable some additional performance optimizations.
/// 
/// The size and shape of a Sector is defined by the union of the bounds of the Renderable
/// Mesh children parented underneath it. Lights and other types of objects may be part of
/// the Sector proper, but will not influence the "official" bounds.
/// 
/// As an implementation detail, Sector derives from SECTR_Member, primarily because every Sector
/// needs the services that Member provides, and a little bit of special treatment besides.
[ExecuteInEditMode]
[AddComponentMenu("SECTR/Core/SECTR Sector")]
public class SECTR_Sector : SECTR_Member {
	#region Private Details
	private List<SECTR_Portal> portals = new List<SECTR_Portal>(8);
	private List<SECTR_Member> members = new List<SECTR_Member>(32);
	private bool visited = false;

	private static List<SECTR_Sector> allSectors = new List<SECTR_Sector>(128);

	SECTR_Sector()
	{
		isSector = true;
	}
	#endregion

	#region Public Interface
	[SECTR_ToolTip("The terrain Sector attached on the top side of this Sector.")]
	public SECTR_Sector TopTerrain;
	[SECTR_ToolTip("The terrain Sector attached on the bottom side of this Sector.")]
	public SECTR_Sector BottomTerrain;
	[SECTR_ToolTip("The terrain Sector attached on the left side of this Sector.")]
	public SECTR_Sector LeftTerrain;
	[SECTR_ToolTip("The terrain Sector attached on the right side of this Sector.")]
	public SECTR_Sector RightTerrain;

	/// Returns a list of all enabled Sectors.
	public new static List<SECTR_Sector> All
	{
		get { return allSectors; }
	}

	/// Returns the list of Sectors that contain a given point.
	/// Sectors may overlap and are not exclusive, hence the list.
	/// <param name="sectors"> List of sectors to write into.</param>
	/// <param name="position"> The world space position for which to search.</param>
	/// <returns>List of Sectors containing position.</returns>
	public static void GetContaining(ref List<SECTR_Sector> sectors, Vector3 position)
	{
		sectors.Clear();
		int numSectors = allSectors.Count;
		for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
		{
			SECTR_Sector sector = allSectors[sectorIndex];
			if(sector.TotalBounds.Contains(position))
			{
				sectors.Add(sector);
			}
		}
	}
	
	/// Returns the list of Sectors that intersect an AABB.
	/// Sectors may overlap and are not exclusive, hence the list.
	/// <param name="sectors"> List of sectors to write into.</param>
	/// <param name="bounds">The world space bounding box for which to search.</param>
	/// <returns>The List of Sectors overlapping bounds</returns>
	public static void GetContaining(ref List<SECTR_Sector> sectors, Bounds bounds)
	{
		sectors.Clear();
		int numSectors = allSectors.Count;
		for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
		{
			SECTR_Sector sector = allSectors[sectorIndex];
			if(sector.TotalBounds.Intersects(bounds))
			{
				sectors.Add(sector);
			}
		}
	}

	/// Utility property for tracking during graph walks.
	public bool Visited
	{
		get { return visited; }
		set { visited = value; }
	}

	/// Returns all of the portals connected to this Sector.
	public List<SECTR_Portal> Portals
	{
		get { return portals; }
	}
	
	/// Accessor for the members of this Sector.
	public List<SECTR_Member> Members
	{
		get { return members; }
	}

	/// Returns true if this Sector is setup as part of a grid of terrain tiles.
	public bool IsConnectedTerrain
	{
		get { return LeftTerrain || RightTerrain || TopTerrain || BottomTerrain; }
	}

	/// Sets up the terrain neighbors structure.
	public void ConnectTerrainNeighbors()
	{
		Terrain myTerrain = GetTerrain(this);
		if(myTerrain)
		{
			myTerrain.SetNeighbors(
				GetTerrain(LeftTerrain),
				GetTerrain(TopTerrain),
				GetTerrain(RightTerrain),
				GetTerrain(BottomTerrain));
		}
	}

	/// Disconnects this terrain from all neighbors and vice versa.
	/// Works around a crash in some versions of Unity.
	public void DisonnectTerrainNeighbors()
	{
		// Set my neighbors to null
		Terrain myTerrain = GetTerrain(this);
		if(myTerrain)
		{
			myTerrain.SetNeighbors(null, null, null, null);
		}

		// Remove self from neighbor terrains.
		Terrain topTerrain = GetTerrain(TopTerrain);
		if(topTerrain)
		{
			topTerrain.SetNeighbors(GetTerrain(TopTerrain.LeftTerrain), GetTerrain(TopTerrain.TopTerrain), GetTerrain(TopTerrain.RightTerrain), null);
		}

		Terrain bottomTerrain = GetTerrain(BottomTerrain);
		if(bottomTerrain)
		{
			bottomTerrain.SetNeighbors(GetTerrain(BottomTerrain.LeftTerrain), null, GetTerrain(BottomTerrain.RightTerrain), GetTerrain(BottomTerrain.BottomTerrain));
		}

		Terrain leftTerrain = GetTerrain(LeftTerrain);
		if(leftTerrain)
		{
			leftTerrain.SetNeighbors(GetTerrain(LeftTerrain.LeftTerrain), GetTerrain(LeftTerrain.TopTerrain), null, GetTerrain(LeftTerrain.BottomTerrain));
		}

		Terrain rightTerrain = GetTerrain(RightTerrain);
		if(rightTerrain)
		{
			rightTerrain.SetNeighbors(null, GetTerrain(RightTerrain.TopTerrain), GetTerrain(RightTerrain.RightTerrain),GetTerrain(RightTerrain.BottomTerrain));
		}
	}
	
	#if UNITY_EDITOR
	public List<SECTR_Member.Child> GetSharedChildren()
	{
		List<SECTR_Member.Child> sharedChildren = new List<SECTR_Member.Child>();
		List<SECTR_Sector> overlappingSectors = new List<SECTR_Sector>();
		int numChildren = Children.Count;
		for(int childIndex = 0; childIndex < numChildren; ++childIndex)
		{
			SECTR_Member.Child child = Children[childIndex];
			if(child.renderer && !SECTR_Geometry.BoundsContainsBounds(TotalBounds, child.rendererBounds))
			{
				GetContaining(ref overlappingSectors, child.rendererBounds);
				if(overlappingSectors.Count > 1)
				{
					sharedChildren.Add(child);
					continue;
				}
			}
			if(child.light && !SECTR_Geometry.BoundsContainsBounds(TotalBounds, child.lightBounds))
			{
				GetContaining(ref overlappingSectors, child.lightBounds);
				if(overlappingSectors.Count > 1)
				{
					sharedChildren.Add(child);
					continue;
				}
			}
			if(child.terrain && !SECTR_Geometry.BoundsContainsBounds(TotalBounds, child.terrainBounds))
			{
				GetContaining(ref overlappingSectors, child.terrainBounds);
				if(overlappingSectors.Count > 1)
				{
					sharedChildren.Add(child);
					continue;
				}
			}
		}
		return sharedChildren;
	}

	public static Color SectorColor = new Color(0,1,1,0.8f);
	#endif
	#endregion
	
	#region Portal System Interface
	/// Informs the Sector that a Portal is connected into it.
	/// Should only be called by SECTR_Portal.
	public void Register(SECTR_Portal portal)
	{
		// Should this be an assert? Probably?
		if(!portals.Contains(portal))
		{
			portals.Add(portal);
		}
	}

	/// Informs the Sector that a Portal is no longer connected into it.
	/// Should only be called by SECTR_Portal.
	public void Deregister(SECTR_Portal portal)
	{
		// Assert that we're contained?
		portals.Remove(portal);
	}
	
	/// Informs the Sector that a Member is in it.
	/// Should only be called by SECTR_Member.
	public void Register(SECTR_Member member)
	{
		members.Add(member);
	}
	
	/// Informs the Sector that a Member is no longer part in it.
	/// Should only be called by SECTR_Member.
	public void Deregister(SECTR_Member member)
	{
		members.Remove(member);
	}
	#endregion
	
	#region Unity Interface
	protected override void OnEnable()
	{
		allSectors.Add(this);
		if(TopTerrain || BottomTerrain || RightTerrain || LeftTerrain)
		{
			ConnectTerrainNeighbors();
		}
		base.OnEnable();
    }
	
	protected override void OnDisable()
	{
		// No need to loop through our portals, as they will null themselves out.
		List<SECTR_Member> originalMembers = new List<SECTR_Member>(members);
		int numMembers = originalMembers.Count;
		for(int memberIndex = 0; memberIndex < numMembers; ++memberIndex)
		{
			SECTR_Member member = originalMembers[memberIndex];
			if(member)
			{
				member.SectorDisabled(this);
			}
		}
		allSectors.Remove(this);
		base.OnDisable();
	}

	#if UNITY_EDITOR
	protected void OnDrawGizmos() 
	{
		Gizmos.color = SectorColor;
		Gizmos.DrawWireCube(TotalBounds.center, TotalBounds.size); 
	}

	protected override void OnDrawGizmosSelected() 
	{
		Bounds bounds = TotalBounds;
		Gizmos.color = SectorColor;
		Gizmos.DrawWireCube( bounds.center, bounds.size ); 

		if(enabled)
		{
			// Render links to neighbor Sectors.
			Gizmos.color = SECTR_Portal.ActivePortalColor;
			int numNeighbors = portals.Count;
			for(int neighborIndex = 0; neighborIndex < numNeighbors; ++neighborIndex)
			{
				SECTR_Portal portal = portals[neighborIndex];
				Gizmos.DrawLine(TotalBounds.center, portal.Center);
			}

			Gizmos.color = Color.red;
			List<SECTR_Member.Child> sharedChildren = GetSharedChildren();
			int numSharedChildren = sharedChildren.Count;
			for(int childIndex = 0; childIndex < numSharedChildren; ++childIndex)
			{
				SECTR_Member.Child child = sharedChildren[childIndex];
				Bounds totalChildBounds = new Bounds(child.gameObject.transform.position, Vector3.zero);
				if(child.renderer)
				{
					totalChildBounds.Encapsulate(child.rendererBounds);
				}
				if(child.light)
				{
					totalChildBounds.Encapsulate(child.lightBounds);
				}
				if(child.terrain)
				{
					totalChildBounds.Encapsulate(child.terrainBounds);
				}
				Gizmos.DrawWireCube(totalChildBounds.center, totalChildBounds.size);
			}
		}
    }
	#endif

	protected static Terrain GetTerrain(SECTR_Sector sector)
	{
		if(sector)
		{
			SECTR_Member realSector = sector.childProxy ? sector.childProxy : sector;
			return realSector.GetComponentInChildren<Terrain>();
		}
		else
		{
			return null;
		}
	}
	#endregion
}
