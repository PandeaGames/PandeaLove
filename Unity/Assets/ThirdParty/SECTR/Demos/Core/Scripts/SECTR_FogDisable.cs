// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System.Collections;

/// \ingroup Demo
/// Disables fog in the associated camera (for use in the PiP camera).
[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class SECTR_FogDisable : MonoBehaviour
{
	private bool previousFogState;

	void OnPreRender()
	{
		previousFogState = RenderSettings.fog;
		RenderSettings.fog = false;
	}
	
	void OnPostRender()
	{
		RenderSettings.fog = previousFogState;            
	}
}
