// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// \ingroup Demo
/// A component that will wander the scene by pathing through the
/// Sector/Portal graph.
/// 
/// Wanderer simply picks a goal sector, plots a path to it, and
/// then follows that path, going through the center of each
/// Portal and Sector along the way. Useful for testing and
/// demoing objects moving through the world.
[AddComponentMenu("SECTR/Demos/SECTR Wanderer")]
public class SECTR_Wanderer : MonoBehaviour
{
	#region Private Details
	private List<SECTR_Graph.Node> path = new List<SECTR_Graph.Node>(16);
	private List<Vector3> waypoints = new List<Vector3>(16);
	private int currentWaypointIndex = 0;
	#endregion

	#region Public Interface
	[SECTR_ToolTip("The speed at which the wanderer moves throughout the world.")]
	public float MovementSpeed = 1;
	#endregion

	#region Unity Interface
	void Update()
	{
		if(waypoints.Count == 0 && SECTR_Sector.All.Count > 0 && MovementSpeed > 0f)
		{
			SECTR_Sector goal = SECTR_Sector.All[Random.Range(0, SECTR_Sector.All.Count)];
			SECTR_Graph.FindShortestPath(ref path, transform.position, goal.transform.position, SECTR_Portal.PortalFlags.Locked);
			Vector3 height = Vector3.zero;
			Collider myCollider = GetComponent<Collider>();
			if(myCollider)
			{
				height.y += myCollider.bounds.extents.y;
			}
			waypoints.Clear();
			int numNodes = path.Count;
			for(int nodeIndex = 0; nodeIndex < numNodes; ++nodeIndex)
			{
				SECTR_Graph.Node node = path[nodeIndex];
				waypoints.Add(node.Sector.transform.position + height);
				if(node.Portal)
				{
					waypoints.Add(node.Portal.transform.position);
				}
			}
			waypoints.Add(goal.transform.position + height);
			currentWaypointIndex = 0;
		}
		
		if(waypoints.Count > 0 && MovementSpeed > 0)
		{
			Vector3 nextWaypoint = waypoints[currentWaypointIndex];
			Vector3 vecToGoal = nextWaypoint - transform.position;
			float sqrGoalDistance = vecToGoal.sqrMagnitude;
			if(sqrGoalDistance > SECTR_Geometry.kVERTEX_EPSILON)
			{
				float distanceToGoal = Mathf.Sqrt(sqrGoalDistance);
				vecToGoal /= distanceToGoal;
				vecToGoal *= Mathf.Min(MovementSpeed * Time.deltaTime, distanceToGoal);
				transform.position += vecToGoal;
			}
			else
			{
				++currentWaypointIndex;
				if(currentWaypointIndex >= waypoints.Count)
				{
					waypoints.Clear();
				}
			}
		}		
	}

#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		if( MovementSpeed > 0 && waypoints.Count > 0 )
		{
			Gizmos.DrawLine(transform.position, waypoints[currentWaypointIndex]);
			for(int i = currentWaypointIndex; i < waypoints.Count - 1; ++i)
			{
				Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
			}
		}
	}
#endif
	#endregion
}
