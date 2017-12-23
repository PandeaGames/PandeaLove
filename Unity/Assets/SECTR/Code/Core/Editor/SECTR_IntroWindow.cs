using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

public class IntroData
{
	public static string sectrBaseURL = "http://sectr.co/";
	public static string siteFileBaseURL = "http://www.sectr.co/uploads/2/5/7/9/25793991/";
	public static string uasBaseURL = "com.unity3d.kharma:content/";
	public static string forumBaseURL = "http://forum.unity3d.com/threads/";
	public static string infoURL = IntroData.siteFileBaseURL + "sectr_info.xml";

	public string version = SECTR_Modules.VERSION;
	public string news = "Welcome to SECTR";

	public string coreDocURL = sectrBaseURL + "docs.html";
	public string audioDocURL = sectrBaseURL + "docs3.html";
	public string streamDocURL = sectrBaseURL + "docs1.html";
	public string visDocURL = sectrBaseURL + "docs2.html";

	public string coreUAS = uasBaseURL + "15240";
	public string audioUAS = uasBaseURL + "15325";
	public string streamUAS = uasBaseURL + "15354";
	public string visUAS = uasBaseURL + "15353";
	public string completeUAS = uasBaseURL + "15356";

	public string coreForumURL = forumBaseURL + "229901/";
	public string audioForumURL = forumBaseURL + "229905/";
	public string streamForumURL = forumBaseURL + "229907/";
	public string visForumURL = forumBaseURL + "229908/";
	public string completeForumURL = forumBaseURL + "229910/";

	public string mailSupportURL = "mailto:support@makecodenow.com";
	public string webSupportURL = sectrBaseURL + "support";
}

[InitializeOnLoad]
public class SECTR_IntroWindow : SECTR_Window 
{
	#region Private Details
	static string showPrefName = "SECTR_Intro_Show";
	static string verPrefName = "SECTR_Intro_Ver";
	static WWW infoWWW = null;
	static IntroData liveData;

	bool hasComplete = false;
	bool didInit = false;


	IntroData defaultData = new IntroData();

	float supportIconSize = 30f;
	float forumIconSize = 70f;
	float saleIconSize = 90f;
	Texture2D bannerIcon;
	Texture2D coreIcon;
	Texture2D audioIcon;
	Texture2D streamIcon;
	Texture2D visIcon;
	Texture2D completeIcon;
	#endregion

	#region Public Interface
	public static void ShowWindow()
	{
		SECTR_IntroWindow window = EditorWindow.GetWindowWithRect<SECTR_IntroWindow>(_ComputeBounds(), true, "SECTR Quick Start");
		window.Show();
	}
	#endregion

	#region Unity Interface
	void OnEnable()
	{
		bannerIcon = LoadIcon("SECTR_Intro");
		coreIcon = LoadIcon("SECTR_Core");
		audioIcon = LoadIcon("SECTR_Audio");
		streamIcon = LoadIcon("SECTR_Stream");
		visIcon = LoadIcon("SECTR_Vis");
		completeIcon = LoadIcon("SECTR_Complete");
	}
		
	protected override void OnGUI()
	{
		base.OnGUI();

		if(!didInit)
		{
			position = _ComputeBounds();
			didInit = true;
		}
	
		hasComplete = SECTR_Modules.AUDIO && SECTR_Modules.STREAM && SECTR_Modules.VIS;
		Color textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
		headerStyle.normal.textColor = textColor;
		elementStyle.normal.textColor = textColor;

		Rect mainRect = EditorGUILayout.BeginVertical();

		_DrawHeader();
		EditorGUILayout.Space();
		_DrawNews();
		EditorGUILayout.Space();
		_DrawSupport();
		EditorGUILayout.Space();
		_DrawDocs();
		EditorGUILayout.Space();
		_DrawUpsell();

		EditorGUILayout.EndVertical();

		_DrawOptOut(mainRect);
	}
	#endregion

	#region Private Methods
	static SECTR_IntroWindow()
	{
		EditorApplication.update += InitSequence;
	}

