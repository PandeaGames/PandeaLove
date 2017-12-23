// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System;
using System.Collections;

/// \ingroup Core
/// Abstract base class that implements planar, convex hulls, for use in SECTR_Portal, SECTR_Occluder
/// and other client classes.
///
/// Planar, convex hulls are a common pattern within the framework. They provide a reasonable balance between
/// CPU cost and versatility. In order to allow geometry to be created within Unity or using external
/// modelling programs, Hulls are based on standard Unity Mesh resources, and are lazily converted into a
/// simpler, loop representation at runtime.
public abstract class SECTR_Hull : MonoBehaviour
{
	#region Private Details
	// Mesh related data
	private Mesh previousMesh = null;
	private Vector3[] vertsCW;
	private Vector3 meshCentroid = Vector3.zero;
	protected Vector3 meshNormal = Vector3.forward;
	
	#if UNITY_EDITOR
	// Debug rendering stuff
	private bool hidden = false;
	// Hull editing hooks b/c we can access the associated Editor directly.
	private bool forceEdit = false;
	private bool centerOnEdit = false;
	#endif
	#endregion
	
	#region Public Interface
	[SECTR_ToolTip("Convex, planar mesh that defines the portal shape.")]
	public Mesh HullMesh = null;
	
	/// Returns the verts in clockwise order.
	public Vector3[] VertsCW
	{
		get { ComputeVerts(); return vertsCW; } 
	}

	/// Returns the world space normal of the Hull.
	public Vector3 Normal
	{
		get { ComputeVerts(); return transform.rotation * meshNormal; }
	}

	/// Returns the world space, backwards facing normal of the hull.
	public Vector3 ReverseNormal
	{
		get { ComputeVerts(); return transform.rotation * -meshNormal; }
	}

	/// Returns the world space centroid of the Hull.
	public Vector3 Center
	{
		get { ComputeVerts(); return transform.localToWorldMatrix.MultiplyPoint3x4(meshCentroid); }
	}

	/// Returns the world space plane of this hull.
	public Plane HullPlane
	{
		get
		{
			ComputeVerts(); 
			return new Plane(Normal, Center); 
		}
	}

	/// Returns the world space plane of this hull, but with the normal flipped.
	public Plane ReverseHullPlane
	{
		get
		{
			ComputeVerts(); 
			return new Plane(ReverseNormal, Center);
		}
	}

	// Returs the world space bounding box of this convex hull.
	public Bounds BoundingBox
	{
		get
		{
			Bounds hullBounds = new Bounds(transform.position, Vector3.zero);
			if(HullMesh)
			{
				ComputeVerts();
				if(vertsCW != null)
				{
					Matrix4x4 hullMatrix = transform.localToWorldMatrix;
					int numVerts = vertsCW.Length;
					for(int vertIndex = 0; vertIndex < numVerts; ++vertIndex)
					{
						hullBounds.Encapsulate(hullMatrix.MultiplyPoint3x4(vertsCW[vertIndex]));
					}	
				}
			}
			return hullBounds;
		}
	}
	
	/// Determines whether the given point is inside the extents of the hull.
	/// Distance tolerance will reject points more than that distance from the plane.
	/// <param name="p">The point to test.</param>
	/// <param name="distanceTolerance">The maximum distance to be considered "in the hull".</param> 
	public bool IsPointInHull(Vector3 p, float distanceTolerance)
	{
		// Make sure that all vertex data is up to date.
		ComputeVerts();
		// Convex hull verts are in local space, so we'll do all calcs in that space.
		Vector3 localP = transform.worldToLocalMatrix.MultiplyPoint3x4(p);
		Vector3 projectedP = localP - (Vector3.Dot(localP - meshCentroid, meshNormal) * meshNormal);
		// Reject any points that are too far from the plane.
		if(vertsCW != null && Vector3.SqrMagnitude(localP - projectedP) < (distanceTolerance * distanceTolerance))
		{
			// A point is guaranteed to be inside a convex hull if the angles between
			// that point and all pairs of vertices equal 2 * PI.
			float angleSum = Mathf.PI * 2f;
			int numVerts = vertsCW.Length;
			for(int i = 0; i < numVerts; ++i)
			{
				Vector3 p1 = vertsCW[i] - projectedP;
				Vector3 p2 = vertsCW[(i+1) % numVerts] - projectedP;
				float m1 = p1.magnitude;
				float m2 = p2.magnitude;
				float mProduct = m1 * m2;
				if(mProduct < SECTR_Geometry.kVERTEX_EPSILON)
				{
					// return true if this point is right on top of a vertex
					return true; 
				}
				else
				{
					// Subtract out this wedge angle from the total.
					float cosTheta = Vector3.Dot(p1, p2) / mProduct;
					angleSum -= Mathf.Acos(cosTheta);
				}
			}
			return Mathf.Abs(angleSum) < SECTR_Geometry.kVERTEX_EPSILON;	
		}
		return false;
	}
	
