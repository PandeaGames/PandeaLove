// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System.Collections.Generic;

/// \ingroup Core
/// A library of useful geometric functions.
///
/// SECTR is an inherently geometric library, and this class
/// is a common repository for useful geometric methods shared
/// by several classes in the library.
public static class SECTR_Geometry
{
	public const float kVERTEX_EPSILON = 0.001f;
	public const float kBOUNDS_CHEAT = 0.01f;
	
	/// Computes the bounds of the input Light. Area and Directional lights are treated as points as
	/// they have no good representation in SECTR.
	/// <param name="light">The light whose bounds need computing.</param> 
	/// <returns>The world space Bounds of the light.</returns>
	public static Bounds ComputeBounds(Light light)
	{
		Bounds bounds;
		if(light)
		{
			switch(light.type)
			{
			case LightType.Spot:
				Vector3 lightPos = light.transform.position;
				bounds = new Bounds(lightPos, Vector3.zero);
				Vector3 lightUp = light.transform.up;
				Vector3 lightRight = light.transform.right;
				Vector3 lightEnd = lightPos + light.transform.forward * light.range;
				float spotRadius = Mathf.Tan(light.spotAngle * 0.5f * Mathf.Deg2Rad) * light.range;
				bounds.Encapsulate(lightEnd);
				Vector3 up = lightEnd + lightUp * spotRadius;
				Vector3 down = lightEnd + lightUp * -spotRadius;
				Vector3 right = lightRight * spotRadius;
				Vector3 left = lightRight * -spotRadius;
				bounds.Encapsulate(up + right);
				bounds.Encapsulate(up + left);
				bounds.Encapsulate(down + right);
				bounds.Encapsulate(down + left);
				break;
			case LightType.Point:
				float lightDiameter = light.range * 2f;
				bounds = new Bounds(light.transform.position, new Vector3(lightDiameter, lightDiameter, lightDiameter));
				break;
			case LightType.Area:
			case LightType.Directional:
			default:
				bounds = new Bounds(light.transform.position, new Vector3(kBOUNDS_CHEAT, kBOUNDS_CHEAT, kBOUNDS_CHEAT));
				break;
			}
		}
		else
		{
			bounds = new Bounds(light.transform.position, new Vector3(kBOUNDS_CHEAT, kBOUNDS_CHEAT, kBOUNDS_CHEAT));
		}
	
		return bounds;
	}

	/// Computes the bounds of the input Terrain.
	/// <param name="terrain">The terrain whose bounds need computing.</param> 
	/// <returns>The world space Bounds of the terrain.</returns>
	public static Bounds ComputeBounds(Terrain terrain)
	{
		if(terrain)
		{
			Vector3 terrainSize = terrain.terrainData != null ? terrain.terrainData.size : Vector3.zero;
			Vector3 terrainPosition = terrain.transform.position;
			Vector3 terrainCenter = new Vector3(terrainPosition.x + terrainSize.x * 0.5f, terrainPosition.y + terrainSize.y * 0.5f, terrainPosition.z + terrainSize.z * 0.5f); 
			return new Bounds(terrainCenter, terrainSize);
		}
		return new Bounds();
	}

	/// Determines if an AABB intersects a frustum.
	/// <param name="bounds">The AABB to check for inclusion.</param>
	/// <param name="frustum">An array of planes that define the frustum.</param>
	/// <param name="inMask">A bitmask of which planes to test for intersection, as computed by a parent AABB.</param>
	/// <param name="outMask">The bitmask of planes that intersect this AABB.</param>
	/// <returns>Returns true if it is fully or partially contained, false otherwise.</param>
	public static bool FrustumIntersectsBounds(Bounds bounds, List<Plane> frustum, int inMask, out int outMask)
	{
		Vector3 boundsCenter = bounds.center;
		Vector3 boundsExtents = bounds.extents;
		outMask = 0;
		for(int planeIndex = 0, k = 1; k <= inMask; ++planeIndex, k += k) if((k & inMask) != 0)
		{
			Plane plane = frustum[planeIndex];
			float m = boundsCenter.x * plane.normal.x + 
					  boundsCenter.y * plane.normal.y + 
					  boundsCenter.z * plane.normal.z + 
					  plane.distance;
			float n = boundsExtents.x * Mathf.Abs(plane.normal.x) + 
				      boundsExtents.y * Mathf.Abs(plane.normal.y) + 
					  boundsExtents.z * Mathf.Abs(plane.normal.z);
			if(m + n < 0f)
			{
				return false;
			}
			else
			{
				outMask |= k;
			}
		}
		return true;
	}

