// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System.Collections.Generic;

/// \ingroup Stream
/// Loads SECTR_Chunk components that this object is in at Start and nothing more.
///
/// StartLoader is designed to be combined with SECTR_LoadingDoor in order to make
/// sure that the reference count of the initial Sector(s) work out correctly with
/// the load/unload logic of the door.
/// 
/// StartLoader self-destructs immediately after doing its work in order to
/// eliminate its overhead post-start.
[RequireComponent(typeof(SECTR_Member))]
[AddComponentMenu("SECTR/Stream/SECTR Start Loader")]
public class SECTR_StartLoader : SECTR_Loader 
{
	#region Private Details
	private Texture2D fadeTexture = null;
	private float fadeAmount = 1;

	private SECTR_Member cachedMember = null;
	#endregion

	#region Public Interface
	[SECTR_ToolTip("Set to true if the scene should start at black and fade in when loaded.")]
	public bool FadeIn = false;
	[SECTR_ToolTip("Amount of time to fade in.", "FadeIn")]
	public float FadeTime = 2;
	[SECTR_ToolTip("The color to fade the screen to on load.", "FadeIn")]
	public Color FadeColor = Color.black;
	[System.NonSerialized]
	public bool Paused = false;

	/// Returns true if all referenced Chunks are loaded. False, otherwise.
	public override bool Loaded
	{
		get
		{
			bool loaded = true;
			int numSectors = cachedMember ? cachedMember.Sectors.Count : 0;
			for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
			{
				SECTR_Sector sector = cachedMember.Sectors[sectorIndex];
				if(sector.Frozen)
				{
					SECTR_Chunk chunk = sector.GetComponent<SECTR_Chunk>();
					if(chunk && !chunk.IsLoaded())
					{
						loaded = false;
						break;
					}
				}
			}
			return loaded;
		}
	}
	#endregion
	
	#region Unity Interface
	void OnEnable()
	{
		cachedMember = GetComponent<SECTR_Member>();

		if(FadeIn)
		{
			fadeTexture = new Texture2D(1, 1);
			fadeTexture.SetPixel(0,0, FadeColor);
			fadeTexture.Apply();
		}
	}

	void OnDisable()
	{
		cachedMember = null;
		fadeTexture = null;
	}

	void Start()
	{
		cachedMember.ForceUpdate(true);
		int numSectors = cachedMember.Sectors.Count;
		for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
		{
			SECTR_Sector sector = cachedMember.Sectors[sectorIndex];
			SECTR_Chunk chunk = sector.GetComponent<SECTR_Chunk>();
			if(chunk)
			{
				chunk.AddReference();
			}
		}

		LockSelf(true);
	}

	void Update()
	{
		if(Loaded)
		{
			if(locked)
			{
				LockSelf(false);
			}

			if(!FadeIn)
			{
				GameObject.Destroy(this);
			}
		}
	}

	void OnGUI()
	{
		if(FadeIn && enabled)
		{
			if(Loaded && !Paused)
			{
				float loadDelta = Time.deltaTime / FadeTime;
				fadeAmount -= loadDelta;
				fadeAmount = Mathf.Clamp01(fadeAmount);
			}

			GUI.color = new Color(1, 1, 1, fadeAmount);
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeTexture);

			if(fadeAmount == 0)
			{
				GameObject.Destroy(this);
			}
		}
	}
	#endregion
}
