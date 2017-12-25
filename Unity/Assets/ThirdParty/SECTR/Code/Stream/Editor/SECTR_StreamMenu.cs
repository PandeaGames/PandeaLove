// Copyright (c) 2014 Make Code Now! LLC

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class SECTR_StreamMenu : SECTR_Menu
{
	const string rootCreatePath = createMenuRootPath + "STREAM/";
	const string createNeighborItem = rootCreatePath + "Neighbor Loader";
	const string createRegionItem = rootCreatePath + "Region Loader";
	const string createTriggerItem = rootCreatePath + "Trigger Loader";
	const string createGroupItem = rootCreatePath + "Group Loader";
	const string createStartItem = rootCreatePath + "Start Loader";
	const string createHibernatorItem = rootCreatePath + "Hibernator";
	const string createDoorItem = rootCreatePath + "Loading Door";
	const int createNeighborPriority = streamPriority + 0;
	const int createRegionPriority = streamPriority + 2;
	const int createTriggerPriority = streamPriority + 5;
	const int createGroupPriority = streamPriority + 10;
	const int createStartPriority = streamPriority + 15;
	const int createHibernatorPriority = streamPriority + 50;
	const int createDoorPriority = streamPriority + 100;

	const string streamWindowItem = windowMenuRootPath + "Stream";
	const int streamWindowPriority = windowPriority;

	[MenuItem(createNeighborItem, false, createNeighborPriority)]
	public static void CreateNeighborLoader()
	{
		string newObjectName = "SECTR Neighbor Loader";
		string undoName = "Created " + newObjectName;
		GameObject newGameObject = CreateGameObject(newObjectName);
		newGameObject.AddComponent<SECTR_NeighborLoader>();
		SECTR_Undo.Created(newGameObject, undoName);
		Selection.activeGameObject = newGameObject;
	}

	[MenuItem(createRegionItem, false, createRegionPriority)]
	public static void CreateRegionLoader()
	{
		string newObjectName = "SECTR Region Loader";
		string undoName = "Created " + newObjectName;
		GameObject newGameObject = CreateGameObject(newObjectName);
		newGameObject.AddComponent<SECTR_RegionLoader>();
		SECTR_Undo.Created(newGameObject, undoName);
		Selection.activeGameObject = newGameObject;
	}

	[MenuItem(createTriggerItem, false, createTriggerPriority)]
	public static void CreateTriggerLoader()
	{
		string newObjectName = "SECTR Trigger Loader";
		string undoName = "Created " + newObjectName;
		GameObject newGameObject;
		SECTR_Portal selectedPortal = Selection.activeGameObject ? Selection.activeGameObject.GetComponent<SECTR_Portal>() : null;
		if(selectedPortal)
		{
			newGameObject = CreateTriggerFromPortal(selectedPortal, newObjectName);
			SECTR_TriggerLoader newLoader = newGameObject.AddComponent<SECTR_TriggerLoader>();
			if(selectedPortal.FrontSector)
			{
				newLoader.Sectors.Add(selectedPortal.FrontSector);
			}
			if(selectedPortal.BackSector)
			{
				newLoader.Sectors.Add(selectedPortal.BackSector);
			}
		}
		else
		{
			newGameObject = CreateGameObject(newObjectName);
			BoxCollider newCollider = newGameObject.AddComponent<BoxCollider>();
			newCollider.isTrigger = true;
			newGameObject.AddComponent<SECTR_TriggerLoader>();
		}

		SECTR_Undo.Created(newGameObject, undoName);
		Selection.activeGameObject = newGameObject;
	}

	[MenuItem(createGroupItem, false, createGroupPriority)]
	public static void CreateGroupLoader()
	{
		string newObjectName = "SECTR Group Loader";
		string undoName = "Created " + newObjectName;
		GameObject newGameObject = CreateGameObject(newObjectName);
		newGameObject.AddComponent<SECTR_GroupLoader>();
		SECTR_Undo.Created(newGameObject, undoName);
		Selection.activeGameObject = newGameObject;
	}

	[MenuItem(createStartItem, false, createStartPriority)]
	public static void CreateStartLoader()
	{
		string newObjectName = "SECTR Start Loader";
		string undoName = "Created " + newObjectName;
		GameObject newGameObject = CreateGameObject(newObjectName);
		newGameObject.AddComponent<SECTR_StartLoader>();
		SECTR_Undo.Created(newGameObject, undoName);
		Selection.activeGameObject = newGameObject;
	}

	[MenuItem(createHibernatorItem, false, createHibernatorPriority)]
	public static void CreateHibernator()
	{
		string newObjectName = "SECTR Hibernator";
		string undoName = "Created " + newObjectName;
		GameObject newGameObject = CreateGameObject(newObjectName);
		newGameObject.AddComponent<SECTR_Hibernator>();
		SECTR_Undo.Created(newGameObject, undoName);
		Selection.activeGameObject = newGameObject;
	}

	[MenuItem(createDoorItem, false, createDoorPriority)]
	public static void CreateLoadingDoor()
	{
		CreateDoor<SECTR_LoadingDoor>("SECTR Loading Door");
	}

	[MenuItem(streamWindowItem, false, streamWindowPriority)]
	public static void OpenStreamWindow()
	{
		// Get existing open window or if none, make a new one:		
		SECTR_StreamWindow window = EditorWindow.GetWindow<SECTR_StreamWindow>("SECTR Stream");
		window.Show();
	}
}