	static void InitSequence()
	{
		if(infoWWW == null)
		{
			infoWWW = new WWW(IntroData.infoURL);
		}
		else if(infoWWW.isDone || !string.IsNullOrEmpty(infoWWW.error))
		{
			if(infoWWW.isDone)
			{
				XmlSerializer mySerializer = new XmlSerializer(typeof(IntroData));
				StringReader stringReader = new StringReader(infoWWW.text);
				liveData = (IntroData)mySerializer.Deserialize(stringReader);
				if(EditorPrefs.GetBool(showPrefName) || EditorPrefs.GetString(verPrefName) != liveData.version)
				{
					EditorPrefs.SetString(verPrefName, liveData.version);
					ShowWindow();
				}
			}
			EditorApplication.update -= InitSequence;
			infoWWW = null;
		}
	}

	private IntroData _CurrentData()
	{
		return liveData != null ? liveData : defaultData;
	}

	private void _DrawHeader()
	{
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUIStyle aStyle = new GUIStyle(GUI.skin.label);
		aStyle.alignment = TextAnchor.MiddleCenter;
		GUILayout.Label(bannerIcon, aStyle, GUILayout.Height(50));
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		elementStyle.alignment = TextAnchor.MiddleLeft;
		EditorGUILayout.LabelField("Installed: " + SECTR_Modules.VERSION, elementStyle, GUILayout.Width(position.width * 0.5f));
		elementStyle.alignment = TextAnchor.MiddleRight;
		EditorGUILayout.LabelField("Latest: " + _CurrentData().version, elementStyle, GUILayout.Width(position.width * 0.4f));
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		if(!string.IsNullOrEmpty(_CurrentData().version) && SECTR_Modules.VERSION != _CurrentData().version)
		{
			if(GUILayout.Button("New Version Available"))
			{
				string storeURL;
				if(hasComplete)
				{
					storeURL = _CurrentData().completeUAS;
				}
				else if(SECTR_Modules.AUDIO)
				{
					storeURL = _CurrentData().audioUAS;
				}
				else if(SECTR_Modules.STREAM)
				{
					storeURL = _CurrentData().streamUAS;
				}
				else if(SECTR_Modules.VIS)
				{
					storeURL = _CurrentData().visUAS;
				}
				else
				{
					storeURL = _CurrentData().coreUAS;
				}
				Application.OpenURL(storeURL);
			}
		}
	}

	private void _DrawNews()
	{
		string nullSearch = null;
		DrawHeader("LATEST NEWS", ref nullSearch, 0, true);
		elementStyle.alignment = TextAnchor.MiddleCenter;
		EditorGUILayout.LabelField(_CurrentData().news, elementStyle, GUILayout.Height(50));
	}

	private void _DrawSupport()
	{
		string nullSearch = null;
		DrawHeader("FORUMS & SUPPORT", ref nullSearch, 0, true);
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button(coreIcon, GUILayout.Width(forumIconSize), GUILayout.Height(forumIconSize)))
		{
			Application.OpenURL(_CurrentData().coreForumURL);
		}
		if(SECTR_Modules.AUDIO && GUILayout.Button(audioIcon, GUILayout.Width(forumIconSize), GUILayout.Height(forumIconSize)))
		{
			Application.OpenURL(_CurrentData().audioForumURL);
		}
		if(SECTR_Modules.STREAM && GUILayout.Button(streamIcon, GUILayout.Width(forumIconSize), GUILayout.Height(forumIconSize)))
		{
			Application.OpenURL(_CurrentData().streamForumURL);
		}
		if(SECTR_Modules.VIS && GUILayout.Button(visIcon, GUILayout.Width(forumIconSize), GUILayout.Height(forumIconSize)))
		{
			Application.OpenURL(_CurrentData().visForumURL);
		}
		if(hasComplete && GUILayout.Button(completeIcon, GUILayout.Width(forumIconSize), GUILayout.Height(forumIconSize)))
		{
			Application.OpenURL(_CurrentData().completeForumURL);
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Email Support", GUILayout.Height(supportIconSize)))
		{
			Application.OpenURL(_CurrentData().mailSupportURL);
		}
		if(GUILayout.Button("Web Support", GUILayout.Height(supportIconSize)))
		{
			Application.OpenURL(_CurrentData().webSupportURL);
		}
		EditorGUILayout.EndHorizontal();
	}

