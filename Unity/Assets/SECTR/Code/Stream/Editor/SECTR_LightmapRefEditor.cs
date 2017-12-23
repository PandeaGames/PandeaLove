// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SECTR_LightmapRef))]
public class SECTR_LightmapRefEditor : SECTR_Editor
{
	public override void OnInspectorGUI()
	{
		SECTR_LightmapRef myRef = (SECTR_LightmapRef)target;

		base.OnInspectorGUI();
		bool wasEnabled = GUI.enabled;
		GUI.enabled = false;
		foreach(SECTR_LightmapRef.RefData refData in myRef.LightmapRefs)
		{
			EditorGUILayout.IntField("Index", refData.index);
			ObjectField<Texture2D>("Near Lightmap", "", refData.NearLightmap, false);
			ObjectField<Texture2D>("Far Lightmap", "", refData.FarLightmap, false);
		}
		GUI.enabled = wasEnabled;
	}
}
