// Copyright (c) 2014 Make Code Now! LLC

#if UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
#define UNITY_4
#endif

using UnityEngine;
using System;

public static class SECTR_Modules
{
	public static bool AUDIO = false;
	public static bool VIS = false;
	public static bool STREAM = false;
	public static bool DEV = false;
	public static string VERSION = "1.3.4";

	static SECTR_Modules()
	{
		AUDIO = Type.GetType("SECTR_AudioSystem") != null;
		VIS = Type.GetType("SECTR_CullingCamera") != null;
		STREAM = Type.GetType("SECTR_Chunk") != null;
		DEV = Type.GetType("SECTR_Tests") != null;
	}

	public static bool HasPro()
	{
		#if UNITY_4_0
		return false; // 4.0 and below users, set this to true or false based on what you have.
		#elif UNITY_4
		return Application.HasProLicense();
		#else
		// Unity 5 is Pro for all the ways that SECTR cares.
		return true;
		#endif
	}

	public static bool HasComplete()
	{
		return AUDIO && VIS && STREAM;
	}
}