	#if UNITY_EDITOR
	public bool Hidden
	{
		set { hidden = value; }
	}

	public bool ForceEditHull
	{
		set { forceEdit = value; }
		get { return forceEdit; }
	}
	
	public bool CenterOnEdit
	{
		set { centerOnEdit = value; }
		get { return centerOnEdit; }
	}
	#endif
	#endregion

	#region Unity Interface
	#if UNITY_EDITOR
	protected virtual void OnDrawGizmos()
	{
		if(HullMesh != null && !hidden)
		{
			ComputeVerts();

			// Draw an invisible cube so that we can be more easily selected in editor.
			Gizmos.color = new Color(0, 0, 0, 0.0F);
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawCube(HullMesh.bounds.center, HullMesh.bounds.size);
		}
	}
	#endif
	#endregion
	
	#region Protected Methods
	// Transforms the data in HullMesh into a linear list of vertices,
	// computes other mesh based data, and checks for convexity and planarity.
	protected void ComputeVerts()
	{
		if(HullMesh != previousMesh)
		{
			if(HullMesh)
			{
				int numVerts = HullMesh.vertexCount;
				vertsCW = new Vector3[numVerts];
				
				// Compute the hull centroid and initialize the CW verts array.
				meshCentroid = Vector3.zero;
				for(int vertIndex = 0; vertIndex < numVerts; ++vertIndex)
				{
					Vector3 vertex = HullMesh.vertices[vertIndex];
					vertsCW[vertIndex] = vertex;
					meshCentroid += vertex;
				}
				meshCentroid /= HullMesh.vertexCount;
				
				// Compute the hull normal
				meshNormal = Vector3.zero;
				int numNormals = HullMesh.normals.Length;
				for(int normalIndex = 0; normalIndex < numNormals; ++normalIndex)
				{
					meshNormal += HullMesh.normals[normalIndex];
				}
				meshNormal /= HullMesh.normals.Length;
				meshNormal.Normalize();
				
				// Project the points onto a perfect plane and determine if the mesh is non-planar.
				bool meshIsPlanar = true;
				for(int vertIndex = 0; vertIndex < numVerts; ++vertIndex)
				{
					Vector3 vertex = vertsCW[vertIndex];
					Vector3 projectedVert = vertex - Vector3.Dot(vertex - meshCentroid, meshNormal) * meshNormal;
					meshIsPlanar = meshIsPlanar && Vector3.SqrMagnitude(vertex - projectedVert) < SECTR_Geometry.kVERTEX_EPSILON;
					vertsCW[vertIndex] = projectedVert;
				}
				
				if(!meshIsPlanar)
				{
					Debug.LogWarning("Occluder mesh of " + name + " is not planar!");
				}
				
				// Reorder the verts to be clockwise about the normal
				Array.Sort(vertsCW, delegate(Vector3 a, Vector3 b)
				{
					return SECTR_Geometry.CompareVectorsCW(a, b, meshCentroid, meshNormal) * -1;
				});
				
				if(!SECTR_Geometry.IsPolygonConvex(vertsCW))
				{
					Debug.LogWarning("Occluder mesh of " + name + " is not convex!");
				}
			}
			else
			{
				meshNormal = Vector3.zero;
				meshCentroid = Vector3.zero;
				vertsCW = null;
			}
			previousMesh = HullMesh;
		}
	}

    #if UNITY_EDITOR
	protected void DrawHull(Color hullColor)
	{
		if(HullMesh != null && vertsCW != null && !hidden)
		{
			Gizmos.color = hullColor;
			for(int vertIndex = 0; vertIndex < vertsCW.Length; ++vertIndex)
			{
				int nextVert = (vertIndex + 1) % vertsCW.Length;
				Gizmos.DrawLine(vertsCW[vertIndex], vertsCW[nextVert]);
			}
		}
	}

	protected void DrawNormal(Color normalColor, bool drawReverse)
	{
		if(HullMesh != null && !hidden)
		{
			Gizmos.color = normalColor;
			Gizmos.DrawLine(meshCentroid, meshCentroid + (drawReverse ? -meshNormal : meshNormal));
		}
	}
	#endif
	#endregion

}
