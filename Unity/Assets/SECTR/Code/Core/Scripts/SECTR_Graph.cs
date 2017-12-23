// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System.Collections.Generic;

/// \ingroup Core
/// A set of static utility functions used to traverse the Sector/Portal graph. 
/// 
/// The set of Sectors and Portals can be thought of as a graph, where the Sectors
/// are the nodes and the Portals are the edges. SectorGraph implements useful
/// funtions for traversing or otherwise searching the graph.
public static class SECTR_Graph
{
	#region Private details
	private static List<SECTR_Sector> initialSectors = new List<SECTR_Sector>(4);
	private static List<SECTR_Sector> goalSectors = new List<SECTR_Sector>(4);
	private static SECTR_PriorityQueue<Node> openSet = new SECTR_PriorityQueue<Node>(64);
	private static Dictionary<SECTR_Portal, Node> closedSet = new Dictionary<SECTR_Portal, Node>(64);
	#endregion

	#region Public Interface
	/// Represents a Node in the Sector/Portal graph.
	/// Contains useful data for implementing traversals.
	public class Node : System.IComparable<Node>
	{	
		public SECTR_Portal Portal = null;
		public SECTR_Sector Sector = null;
		public float CostPlusEstimate = 0;
		public float Cost = 0;
		public int Depth = 0;
		public bool ForwardTraversal = false;
		public Node Parent = null;

		/// Comparison function for two Nodes. Used in A*.
		/// <param name="other">The Node against to compare ourselves.</param>
		/// <returns>The relative ordering of this and another Node.</returns>
		public int CompareTo(Node other)
		{
			if(CostPlusEstimate > other.CostPlusEstimate)
			{
				return 1;
			}
			else if(CostPlusEstimate < other.CostPlusEstimate)
			{
				return -1;
			}
			else
			{
				return 0;
			}
		}

		/// Utility function for reconstructing a path from a set of nodes.
		/// <param name="path">The List to populate with the path.</param>
		/// <param name="currentNode">The Node from which to start the path generation.</param>
		public static void ReconstructPath(List<Node> path, Node currentNode)
		{
			if(currentNode != null)
			{
				path.Insert(0, currentNode);
				ReconstructPath(path, currentNode.Parent);
			}
		}
	};

	/// Generates a List of nodes that is a depth-first traversal of 
	/// walk of sector graph from the specified root.
	/// <param name="nodes">List into which walk results will be written.</param> 
	/// <param name="root">The Sector at which to start the traversal.</param>
	/// <param name="stopFlags">Flag set to test at each SECTR_Portal. A failed test will stop the traversal.</param> 
	/// <param name="maxDepth">The depth into the graph at which to end the traversal. -1 means no limit.</param>
	/// <returns>A List of Nodes in depth-first traveral order.</returns>
	public static void DepthWalk(ref List<Node> nodes, SECTR_Sector root, SECTR_Portal.PortalFlags stopFlags, int maxDepth)
	{
		nodes.Clear();
		if(root == null)
		{
			return;
		}
		else if(maxDepth == 0)
		{
			Node node = new Node();
			node.Sector = root;
			nodes.Add(node);
			return;
		}

		// We only want to visit each Sector once, so we'll mark them
		// in order to avoid cycles.
		int numSectors = SECTR_Sector.All.Count;
		for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
		{
			SECTR_Sector.All[sectorIndex].Visited = false;
		}
		
		// Use a stack for the search, to keep implementation similar
		// to the breadth first search above.
		Stack<Node> nodeStack = new Stack<Node>(numSectors);
		Node rootNode = new Node();
		rootNode.Sector = root;
		rootNode.Depth = 1;
		nodeStack.Push(rootNode);
		root.Visited = true;
		int exploredNodes = 0;
		
		while(nodeStack.Count > 0)
		{
			Node nextNode = nodeStack.Pop();
			nodes.Add(nextNode);
			++exploredNodes;

			if(maxDepth < 0 || nextNode.Depth <= maxDepth)
			{
				int numPortals = nextNode.Sector.Portals.Count;
				for(int portalIndex = 0; portalIndex < numPortals; ++portalIndex)
				{
					SECTR_Portal portal = nextNode.Sector.Portals[portalIndex];
					if(portal && (portal.Flags & stopFlags) == 0)
					{
						SECTR_Sector neighborSector = portal.FrontSector == nextNode.Sector ? portal.BackSector : portal.FrontSector;
						if(neighborSector && !neighborSector.Visited)
						{
							Node neighborNode = new Node();
							neighborNode.Parent = nextNode;
							neighborNode.Sector = neighborSector;
							neighborNode.Portal = portal;
							neighborNode.Depth = nextNode.Depth + 1;
							nodeStack.Push(neighborNode);
							neighborSector.Visited = true;
						}
					}
				}
			}
		}
	}
	
