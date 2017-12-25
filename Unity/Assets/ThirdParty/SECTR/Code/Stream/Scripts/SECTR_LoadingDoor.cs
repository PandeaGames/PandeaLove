// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System.Collections.Generic;

/// \ingroup Stream
/// Extends the basic SECTR_Door with awareness of streaming SECTR_Chunks. This door won't open
/// unless the SECTR_Chunks on both sides of the door's SECTR_Portal are loaded.
/// 
/// Unity restricts their async APIs to Pro owners, which means that when
/// a Chunk is loaded, it may cause a noticeable hitch for non-Pro users.
/// This component is an example of how to hide that hitch.
[AddComponentMenu("SECTR/Stream/SECTR Loading Door")]
public class SECTR_LoadingDoor : SECTR_Door 
{
	#region Private Members
	private enum FadeMode
	{
		None,
		FadeIn,
		FadeOut,
		Hold,
	}
	
	private Texture2D fadeTexture = null;

	private class LoadRequest
	{
		public SECTR_Chunk chunkToLoad = null;
		public SECTR_Chunk chunkToUnload = null;
		public SECTR_Chunk loadedChunk = null;
		public bool enteredFront = false;
		public bool enteredBack = false;

		public FadeMode fadeMode = FadeMode.None;
		public float fadeAmount = 0;
		public float holdStart = 0;
	}
	Dictionary<Collider, LoadRequest> loadRequests = new Dictionary<Collider, LoadRequest>(4);
	#endregion

	#region Public Interface
	[SECTR_ToolTip("Specifies which layers are allow to cause loads (vs simply opening the door).")]
	public LayerMask LoadLayers = (int)0xffffff;
	[SECTR_ToolTip("Should screen fade to black before loading.")]
	public bool FadeBeforeLoad = false;
	[SECTR_ToolTip("How long to fade out before loading. Also, how long to fade back in.", "FadeBeforeLoad")]
	public float FadeTime = 1f;
	[SECTR_ToolTip("How long to stay faded out. Helps cover pops right at the moment of loading.", "FadeBeforeLoad")]
	public float HoldTime = 0.1f;
	[SECTR_ToolTip("The color to fade the screen to on load.", "FadeBeforeLoad")]
	public Color FadeColor = Color.black;
	#endregion

	#region Unity Interface
	protected override void OnEnable()
	{
		base.OnEnable();

		if(FadeBeforeLoad)
		{
			fadeTexture = new Texture2D(1, 1);
			fadeTexture.SetPixel(0,0, FadeColor);
			fadeTexture.Apply();
		}
	}

	protected override void OnTriggerEnter(Collider other)
	{
		base.OnTriggerEnter(other);
		if(Portal && ((LoadLayers & 1 << other.gameObject.layer) != 0))
		{
			SECTR_Chunk oppositeChunk = _GetOppositeChunk(other.transform.position);
			if(oppositeChunk)
			{
				LoadRequest loadRequest;
				SECTR_Chunk postUnload = null;
				if(loadRequests.TryGetValue(other, out loadRequest))
				{
					if(loadRequest.chunkToUnload)
					{
						postUnload = loadRequest.chunkToUnload;
						loadRequest.chunkToUnload = null;
					}
				}
				else
				{
					loadRequest = new LoadRequest();
				}

				if(FadeBeforeLoad && !oppositeChunk.IsLoaded())
				{
					loadRequest.fadeMode = FadeMode.FadeOut;
				}

				loadRequest.enteredFront = oppositeChunk.Sector == Portal.BackSector;
				loadRequest.enteredBack = oppositeChunk.Sector == Portal.FrontSector;
				if(FadeBeforeLoad)
				{
					loadRequest.chunkToLoad = oppositeChunk;
				}
				else
				{
					oppositeChunk.AddReference();
					loadRequest.loadedChunk = oppositeChunk;
				}
				loadRequests[other] = loadRequest;

				if(postUnload)
				{
					postUnload.RemoveReference();
				}
			}
		}
	}
	