	/// Determines if an AABB is fulling contained within a frustum.
	/// <param name="bounds">The AABB to check for inclusion.</param>
	/// <param name="frustum">An array of planes that define the frustum.</param>
	/// <returns>Returns true if it is fully contained, false otherwise.</param>
	public static bool FrustumContainsBounds(Bounds bounds, List<Plane> frustum)
	{
		Vector3 boundsCenter = bounds.center;
		Vector3 boundsExtents = bounds.extents;
		
		int numPlanes = frustum.Count;
		for(int planeIndex = 0; planeIndex < numPlanes; ++planeIndex)
		{
			Plane plane = frustum[planeIndex];
			float m = boundsCenter.x * plane.normal.x + 
					  boundsCenter.y * plane.normal.y + 
					  boundsCenter.z * plane.normal.z + 
					  plane.distance;
			float n = boundsExtents.x * Mathf.Abs(plane.normal.x) + 
					  boundsExtents.y * Mathf.Abs(plane.normal.y) + 
					  boundsExtents.z * Mathf.Abs(plane.normal.z);
			if(m + n < 0f || m - n < 0f)
			{
				return false;
			}
		}
		return true;
	}

	/// Tests to see if one AABB fully contains another AABB.
	/// <param name="container">The AABB that does the containing.</param>
	/// <param name="contained">The AABB to test for containment.</param>
	/// <returns>Returns true if the AABB is fully contained.</returns>
	public static bool BoundsContainsBounds(Bounds container, Bounds contained)
	{
		return container.Contains(contained.min) && container.Contains(contained.max);
	}

	/// Tests to see if one an AABB and a Sphere intersect.
	/// <param name="bounds">The AABB for the test.</param>
	/// <param name="sphereCenter">The center of the sphere.</param>
	/// <param name="sphereCenter">The center of the sphere.</param>
	/// <param name="sphereRadius">The radius of the sphere.</param>
	/// <returns>Returns true if the AABB and Sphere intersect.</returns>
	public static bool BoundsIntersectsSphere(Bounds bounds, Vector3 sphereCenter, float sphereRadius)
	{
		Vector3 closestPointInBounds = Vector3.Min(Vector3.Max(sphereCenter, bounds.min), bounds.max);
		float distanceSquared = Vector3.SqrMagnitude(closestPointInBounds - sphereCenter);
		
		// The AABB and the sphere overlap if the closest point within the AABB is within the sphere's radius.
		return distanceSquared <= (sphereRadius * sphereRadius);
	}

	/// Extrudes an AABB along a ray.
	/// <param name="bounds">The original AABB.</param>
	/// <param name="projection">The direction and distance by which to project the bounds.</param> 
	/// <returns>The extruded AABB.</returns>
	public static Bounds ProjectBounds(Bounds bounds, Vector3 projection)
	{
		Vector3 projMin = bounds.min + projection;
		Vector3 projMax = bounds.max + projection;
		bounds.Encapsulate(projMin);
		bounds.Encapsulate(projMax);
		return bounds;
	}

	/// Determines if a point is in front of or behind a plane.
	/// <param name="position">The position of the point.</param>
	/// <param name="center">A point on the plane.</param>
	/// <param name="normal">The normal of the plane.</param>
	public static bool IsPointInFrontOfPlane(Vector3 position, Vector3 center, Vector3 normal)
	{
		Vector3 vecToPortal = (position - center).normalized;
		return Vector3.Dot(normal, vecToPortal) > 0f;
	}

	/// Determines if is polygon convex. Verts must be sorted in CW or CCW order.
	/// <param name="verts">The sorted array of verts in the polygon.</param>
	/// <returns>True if the polygon is convex. False otherwise.</returns>
	public static bool IsPolygonConvex(Vector3[] verts)
	{
		int vertCount = verts.Length;
		if(vertCount < 3)
		{
			return false;
		}

		float totalConvexAngle = (vertCount - 2) * Mathf.PI;
		for(int vertIndex = 0; vertIndex < vertCount; ++vertIndex)
		{
			Vector3 vert0 = verts[vertIndex];
			Vector3 vert1 = verts[(vertIndex+1) % vertCount];
			Vector3 vert2 = verts[(vertIndex+2) % vertCount];

			Vector3 edge0 = vert0 - vert1;
			edge0.Normalize();
			Vector3 edge1 = vert2 - vert1;
			edge1.Normalize();

			totalConvexAngle -= Mathf.Acos(Vector3.Dot(edge0, edge1));
		}
		return Mathf.Abs(totalConvexAngle) < SECTR_Geometry.kVERTEX_EPSILON;
	}

	/// Determines the relative order of two points on a plane.
	/// <param name="a">The first position to compare.</param> 
	/// <param name="b">The second position to compare.</param>
	/// <param name="centroid">The centroid of the reference polygon.</param>
	/// <param name="normal">The normal about which to measure the "rotation".</param>
	/// <returns>1 if they are CW, -1 for CCW, and 0 if they are identical.</returns>
	public static int CompareVectorsCW(Vector3 a, Vector3 b, Vector3 centroid, Vector3 normal)
	{
		Vector3 crossProduct = Vector3.Cross(a - centroid, b - centroid);
		float crossMag = crossProduct.magnitude;
		if(crossMag > SECTR_Geometry.kVERTEX_EPSILON)
		{
			crossProduct /= crossMag;
			return Vector3.Dot(normal, crossProduct) > 0F ? 1 : -1;
		}
		return 0;
	}
}
