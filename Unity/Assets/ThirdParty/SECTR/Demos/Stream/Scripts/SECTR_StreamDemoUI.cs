// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System.Collections;

[AddComponentMenu("SECTR/Demos/SECTR Stream Demo UI")]
public class SECTR_StreamDemoUI : SECTR_DemoUI 
{
	#region Public Interface
	[Multiline] public string NoExportMessage;
	#endregion

	#region Unity Interface
	protected override void OnEnable()
	{
		base.OnEnable();
		SECTR_StartLoader startLoader = GetComponent<SECTR_StartLoader>();
		if(startLoader)
		{
			startLoader.Paused = true;
		}
	}

	protected override void OnGUI()
	{
		bool exported = false;
		int numSectors = SECTR_Sector.All.Count;
		for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
		{
			if(SECTR_Sector.All[sectorIndex].Frozen)
			{
				exported = true;
				break;
			}
		}

		if(!exported && !string.IsNullOrEmpty(NoExportMessage))
		{
			DemoMessage = NoExportMessage;
		}

		base.OnGUI();

		SECTR_StartLoader startLoader = GetComponent<SECTR_StartLoader>();
		if(passedIntro && startLoader && startLoader.Paused)
		{
			startLoader.Paused = false;
		}
	}
	#endregion
}
