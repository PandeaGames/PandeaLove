// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SECTR_Sector))]
[CanEditMultipleObjects]
public class SECTR_SectorEditor : SECTR_MemberEditor
{
	bool terrainFoldout = false;

	public override void OnInspectorGUI()
	{
		SECTR_Sector mySector = (SECTR_Sector)target;
		 
		base.OnInspectorGUI();
		List<SECTR_Member.Child> sharedChildren = mySector.GetSharedChildren();
		if(sharedChildren.Count > 0 && GUILayout.Button(new GUIContent("Fix Shared Children", "Adds Member components to any children that extend beyond this Sector and into other sectors.")))
		{
			MakeSharedChildrenMembers(mySector, sharedChildren, "Fix Shared Children");
		}

		if(mySector.GetComponentInChildren<Terrain>())
		{
			terrainFoldout = EditorGUILayout.Foldout(terrainFoldout, "Terrain Connections");
			if(terrainFoldout)
			{
				serializedObject.Update();
				DrawProperty("TopTerrain");
				DrawProperty("BottomTerrain");
				DrawProperty("LeftTerrain");
				DrawProperty("RightTerrain");

				if(GUILayout.Button("Reconnect Neighbors"))
				{
					mySector.ConnectTerrainNeighbors();
				}
				serializedObject.ApplyModifiedProperties();
			}
		}
	}

	public static void MakeSharedChildrenMembers(SECTR_Sector sector, List<SECTR_Member.Child> sharedChildren, string undoName)
	{
		int numSharedChildren = sharedChildren.Count;
		for(int childIndex = 0; childIndex < numSharedChildren; ++childIndex)
		{
			SECTR_Member.Child child = sharedChildren[childIndex];
			bool hasMemberParent = false;
			Transform parent = child.gameObject.transform;
			while(parent != null)
			{
				if(parent.gameObject != sector.gameObject && parent.GetComponent<SECTR_Member>())
				{
					hasMemberParent = true;
					break;
				}
				else
				{
					parent = parent.parent;
				}
			}
			if(!hasMemberParent)
			{
				SECTR_Member newMember = child.gameObject.AddComponent<SECTR_Member>();
				SECTR_Undo.Created(newMember, undoName);
			}
		}
		sector.ForceUpdate(true);
	}
}
