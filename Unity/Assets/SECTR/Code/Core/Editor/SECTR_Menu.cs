// Copyright (c) 2014 Make Code Now! LLC

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class SECTR_Menu : MonoBehaviour 
{
	protected const int windowPriority = 1000000;
	protected const int createPriority = 1000000;
	protected const int assetPriority = 1000000;
	protected const int corePriority = createPriority + 0;
	protected const int audioPriority = createPriority + 500;
	protected const int streamPriority = createPriority + 1000;
	protected const int visPriority = createPriority + 1500;
	protected const int completePriority = createPriority + 2000;
	protected const int devPriority = createPriority + 10000;

	protected const string createMenuRootPath = "GameObject/Create Other/SECTR/";
	protected const string assetMenuRootPath = "Assets/Create/SECTR/";
	protected const string windowMenuRootPath = "Window/SECTR/";
	
	protected static GameObject CreateTriggerFromPortal(SECTR_Portal portal, string newName)
	{
		GameObject newGameObject = null;
		if(portal)
		{
			newGameObject = new GameObject(newName);
			BoxCollider newCollider = newGameObject.AddComponent<BoxCollider>();
			newCollider.isTrigger = true;

			newGameObject.transform.position = portal.transform.position;
			newGameObject.transform.rotation = portal.transform.rotation;
			Vector3 newSize = Vector3.Scale(portal.HullMesh ? portal.HullMesh.bounds.size : Vector3.one, portal.transform.lossyScale);
			float maxSize = Mathf.Max(newSize.x, Mathf.Max(newSize.y, newSize.z));
			if(Mathf.Abs(newSize.x) < 0.001f)
			{
				newSize.x = maxSize;
			}
			else if(Mathf.Abs(newSize.y) < 0.001f)
			{
				newSize.y = maxSize;
			}
			else
			{
				newSize.z = maxSize;
			}
			newCollider.size = newSize;
		}
		return newGameObject;
	}

	protected static GameObject CreateDoor<T>(string newName) where T : SECTR_Door
	{
		string undoName = "Create " + newName;
		GameObject newGameObject;
		SECTR_Portal selectedPortal = Selection.activeGameObject ? Selection.activeGameObject.GetComponent<SECTR_Portal>() : null;
		if(selectedPortal)
		{
			newGameObject = CreateTriggerFromPortal(selectedPortal, newName);
			T newDoor = newGameObject.AddComponent<T>();
			newDoor.Portal = selectedPortal;
		}
		else
		{
			newGameObject = CreateGameObject(newName);
			BoxCollider newCollider = newGameObject.AddComponent<BoxCollider>();
			newCollider.isTrigger = true;
			newGameObject.AddComponent<T>();
		}
		
		SECTR_Undo.Created(newGameObject, undoName);
		Selection.activeGameObject = newGameObject;
		return newGameObject;
	}

	protected static GameObject CreateGameObject(string name)
	{
		GameObject newGameObject = new GameObject(name);
		if(SceneView.lastActiveSceneView && SceneView.lastActiveSceneView.camera)
		{
			Camera camera = SceneView.lastActiveSceneView.camera;
			RaycastHit hit;
			if(Physics.Raycast(camera.transform.position, camera.transform.forward, out hit))
			{
				newGameObject.transform.position = hit.point;
			}
			else
			{
				Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
				float enter;
				if(groundPlane.Raycast(new Ray(camera.transform.position, camera.transform.forward), out enter))
				{
					newGameObject.transform.position = camera.transform.position + camera.transform.forward * enter;
				}
			}
		}
		return newGameObject;
	}

}