	private void _DrawDocs()
	{
		string nullSearch = null;
		DrawHeader("VIDEOS & DOCS", ref nullSearch, 0, true);
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button(coreIcon, GUILayout.Width(forumIconSize), GUILayout.Height(forumIconSize)))
		{
			Application.OpenURL(_CurrentData().coreDocURL);
		}
		if(SECTR_Modules.AUDIO && GUILayout.Button(audioIcon, GUILayout.Width(forumIconSize), GUILayout.Height(forumIconSize)))
		{
			Application.OpenURL(_CurrentData().audioDocURL);
		}
		if(SECTR_Modules.STREAM && GUILayout.Button(streamIcon, GUILayout.Width(forumIconSize), GUILayout.Height(forumIconSize)))
		{
			Application.OpenURL(_CurrentData().streamDocURL);
		}
		if(SECTR_Modules.VIS && GUILayout.Button(visIcon, GUILayout.Width(forumIconSize), GUILayout.Height(forumIconSize)))
		{
			Application.OpenURL(_CurrentData().visDocURL);
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
	}

	private void _DrawUpsell()
	{
		if(!hasComplete || SECTR_Modules.DEV)
		{
			string nullSearch = null;
			DrawHeader("-GET MORE SECTR-", ref nullSearch, 0, true);
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if((!SECTR_Modules.AUDIO || SECTR_Modules.DEV) &&
			   GUILayout.Button(audioIcon, GUILayout.Width(saleIconSize), GUILayout.Height(saleIconSize)))
			{
				Application.OpenURL(_CurrentData().audioUAS);
			}
			if((!SECTR_Modules.STREAM || SECTR_Modules.DEV) &&
			   GUILayout.Button(streamIcon, GUILayout.Width(saleIconSize), GUILayout.Height(saleIconSize)))
			{
				Application.OpenURL(_CurrentData().streamUAS);
			}
			if((!SECTR_Modules.VIS || SECTR_Modules.DEV) &&
			   GUILayout.Button(visIcon, GUILayout.Width(saleIconSize), GUILayout.Height(saleIconSize)))
			{
				Application.OpenURL(_CurrentData().visUAS);
			}
			if((!hasComplete || SECTR_Modules.DEV) &&
			   GUILayout.Button(completeIcon, GUILayout.Width(saleIconSize), GUILayout.Height(saleIconSize)))
			{
				Application.OpenURL(_CurrentData().completeUAS);
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}
	}

	private void _DrawOptOut(Rect mainRect)
	{
		float top = mainRect.yMax + 10;
		float height = position.height - mainRect.yMax;
		float center = top + height * 0.5f;

		GUI.enabled = false;
		Rect toggleBGRect = new Rect(0, top, position.width, height);
		GUI.Box(toggleBGRect, GUIContent.none);
		GUI.enabled = true;
		bool playerPref = EditorPrefs.GetBool(showPrefName, true);
		Rect toggleRect = new Rect(5, center - lineHeight * 0.75f, 100f, 20f);
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
		playerPref = EditorGUI.Toggle(toggleRect, "Show at Startup", playerPref);
#else
		playerPref = EditorGUI.ToggleLeft(toggleRect, "Show at Startup", playerPref);
#endif
		EditorPrefs.SetBool(showPrefName, playerPref);
	}

	private static Rect _ComputeBounds()
	{
		float windowWidth = 400f;
		float windowHeight = SECTR_Modules.HasComplete() && !SECTR_Modules.DEV ? 450f : 550f;
		float centerX = Screen.currentResolution.width * 0.5f;
		float centerY = Screen.currentResolution.height * 0.5f;
		return new Rect(centerX - windowWidth * 0.5f, centerY - windowHeight * 0.5f, windowWidth, windowHeight);
	}

	private static Texture2D LoadIcon(string iconName)
	{
		string iconSuffix = EditorGUIUtility.isProSkin ? "_White.psd" : "_Black.psd";
		iconName += iconSuffix;
		// Look for each icon first in the default path, only do a full search if we don't find it there.
		// Full search would be required if someone imports the library into a non-standard place.
		Texture2D icon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/SECTR/Code/Core/Editor/Icons/" + iconName, typeof(Texture2D));
		if(!icon)
		{
			icon = SECTR_Asset.Find<Texture2D>(iconName);
		}
		return icon;
	}
	#endregion
}
