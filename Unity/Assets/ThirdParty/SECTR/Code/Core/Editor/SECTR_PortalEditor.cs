// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SECTR_Portal))] 
[CanEditMultipleObjects]
public class SECTR_PortalEditor : SECTR_HullEditor 
{
	#region Members	
	private bool pickBack = false;
	private bool pickFront = false;
	private GUIStyle boxStyle = null;
	private GUIStyle buttonStyle = null;
	private GUIStyle nullStyle = null;
	private SerializedProperty frontProp;
	private SerializedProperty backProp;
	#endregion
	
	#region Unity Interface	
	public override void OnEnable()
	{
		base.OnEnable();
		frontProp = serializedObject.FindProperty("frontSector");
		backProp = serializedObject.FindProperty("backSector");
	}

    public void OnSceneGUI() 
	{
		SECTR_Portal myPortal = (SECTR_Portal)target;

		if(boxStyle == null)
		{
			boxStyle = new GUIStyle(GUI.skin.box);
			boxStyle.alignment = TextAnchor.UpperCenter;
			boxStyle.fontSize = 15;
			boxStyle.normal.textColor = Color.white;
		}
		
		if(buttonStyle == null)
		{
			buttonStyle = new GUIStyle(GUI.skin.button);
			buttonStyle.alignment = TextAnchor.UpperCenter;
			buttonStyle.fontSize = 12;
			buttonStyle.normal.textColor = Color.white;
		}

		if(nullStyle == null)
		{
			nullStyle = new GUIStyle();
		}

		// Viewport GUI Drawing
		_DrawViewportGUI(myPortal);
		
		// Input
		if(pickBack || pickFront)
		{
			_PickSector(myPortal);
		}
		else if((createHull || myPortal.ForceEditHull) && !Application.isPlaying)
		{
			_EditHull(myPortal);
		}

		// Input may destroy this object.
		if(target == null)
		{
			return;
		}

		// Viewport 3D drawing
		if(!createHull && !myPortal.ForceEditHull)
		{
			_DrawSectorLinks(myPortal);
		}
		
		if(createHull || myPortal.ForceEditHull)
		{
			_DrawHullEditor(myPortal);
		}
		else if(pickBack || pickFront)
		{
			Handles.color = pickFront ? SECTR_Portal.FrontAnchorColor : SECTR_Portal.BackAnchorColor;
			Handles.DrawSolidDisc(closestVert, lastHit.normal, .1f);
			SECTR_Sector sector = _GetSectorFromSelection();
			if(sector != null)
			{
				Handles.Label(closestVert, sector.name);
			}
		}
    }
	
	public override void OnInspectorGUI() 
	{
		SECTR_Portal myPortal = (SECTR_Portal)target;
		SECTR_Sector newFront = ObjectField<SECTR_Sector>("Front Sector", "Reference to the Sector on the front side of this Portal", myPortal.FrontSector, true);
		SECTR_Sector newBack = ObjectField<SECTR_Sector>("Back Sector", "Reference to the Sector on the back side of this Portal", myPortal.BackSector, true);

		// Only apply changes if things are actually different.
		// Note that the code below duplicates some functionality from SECTR_Portal's
		// accessors, but I can'f figure out any other way to get the SerializedProperty
		// multi-select compatable Undo to work...
		if(myPortal.FrontSector != newFront || myPortal.BackSector != newBack)
		{
			serializedObject.Update();
			if(myPortal.FrontSector != newFront)
			{
				if(myPortal.FrontSector)
				{
					myPortal.FrontSector.Deregister(myPortal);
				}
				frontProp.objectReferenceValue = newFront;
				if(myPortal.FrontSector)
				{
					myPortal.FrontSector.Register(myPortal);
				}
			}
			if(myPortal.BackSector != newBack)
			{
				if(myPortal.BackSector)
				{
					myPortal.BackSector.Deregister(myPortal);
				}
				backProp.objectReferenceValue = newBack;
				if(myPortal.BackSector)
				{
					myPortal.BackSector.Register(myPortal);
				}
			}
			serializedObject.ApplyModifiedProperties();
		}

		base.OnInspectorGUI();
    }
	#endregion

