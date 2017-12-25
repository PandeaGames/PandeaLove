// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System.Collections;

// Utiltiy class for serializing ulong, which Unity can't do by default.
[System.Serializable]
public class SECTR_ULong
{
	[SerializeField] private int first = 0;
	[SerializeField] private int second = 0;
			
	// The value of this unsigned long	
	public ulong value		
	{		
		get		
		{
			ulong n = (ulong)second;
			n = n << 32;
			return n | (ulong)first;
		}		
		set
		{
			first = (int)(value & uint.MaxValue);	
			second = (int)(value >> 32);
		}
	}
	

	public SECTR_ULong(ulong newValue)
	{
		value = newValue;	
	}
		
	public SECTR_ULong()
	{
		value = 0UL;
	}
	
	public override string ToString()
	{
		return string.Format ("[ULong: value={0}, firstHalf={1}, secondHalf={2}]", value, first, second);	
	}
				
	// Comparison	
	public static bool operator>(SECTR_ULong a, ulong b) { return a.value > b; }	
	public static bool operator>(ulong a, SECTR_ULong b) { return a > b.value; }	
	public static bool operator<(SECTR_ULong a, ulong b) { return a.value < b; }	
	public static bool operator<(ulong a, SECTR_ULong b) { return a < b.value; }	
}