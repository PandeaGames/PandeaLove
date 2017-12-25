using UnityEngine;
using UnityEditor;
using System.Collections;

public class SECTR_Window : EditorWindow 
{
	#region Private Details
	protected int headerHeight = 25;
	#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
	protected int lineHeight = 16;
	#else
	protected int lineHeight = (int)EditorGUIUtility.singleLineHeight;
	#endif
	protected GUIStyle headerStyle = null;
	protected GUIStyle elementStyle = null;
	protected GUIStyle selectionBoxStyle = null;
	protected GUIStyle iconButtonStyle = null;
	protected GUIStyle searchBoxStyle = null;
	protected GUIStyle searchCancelStyle = null;
	protected Texture2D selectionBG = null;
	#endregion

	#region Public Interface
	public static Color UnselectedItemColor
	{
		get { return EditorGUIUtility.isProSkin ? Color.gray : Color.black; }
	}
	#endregion


	protected virtual void OnGUI()
	{
		if(headerStyle == null)
		{
			headerStyle = new GUIStyle(EditorStyles.toolbar);
			headerStyle.fontStyle = FontStyle.Bold;
			headerStyle.alignment = TextAnchor.MiddleLeft;
		}
		
		if(elementStyle == null)
		{
			elementStyle = new GUIStyle(GUI.skin.label);
			elementStyle.margin = new RectOffset(0, 0, 5, 5);
			elementStyle.border = new RectOffset(0, 0, 0, 0);
			elementStyle.normal.textColor = UnselectedItemColor;
		}

		if(selectionBG == null)
		{
			selectionBG = new Texture2D(1, 1);
			selectionBG.SetPixel(0,0, new Color(62f/255f, 125f/255f, 231f/255f));
			selectionBG.Apply();
		}

		if(selectionBoxStyle == null)
		{
			selectionBoxStyle = new GUIStyle(GUI.skin.box);
			selectionBoxStyle.normal.background = selectionBG;
		}

		if(iconButtonStyle == null)
		{
			iconButtonStyle = new GUIStyle(GUI.skin.button);
			iconButtonStyle.padding = new RectOffset(2,2,2,2);
		}

		if(searchBoxStyle == null)
		{
			searchBoxStyle = GUI.skin.FindStyle("ToolbarSeachTextField");
		}

		if(searchCancelStyle == null)
		{
			searchCancelStyle = GUI.skin.FindStyle("ToolbarSeachCancelButton");
		}
	}

	protected Rect DrawHeader(string title, ref string searchString, float searchWidth, bool center)
	{
		Rect headerRect = EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		headerStyle.alignment = center ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
		if(center)
		{
			GUILayout.FlexibleSpace();
		}
		GUILayout.Label(title, headerStyle);
		if(searchString != null)
		{
			GUI.SetNextControlName(title + "_Header");
			searchString = EditorGUILayout.TextField(searchString, searchBoxStyle, GUILayout.Width(searchWidth));
			if(GUILayout.Button("", searchCancelStyle))
			{
				// Remove focus if cleared
				searchString = "";
				GUI.FocusControl(null);
			}
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		return headerRect;
	}
}