	/// Generates a List of nodes that is a braedth-first traversal of 
	/// walk of sector graph from the specified root.
	/// <param name="nodes">List into which walk results will be written.</param> 
	/// <param name="root">The Sector at which to start the traversal.</param>
	/// <param name="stopFlags">Flag set to test at each SECTR_Portal. A failed test will stop the traversal.</param> 
	/// <param name="maxDepth">The depth into the graph at which to end the traversal. -1 means no limit.</param>
	/// <returns>A List of Nodes in breadth-first traveral order.</returns>
	public static void BreadthWalk(ref List<Node> nodes, SECTR_Sector root, SECTR_Portal.PortalFlags stopFlags, int maxDepth)
	{
		nodes.Clear();
		if(root == null)
		{
			return;
		}
		else if(maxDepth == 0)
		{
			Node node = new Node();
			node.Sector = root;
			nodes.Add(node);
			return;
		}

		// We only want to visit each Sector once, so we'll mark them
		// in order to avoid cycles.
		int numSectors = SECTR_Sector.All.Count;
		for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
		{
			SECTR_Sector.All[sectorIndex].Visited = false;
		}
		
		// Use a stack for the search, to keep implementation similar
		// to the breadth first search above.
		Queue<Node> nodeQueue = new Queue<Node>(numSectors);
		Node rootNode = new Node();
		rootNode.Sector = root;
		rootNode.Depth = 0;
		nodeQueue.Enqueue(rootNode);
		root.Visited = true;
		int exploredNodes = 0;
		
		while(nodeQueue.Count > 0)
		{
			Node nextNode = nodeQueue.Dequeue();
			nodes.Add(nextNode);
			++exploredNodes;

			if(maxDepth < 0 || nextNode.Depth < maxDepth)
			{
				int numPortals = nextNode.Sector.Portals.Count;
				for(int portalIndex = 0; portalIndex < numPortals; ++portalIndex)
				{
					SECTR_Portal portal = nextNode.Sector.Portals[portalIndex];
					if(portal && (portal.Flags & stopFlags) == 0)
					{
						SECTR_Sector neighborSector = portal.FrontSector == nextNode.Sector ? portal.BackSector : portal.FrontSector;
						if(neighborSector && !neighborSector.Visited)
						{
							Node neighborNode = new Node();
							neighborNode.Parent = nextNode;
							neighborNode.Sector = neighborSector;
							neighborNode.Portal = portal;
							neighborNode.Depth = nextNode.Depth + 1;
							nodeQueue.Enqueue(neighborNode);
							nextNode.Sector.Visited = true;
						}
					}
				}
			}
		}
	}
	
