// Copyright (c) 2014 Make Code Now! LLC

using System;
using System.Collections.Generic;

/// \ingroup Core
/// Implements a priority queue in terms of a binary heap.
/// 
/// Based on http://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx.
public class SECTR_PriorityQueue<T> where T : IComparable<T>
{
	#region Private Details
	private List<T> data;
	#endregion

	#region Public Interface
	public SECTR_PriorityQueue()
	{
		data = new List<T>(64);
	}

	public SECTR_PriorityQueue(int capacity)
	{
		data = new List<T>(capacity);
	}

	/// Returns the number of items in the queue.
	public int Count
	{
		get { return data.Count; }
		set {}
	}

	/// Retrieves or modifies the item at the specified index.
	public T this[int index]
    {
        get { return index < data.Count ? data[index] : default(T); }
        set 
        { 
			if( index < data.Count )
			{
            	data[index] = value;
				_Update(index);
			}
        }
    }

	/// Enqueue the specified item.
	/// <param name='item'>The item to enqueue.</param>
	public void Enqueue(T item)
	{
		data.Add(item);
		int ci = data.Count - 1; // child index; start at end
		while(ci > 0)
		{
			int pi = (ci - 1) / 2; // parent index
			if(data[ci].CompareTo(data[pi]) < 0)
			{
				_SwapElements(ci, pi);
				ci = pi;
			}
			else
			{
				break; // child item is larger than (or equal) parent so we're done
			}
		}
	}

	/// Dequeue the lowest priority item from the queue.
	public T Dequeue()
	{
		// assumes pq is not empty; up to calling code
		int li = data.Count - 1; // last index (before removal)
		T frontItem = data[0];   // fetch the front
		data[0] = data[li];
		data.RemoveAt(li);
	
		--li; // last index (after removal)
		int pi = 0; // parent index. start at front of pq
		while(true)
		{
			int ci = pi * 2 + 1; // left child index of parent
			if(ci > li)
			{
				break;  // no children so done
			}
			int rc = ci + 1;     // right child
			if(rc <= li && data[rc].CompareTo(data[ci]) < 0) 
			{
				ci = rc; // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
			}
			if(data[pi].CompareTo(data[ci]) > 0) 
			{
				_SwapElements(pi, ci); // swap parent and child
				pi = ci;
			}
			else
			{
				break; // parent is smaller than (or equal to) smallest child so done
			}
		}
		return frontItem;
	}

	/// Examine the lowest priority item but don't remove it.
	public T Peek()
	{
		return data.Count > 0 ? data[0] : default(T);
	}

	/// Returns a nice string that represents the current state of the queue.
	public override string ToString()
	{
		string s = "";
		for(int i = 0; i < data.Count; ++i)
		{
			s += data[i].ToString() + " ";
		}
		s += "count = " + data.Count;
		return s;
	}

	/// Indicates if the queue is consistent/properly sorted.
	public bool IsConsistent()
	{
		// is the heap property true for all data?
		if(data.Count > 0)
		{
			int li = data.Count - 1; // last index
			for(int pi = 0; pi < data.Count; ++pi) // each parent index
			{
				int lci = 2 * pi + 1; // left child index
				int rci = 2 * pi + 2; // right child index
				
				if((lci <= li && data[pi].CompareTo(data[lci]) > 0) ||  // if lc exists and it's greater than parent then bad.
					(rci <= li && data[pi].CompareTo(data[rci]) > 0))   // check the right child too.
				{
					return false; 
				}
			}
		}
		return true; // passed all checks
	} 

	public void Clear()
	{
		data.Clear();
	}
	#endregion
	
	#region Private Methods
	private void _SwapElements(int i, int j)
	{
		T h = data[i];
		data[i] = data[j];
		data[j] = h;
	}

	/// Notify the PQ that the object at position i has changed
	/// and the PQ needs to restore order.
	private void _Update(int i)
	{
		int ci = i; // child index; start at current item
		while (ci > 0)
		{
			int pi = (ci - 1) / 2; // parent index
			if (data[ci].CompareTo(data[pi]) < 0)
			{
				_SwapElements(ci, pi);
				ci = pi;
			}
			else
			{
				break; // child item is larger than (or equal) parent so we're done
			}
		}
		
		if(ci >= i)
		{
			while(true)
			{
				int pn = ci;
				int p1 = 2 * ci + 1;
				int p2 = 2 * ci + 2;
				
				if(data.Count > p1 && data[ci].CompareTo(data[p1]) > 0)
				{
					_SwapElements(p1, pn);
					ci = p1;
				}
				else if(data.Count > p2 && data[ci].CompareTo(data[p2]) > 0)
				{
					_SwapElements(p2, pn);
					ci = p2;
				}
				else
				{
					break;
				}
			}
		}
	}
	#endregion
}