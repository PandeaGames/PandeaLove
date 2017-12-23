// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SECTR_ChunkRef))]
[CanEditMultipleObjects]
public class SECTR_ChunkRefEditor : SECTR_Editor
{
	public override void OnInspectorGUI()
	{
		GUI.enabled = false;
		base.OnInspectorGUI();
		GUI.enabled = true;
	}
}