	/// Finds the shortest path through the portal graph between two points.
	/// The start and end points must currently be within Sector in the graph.
	/// <param name="path">List into which search results will be written.</param> 
	/// <param name="start">The world space position at which to start the search.</param>
	/// <param name="goal">The world space goal of the search.</param>
	/// <param name="stopFlags">Flag set to test at each SECTR_Portal. A failed test will stop the traversal.</param> 
	/// <returns>A list of nodes from the Start to the Goal. Empty if there is no path.</returns>
	public static void FindShortestPath(ref List<Node> path, Vector3 start, Vector3 goal, SECTR_Portal.PortalFlags stopFlags)
	{
		// This is an implementation of a basic A* search.
		// Implementation is optimized through use of a priority queue open list
		// and a dictionary for the closed list.
		path.Clear();
		openSet.Clear();
		closedSet.Clear();

		// Get the list of starting portals, all of which will be pushed on to the open set.
		// There may be multiple candidate Sectors becuase the bounding boxes may overlap.
		SECTR_Sector.GetContaining(ref initialSectors, start);
		SECTR_Sector.GetContaining(ref goalSectors, goal);

		int numInitialSectors = initialSectors.Count;
		for(int initialSectorIndex = 0; initialSectorIndex < numInitialSectors; ++initialSectorIndex)
		{
			SECTR_Sector sector = initialSectors[initialSectorIndex];
			if(goalSectors.Contains(sector))
			{
				Node newElement = new Node();
				newElement.Sector = sector;
				path.Add(newElement);
				return;
			}
			
			int numPortals = sector.Portals.Count;
			for(int portalIndex = 0; portalIndex < numPortals; ++portalIndex)
			{
				SECTR_Portal portal = sector.Portals[portalIndex];
				if((portal.Flags & stopFlags) == 0)
				{
					Node newElement = new Node();
					newElement.Portal = portal;
					newElement.Sector = sector;
					newElement.ForwardTraversal = sector == portal.FrontSector;
					newElement.Cost = Vector3.Magnitude(start - portal.transform.position);
					float estimate = Vector3.Magnitude(goal - portal.transform.position);
					newElement.CostPlusEstimate = newElement.Cost + estimate;
					openSet.Enqueue(newElement);
				}
			}
		}
		
		// Time to do some A*...
		while(openSet.Count > 0)
		{
			Node current = openSet.Dequeue();
			SECTR_Sector sector = current.ForwardTraversal ? current.Portal.BackSector : current.Portal.FrontSector;
			if(!sector)
			{
				continue;
			}

			// If the current element Sector contains the goal point, we're done.
			// NOTE: I *think* it's correct to end here but we should prove that
			// this is correct even strange connections of concave Sector.
			if(goalSectors.Contains(sector))
			{
				Node.ReconstructPath(path, current);
				break;
			}

			int numPortals = sector.Portals.Count;
			for(int portalIndex = 0; portalIndex < numPortals; ++portalIndex)
			{
				SECTR_Portal portal = sector.Portals[portalIndex];
				if(portal != current.Portal && (portal.Flags & stopFlags) == 0)
				{
					// Create a new SearchElement for this neighbor.
					Node neighborElement = new Node();
					neighborElement.Parent = current;
					neighborElement.Portal = portal;
					neighborElement.Sector = sector;
					neighborElement.ForwardTraversal = sector == portal.FrontSector;
					neighborElement.Cost = current.Cost + Vector3.Magnitude(neighborElement.Portal.transform.position - current.Portal.transform.position);
					float estimate = Vector3.Magnitude(goal - neighborElement.Portal.transform.position);
					neighborElement.CostPlusEstimate = neighborElement.Cost + estimate;
					
					// If the closed list already contains this portal,
					// and that version is closer than us, we'll skip this node.
					Node closedElement = null;
					closedSet.TryGetValue(neighborElement.Portal, out closedElement);
					if(closedElement != null && closedElement.CostPlusEstimate < neighborElement.CostPlusEstimate)
					{
						continue;
					}
					
					// Check to see if the neighbor is already on the open list.
					Node openElement = null;
					for(int i = 0; i < openSet.Count; ++i)
					{
						if(openSet[i].Portal == neighborElement.Portal)
						{
							openElement = openSet[i];
							break;
						}
					}
					// Skip this neighbor if the open neighbor is better than us.
					if(openElement != null && openElement.CostPlusEstimate < neighborElement.CostPlusEstimate)
					{
						continue;
					}
					
					// Add this neighbor to the open list.
					openSet.Enqueue(neighborElement);
				}
			}
			
			// Once all neighbors are considered, put this node onto the close list.
			if(!closedSet.ContainsKey(current.Portal))
			{
				closedSet.Add(current.Portal, current);
			}
		}
	}

	/// Gets the graph as dot formatted string (for visualization in GraphViz and the like).
	/// <param name="graphName">The name to embed in the graph file.</param>
	/// <returns>The graph as dot formatted string.</returns>
	public static string GetGraphAsDot(string graphName)
	{
		string graphFile = "graph " + graphName;
		graphFile += " {\n";
		graphFile += "\tlayout=neato\n";
		foreach(SECTR_Portal portal in SECTR_Portal.All)
		{
			graphFile += "\t";
			graphFile += portal.GetInstanceID();
			graphFile += " [";
			graphFile += "label=" + portal.name;
			graphFile += ",shape=hexagon";
			graphFile += "];\n";
		}
		
		foreach(SECTR_Sector sector in SECTR_Sector.All)
		{
			graphFile += "\t";
			graphFile += sector.GetInstanceID();
			graphFile += " [";
			graphFile += "label=" + sector.name;
			graphFile += ",shape=box";
			graphFile += "];\n";
		}
		
		foreach(SECTR_Portal portal in SECTR_Portal.All)
		{
			if(portal.FrontSector)
			{
				graphFile += "\t";
				graphFile += portal.GetInstanceID() + " -- " + portal.FrontSector.GetInstanceID();
				graphFile += ";\n";
			}
			if(portal.BackSector)
			{
				graphFile += "\t";
				graphFile += portal.GetInstanceID() + " -- " + portal.BackSector.GetInstanceID();
				graphFile += ";\n";
			}
		}
		graphFile += "\n}";
		return graphFile;
	}
	#endregion
}