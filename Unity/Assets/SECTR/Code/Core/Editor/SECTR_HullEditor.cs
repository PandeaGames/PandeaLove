// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(SECTR_Hull))]
public abstract class SECTR_HullEditor : SECTR_Editor
{
	#region Members
	protected bool createHull = false;
	protected GameObject lastSelectedObject = null;
	protected RaycastHit lastHit;
	protected Vector3 closestVert;
	protected bool closesetVertIsValid = false;
	protected List<Vector3> newHullVerts = new List<Vector3>(8);

	private GameObject collisionProxy = null;
	private bool collisionIsTerrain = false;
	#endregion

	#region Unity Interface
	public void OnDisable()
	{
		if(target)
		{
			_EndNewHull((SECTR_Hull)target, true);
		}
		if(lastSelectedObject) 
		{
			_EndSelection();
		}
	}
	#endregion

	#region Private Methods
	protected void _DrawHullEditor(SECTR_Hull myHull)
	{
		// Draw Polygon
		Handles.color = Color.green;
		switch(newHullVerts.Count)
		{
		case 0:
			break;
		case 1:
			Handles.DrawSolidDisc(newHullVerts[0], lastHit.normal, .1f);
			break;
		case 2:
			Handles.DrawLine(newHullVerts[0], newHullVerts[1]);
			break;
		default:
			Handles.DrawPolyLine(newHullVerts.ToArray());
			break;
		}
		
		Handles.color = closesetVertIsValid ? Color.green : Color.red;
		Handles.DrawSolidDisc(closestVert, lastHit.normal, .1f);
		if(closesetVertIsValid && newHullVerts.Count > 0)
		{
			Handles.DrawLine(newHullVerts[newHullVerts.Count - 1], closestVert);
		}
	}

	protected void _EditHull(SECTR_Hull myHull)
	{
		HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
		if(Event.current.type == EventType.mouseMove)
		{
			_ComputeCursorVert();
			closesetVertIsValid = true;
			if(newHullVerts.Count > 2)
			{
				Vector3 firstVert = newHullVerts[0];
				if(Vector3.SqrMagnitude(closestVert - firstVert) < SECTR_Geometry.kVERTEX_EPSILON)
				{
					closestVert = firstVert;
					closesetVertIsValid = true;
				}
				else
				{
					Plane hullPlane = new Plane(newHullVerts[0], newHullVerts[1], newHullVerts[2]);
					closesetVertIsValid = hullPlane.GetDistanceToPoint(closestVert) < SECTR_Geometry.kVERTEX_EPSILON;
					if(closesetVertIsValid)
					{
						List<Vector3> newVerts = new List<Vector3>(newHullVerts);
						newVerts.Add(closestVert);
						closesetVertIsValid = SECTR_Geometry.IsPolygonConvex(newVerts.ToArray());
					}
				}
			}
		}
		else if(Event.current.type == EventType.mouseUp && Event.current.button == 0 && !Event.current.alt && !Event.current.control)
		{
			if(closesetVertIsValid)
			{
				if(!newHullVerts.Contains(closestVert))
				{
					newHullVerts.Add(closestVert);
				}
				else if(newHullVerts.Count >= 3)
				{
					_CompleteHull(myHull);
					HandleUtility.Repaint();
				}
			}
		}
		else if(Event.current.type == EventType.keyUp && Event.current.keyCode == KeyCode.Return)
		{
			if(newHullVerts.Count >= 3 || (myHull.ForceEditHull && newHullVerts.Count == 0))
			{
				_CompleteHull(myHull);
				HandleUtility.Repaint();
			}
		}
		else if(Event.current.type == EventType.keyUp && Event.current.keyCode == KeyCode.Escape)
		{
			_EndNewHull(myHull, true);
		}
	}

	protected void _ComputeCursorVert()
	{
		GameObject selected = HandleUtility.PickGameObject(Event.current.mousePosition, false);
		if(selected && (selected.GetComponent<MeshFilter>() || selected.GetComponent<Terrain>()))
		{
			Collider collider = null;
			if(selected != lastSelectedObject)
			{
				if(collisionProxy && !collisionIsTerrain)
				{
					GameObject.DestroyImmediate(collisionProxy);
				}
				if(selected.GetComponent<Terrain>())
				{
					collisionProxy = selected;
					collider = collisionProxy.GetComponent<TerrainCollider>();
					collisionIsTerrain = true;
				}
				else
				{
					collisionProxy = new GameObject("SECTR Collision Proxy");
					collisionProxy.hideFlags = HideFlags.HideAndDontSave;
					collisionProxy.transform.parent = selected.transform.parent;
					collisionProxy.transform.localPosition = selected.transform.localPosition;
					collisionProxy.transform.localRotation = selected.transform.localRotation;
					collisionProxy.transform.localScale = selected.transform.localScale;
					MeshCollider meshCollider = collisionProxy.AddComponent<MeshCollider>();
					meshCollider.sharedMesh = selected.GetComponent<MeshFilter>().sharedMesh;
					collider = meshCollider;
					collisionIsTerrain = false;
				}
				lastSelectedObject = selected;
			}
			else
			{
				if(collisionIsTerrain)
				{
					collider = collisionProxy.GetComponent<TerrainCollider>();
				}
				else
				{
					collider = collisionProxy.GetComponent<MeshCollider>();
				}
			}
			Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			float hitDistance = 10000f;
			collider.Raycast(ray, out lastHit, hitDistance);

			if(collisionIsTerrain)
			{
				closestVert = collisionProxy.transform.worldToLocalMatrix.MultiplyPoint3x4(lastHit.point);
				closestVert.y = collisionProxy.GetComponent<Terrain>().SampleHeight(lastHit.point);
				closestVert = collisionProxy.transform.localToWorldMatrix.MultiplyPoint3x4(closestVert);
			}
			else
			{
				Vector3 hitPointLS = collisionProxy.transform.worldToLocalMatrix.MultiplyPoint3x4(lastHit.point);
				float bestDistanceSqr = float.MaxValue;
				MeshCollider meshCollider = (MeshCollider)(collider);
				int numVerts = meshCollider.sharedMesh.vertexCount;
				for(int vertexIndex = 0; vertexIndex < numVerts; ++vertexIndex)
				{
					Vector3 vert = meshCollider.sharedMesh.vertices[vertexIndex];
					float vertDistanceSqr = Vector3.SqrMagnitude(hitPointLS - vert);
					if(vertDistanceSqr < bestDistanceSqr)
					{
						bestDistanceSqr = vertDistanceSqr;
						closestVert = vert;
					}
				}
				closestVert = collisionProxy.transform.localToWorldMatrix.MultiplyPoint3x4(closestVert);
			}

		}
		else
		{
			lastSelectedObject = null;
			if(collisionProxy && !collisionIsTerrain)
			{
				GameObject.DestroyImmediate(collisionProxy);
			}
			Plane groundPlane = new Plane(Vector3.up, 0);
			Ray selectRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			float distance = 0;
			groundPlane.Raycast(selectRay, out distance);
			closestVert = selectRay.origin + selectRay.direction * distance;
			lastHit.normal = Vector3.up;
		}
	}

