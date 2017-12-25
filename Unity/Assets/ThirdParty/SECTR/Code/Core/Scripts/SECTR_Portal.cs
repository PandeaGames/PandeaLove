// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// \ingroup Core
/// Portals define the logical and geometric connection between two SECTR_Sector objects.
/// 
/// If a Sector is a room then a Portal is like a window or doorway into or
/// out of that room. Portals not only define which portals are connected to 
/// each other, but the shape and size of those connections. Portals can have as 
/// many vertices as necessary, but they must be convex and planar. 
/// 
/// Like the rest of the system, Portals are completely dynamic, and can
/// translate/rotate/scale at runtime without any performance penalty. They can
/// also be turned on and off (i.e. when a door opens or closes) simply
/// by setting their enabled flag on or off.
[ExecuteInEditMode]
[AddComponentMenu("SECTR/Core/SECTR Portal")]
public class SECTR_Portal : SECTR_Hull {
	#region Private Details
	[SerializeField] [HideInInspector] private SECTR_Sector frontSector = null;
	[SerializeField] [HideInInspector] private SECTR_Sector backSector = null;

	// Used in some traversals.
	private bool visited = false;

	// Quick access cache for all SectorPortals in the world.
	private static List<SECTR_Portal> allPortals = new List<SECTR_Portal>(128);
	#endregion

	#region Public Interface
	/// The set of bitflags that can be set on (and tested for) a given portal.
	/// Users are encouraged to add to this set, but after the Reserved range
	/// to avoid interference with future updates.
	[System.Flags]
	public enum PortalFlags
	{
		/// Portal cannot be seen through.
		Closed 	= 1 << 0,				
		/// Portal is locked.
		Locked 	= 1 << 1,				
		/// Portal will be visible independent of geometry (but normal direction still matters).
		PassThrough = 1 << 2,			
		//ReservedEnd = 1 << 23,
	};
	
	[SECTR_ToolTip("Flags for this Portal. Used in graph traversals and the like.", null, typeof(PortalFlags))]
	public PortalFlags Flags = 0;

	/// Returns a list of all enabled portals.
	public static List<SECTR_Portal> All
	{
		get { return allPortals; }
	}

	/// Accessor for the Sector link on the front side of the SectorPortal.
	/// When set, properly notifies the previous Sector of connection changes.
	public SECTR_Sector FrontSector
	{
		set 
		{
			if(frontSector != value)
			{
				if(frontSector)
				{
					frontSector.Deregister(this);
				}
				frontSector = value;
				if(frontSector)
				{
					frontSector.Register(this);
				}
			}
		}
		get { return frontSector && frontSector.enabled ? frontSector : null; }
	}

	/// Accessor for the Sector link on the back side of the SectorPortal.
	/// When set, properly notifies the previous Sector of connection changes.
	public SECTR_Sector BackSector
	{
		set
		{
			if(backSector != value)
			{
				if(backSector)
				{
					backSector.Deregister(this);
				}
				backSector = value;
				if(backSector)
				{
					backSector.Register(this);
				}
			}
		}
		get { return backSector && backSector.enabled ? backSector : null; }
	}

	/// Utility property for tracking during graph walks.
	public bool Visited
	{
		get { return visited; }
		set { visited = value; }
	}

	/// Creates an iterator that steps through all of the connected Sectors.
	/// Helpful way to make some iterating client code more generic.
	public IEnumerable<SECTR_Sector> GetSectors()
	{
		yield return FrontSector;
		yield return BackSector;
	}

	/// Sets a particular flag on this Portal to be true.
	/// <param name="flag">The flag to make true.</param>
	/// <param name="on">Sets the flag on or off.</param> 
	public void SetFlag(PortalFlags flag, bool on)
	{
		if(on)
		{
			Flags |= flag;
		}
		else
		{
			Flags &= ~flag;
		}
	}

	#if UNITY_EDITOR	
	// Debug rendering stuff.
	public static Color ActivePortalColor = Color.green;
	public static Color InactivePortalColor = Color.gray;
	public static Color FrontAnchorColor = new Color(1, 1, 0, 0.8f);
	public static Color BackAnchorColor = new Color(1, 0, 1, 0.8f);
	
	public Vector3 FrontAnchorPosition
	{
		get { ComputeVerts(); return transform.localToWorldMatrix.MultiplyPoint3x4(meshNormal); }
	}
	
	public Vector3 BackAnchorPosition
	{
		get { ComputeVerts(); return transform.localToWorldMatrix.MultiplyPoint3x4(-meshNormal); }
	}
	#endif
	#endregion
	
	#region Unity Interface
	void OnEnable()
	{
		allPortals.Add(this);
		if(frontSector)
		{
			frontSector.Register(this);
		}
		if(backSector)
		{
			backSector.Register(this);
		}
	}
	
	void OnDisable()
	{
		allPortals.Remove(this);
		if(frontSector)
		{
			frontSector.Deregister(this);
		}
		if(backSector)
		{
			backSector.Deregister(this);
		}
	}

	#if UNITY_EDITOR
	protected override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		DrawHull(enabled ? ActivePortalColor : InactivePortalColor);
		DrawNormal(enabled ? FrontAnchorColor : InactivePortalColor, false);
		DrawNormal(enabled ? BackAnchorColor : InactivePortalColor, true);
	}
	#endif
	#endregion
}