	protected override void OnTriggerExit(Collider other)
	{
		base.OnTriggerExit(other);

		if(Portal && ((LoadLayers & 1 << other.gameObject.layer) != 0))
		{
			SECTR_Chunk oppositeChunk = _GetOppositeChunk(other.transform.position);
			if(oppositeChunk)
			{
				LoadRequest loadRequest = loadRequests[other];
				if(FadeBeforeLoad && loadRequest.fadeMode == FadeMode.FadeOut)
				{
					loadRequest.fadeMode = FadeMode.FadeIn;
				}

				bool exitedBack = oppositeChunk.Sector == Portal.FrontSector;
				bool exitedFront = oppositeChunk.Sector == Portal.BackSector;
				if(loadRequest.loadedChunk && ((loadRequest.enteredFront && exitedFront) || (loadRequest.enteredBack && exitedBack)))
				{
					loadRequest.chunkToUnload = loadRequest.loadedChunk;
				}
				else if((loadRequest.enteredFront && exitedBack) || (loadRequest.enteredBack && exitedFront))
				{
					loadRequest.chunkToUnload = oppositeChunk;
				}
				else
				{
					loadRequest.chunkToUnload = loadRequest.loadedChunk;
				}

				if(loadRequests.Count > 1 || IsClosed())
				{
					if(loadRequest.chunkToUnload)
					{
						loadRequest.chunkToUnload.RemoveReference();
					}
					loadRequests.Remove(other);
				}
			}
		}
	}

	void OnGUI()
	{
		if(FadeBeforeLoad)
		{
			float loadDelta = Time.deltaTime / FadeTime;
			float currentFade = 0;
			foreach(LoadRequest loadRequest in loadRequests.Values)
			{
				switch(loadRequest.fadeMode)
				{
				case FadeMode.FadeOut:
				{
					loadRequest.fadeAmount += loadDelta;
					if(loadRequest.fadeAmount >= 1f)
					{
						if(loadRequest.chunkToLoad)
						{
							loadRequest.chunkToLoad.AddReference();
							loadRequest.loadedChunk = loadRequest.chunkToLoad;
							loadRequest.chunkToLoad = null;
						}
						loadRequest.fadeMode = FadeMode.Hold;
						loadRequest.holdStart = Time.time;
					}
				}
					break;
				case FadeMode.FadeIn:
				{
					loadRequest.fadeAmount -= loadDelta;
					if(loadRequest.fadeAmount <= 0)
					{
						loadRequest.fadeMode = FadeMode.None;
					}
				}
					break;
				case FadeMode.Hold:
					if(!CanOpen())
					{
						loadRequest.holdStart = Time.time;
					}
					else if(Time.time >= loadRequest.holdStart + HoldTime)
					{
						loadRequest.fadeMode = FadeMode.FadeIn;
					}
					break;
				}

				loadRequest.fadeAmount = Mathf.Clamp01(loadRequest.fadeAmount);
				currentFade = Mathf.Max(currentFade, loadRequest.fadeAmount);
			}

			if(currentFade > 0f)
			{
				GUI.color = new Color(1, 1, 1, currentFade);
				GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeTexture);
			}
		}
	}
	#endregion

	#region Door Interface
	protected override bool CanOpen()
	{
		if(Portal)
		{
			if(!_IsSectorLoaded(Portal.FrontSector))
			{
				return false;
			}
			if(!_IsSectorLoaded(Portal.BackSector))
			{
				return false;
			}
		}
		return true;
	}

	void OnClose()
	{
		if(loadRequests.Count == 1)
		{
			Dictionary<Collider, LoadRequest>.Enumerator enumerator = loadRequests.GetEnumerator();
			enumerator.MoveNext();
			LoadRequest loadRequest = enumerator.Current.Value;
			if(loadRequest.chunkToUnload)
			{
				loadRequest.chunkToUnload.RemoveReference();
				loadRequests.Clear();
			}
		}
	}
	#endregion

	#region Private Methods
	private bool _IsSectorLoaded(SECTR_Sector sector)
	{
		if(sector && sector.Frozen)
		{
			SECTR_Chunk chunk = sector.GetComponent<SECTR_Chunk>();
			if(chunk && !chunk.IsLoaded())
			{
				return false;
			}
		}
		return true;
	}

	private SECTR_Chunk _GetOppositeChunk(Vector3 position)
	{
		if(Portal)
		{
			SECTR_Sector oppositeSector = SECTR_Geometry.IsPointInFrontOfPlane(position, Portal.Center, Portal.Normal) ? Portal.BackSector : Portal.FrontSector;
			if(oppositeSector)
			{
				return oppositeSector.GetComponent<SECTR_Chunk>();
			}
		}
		return null;
	}
	#endregion
}