	protected void _EndSelection()
	{
		if(collisionProxy && !collisionIsTerrain)
		{
			GameObject.DestroyImmediate(collisionProxy);
		}
		collisionProxy = null;
		lastSelectedObject = null;
		closesetVertIsValid = false;
	}

	void _EndNewHull(SECTR_Hull myHull, bool creationFailed)
	{
		if(createHull || myHull.ForceEditHull)
		{
			_EndSelection();
			newHullVerts.Clear();
			createHull = false;
			if(myHull.ForceEditHull && creationFailed)
			{
				DestroyImmediate(myHull.gameObject);
			}
			else
			{
				myHull.ForceEditHull = false;
			}
		}
	}
	
	void _CompleteHull(SECTR_Hull myHull)
	{
		int numNewVerts = newHullVerts.Count;
		if(numNewVerts >= 3)
		{
			Plane hullPlane = new Plane(newHullVerts[0], newHullVerts[1], newHullVerts[2]);
			Vector3 hullNormal = hullPlane.normal;
			
			// For new hulls, set their xform to match the hull geo.
			if(myHull.ForceEditHull && myHull.CenterOnEdit)
			{
				Vector3 newPos = Vector3.zero;
				for(int vertIndex = 0; vertIndex < numNewVerts; ++vertIndex)
				{
					newPos += newHullVerts[vertIndex];
				}
				newPos /= newHullVerts.Count;
				myHull.transform.position = newPos;
				myHull.transform.forward = hullNormal;
			}
			
			// Constructu a new mesh.
			Mesh newMesh = new Mesh();
			newMesh.name = myHull.name;
			Vector3[] newVerts = new Vector3[numNewVerts];
			Vector3[] newNormals = new Vector3[numNewVerts];
			Vector2[] newUVs = new Vector2[numNewVerts];
			
			// Compute new positions and normals, which are always in hull local space.
			Vector3 localNormal = myHull.transform.worldToLocalMatrix.MultiplyVector(hullNormal);
			Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			for(int vertIndex = 0; vertIndex < numNewVerts; ++vertIndex)
			{
				Vector3 localPosition = myHull.transform.worldToLocalMatrix.MultiplyPoint3x4(newHullVerts[vertIndex]);
				newVerts[vertIndex] = localPosition;
				newNormals[vertIndex] = localNormal;
				min = Vector3.Min(min, localPosition);
				max = Vector3.Max(max, localPosition);
			}
			// Compute a planar projection for the UVs.
			Vector3 uvScalar = new Vector3(1f / max.x - min.x, 1f / max.y - min.y, 1);
			for(int vertIndex = 0; vertIndex < numNewVerts; ++vertIndex)
			{
				newUVs[vertIndex] = Vector3.Scale(newVerts[vertIndex],  uvScalar);
			}
			// Triangle indices assume a CW sorting of the verts.
			int numTriangles = numNewVerts - 2;
			int[] triangles = new int[numTriangles * 3];
			for(int triIndex = 0; triIndex < numTriangles; ++triIndex)
			{
				triangles[triIndex*3] = 0;
				triangles[triIndex*3+1] = triIndex+1;
				triangles[triIndex*3+2] = triIndex+2;
			}
			// Fill out the mesh stuffs.
			newMesh.vertices = newVerts;
			newMesh.normals = newNormals;
			newMesh.uv = newUVs;
			newMesh.triangles = triangles;
			
			// Now create a new, unique mesh asset for the hull.
			// We use assets instead of storing geometry in the scene to ensure that everything serializes properly.
			string sceneDir = null;
			string sceneName = null;
			string exportDir = SECTR_Asset.MakeExportFolder("Portals", false, out sceneDir, out sceneName);
			string newAssetName = exportDir + newMesh.name + ".asset";
			newAssetName = AssetDatabase.GenerateUniqueAssetPath(newAssetName);
			AssetDatabase.CreateAsset(newMesh, newAssetName);
			
			// Let the hull know that we've modified it in an undo friendly way.
			SECTR_Undo.Record(myHull, "Created Portal");
			myHull.HullMesh = newMesh;
		}
		_EndNewHull(myHull, false);
	}
	#endregion
}