	#region Private Methods
	void _DrawViewportGUI(SECTR_Portal myPortal)
	{
		Handles.BeginGUI();

		float width = 400;
		if(pickBack || pickFront)
		{
			float height = 100;
			GUI.Box(new Rect((Screen.width * 0.5f) - (width * 0.5f), Screen.height - height, width, height),
			        "Selecting " + (pickBack ? "back" : "front") + " Sector of " + myPortal.name + ".\n" + 
			        (_GetSectorFromSelection() != null ? "Left Click to select." : "") + "\nEsc to cancel.", boxStyle);
		}
		else if(createHull || myPortal.ForceEditHull)
		{
			float height = 100;
			string returnText = "";
			if(newHullVerts.Count >= 3)
			{
				returnText = "Return to complete.";
			}
			else if(newHullVerts.Count == 0 && myPortal.ForceEditHull)
			{
				returnText = "Return to create empty portal.";
			}
			GUI.Box(new Rect((Screen.width * 0.5f) - (width * 0.5f), Screen.height - height, width, height),
			        "Drawing geometry for " + myPortal.name + ".\n" + 
			        (closesetVertIsValid ? "Left Click to add vert. " : "") + returnText + "\nEsc to cancel.",
			        boxStyle);
		}
		else if(Selection.gameObjects.Length == 1)
		{
			float height = 100;
			GUILayout.BeginArea(new Rect((Screen.width * 0.5f) - (width * 0.5f), Screen.height - height, width, height));
			if(GUILayout.Button(new GUIContent(myPortal.HullMesh ? "Redraw Portal" : "Draw Portal", "Lets you (re)create the geometry for this Portal"), buttonStyle))
			{
				createHull = true;
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button(new GUIContent("Pick Front Sector", "Provides an in-viewport interface for picking the front Sector."), buttonStyle))
			{
				pickFront = true;
			}
			if(GUILayout.Button(new GUIContent("Swap Sectors", "Swaps the front and back Sectors, in case they are backwards."), buttonStyle))
			{
				_SwapSectors(myPortal);
			}
			if(GUILayout.Button(new GUIContent("Pick Back Sector", "Provides an in-viewport interface for picking the front Sector."), buttonStyle))
			{
				pickBack = true;
			}
			GUILayout.EndHorizontal();

			GUILayout.EndArea();
		}
		Handles.EndGUI();
	}

	void _DrawSectorLinks(SECTR_Portal myPortal)
	{
		nullStyle.normal.textColor = SECTR_Portal.FrontAnchorColor;
		Handles.Label(myPortal.FrontAnchorPosition, "F", nullStyle);
		nullStyle.normal.textColor = SECTR_Portal.BackAnchorColor;
		Handles.Label(myPortal.BackAnchorPosition, "B", nullStyle);
		if(myPortal.FrontSector != null)
		{
			Handles.color = SECTR_Portal.FrontAnchorColor;
			Handles.DrawLine(myPortal.FrontAnchorPosition, myPortal.FrontSector.TotalBounds.center);
		}
		
		if(myPortal.BackSector != null)
		{
			Handles.color = SECTR_Portal.BackAnchorColor;
			Handles.DrawLine(myPortal.BackAnchorPosition, myPortal.BackSector.TotalBounds.center);
		}
	}

	void _PickSector(SECTR_Portal myPortal)
	{
		HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
		if(Event.current.type == EventType.mouseMove)
		{
			_ComputeCursorVert();
		}
		else if(Event.current.type == EventType.mouseUp && Event.current.button == 0 && !Event.current.alt && !Event.current.control)
		{
			SECTR_Sector sector = _GetSectorFromSelection();
			if(sector)
			{
				SECTR_Undo.Record(myPortal, "Assign Sector to Portal.");
				if(pickBack)
				{
					myPortal.BackSector = sector;
				}
				else
				{
					myPortal.FrontSector = sector;
				}
				EditorUtility.SetDirty(myPortal);
				pickFront = false;
				pickBack = false;
				_EndSelection();
			}
		}
		else if(Event.current.type == EventType.keyUp && Event.current.keyCode == KeyCode.Escape)
		{
			pickBack = false;
			pickFront = false;
			_EndSelection();
		}
	}

	void _SwapSectors(SECTR_Portal myPortal)
	{
		SECTR_Undo.Record(myPortal, "Swap Portal Sectors");
		SECTR_Sector oldFront = myPortal.FrontSector;
		SECTR_Sector oldBack = myPortal.BackSector;
		myPortal.FrontSector = null;
		myPortal.BackSector = null;
		myPortal.FrontSector = oldBack;
		myPortal.BackSector = oldFront;
		EditorUtility.SetDirty(myPortal);
	}

	SECTR_Sector _GetSectorFromSelection()
	{
		GameObject selected = lastSelectedObject;
		SECTR_Sector sector = null;
		while(selected != null && sector == null)
		{
			sector = selected.GetComponent<SECTR_Sector>();
			selected = selected.transform.parent ? selected.transform.parent.gameObject : null;
		}
		return sector;
	}
	#endregion
}

