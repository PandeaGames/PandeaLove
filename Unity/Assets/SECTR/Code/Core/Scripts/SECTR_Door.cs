// Copyright (c) 2014 Make Code Now! LLC
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
#define UNITY_4
#endif

using UnityEngine;
using System.Collections;

/// \ingroup Core
/// Implements a basic door component that is Portal aware. Also, provides an interface
/// that more complex doors can implement.
/// 
/// This door contains two base states (Open and Closed) and two transitional states (Opening and
/// Closing). The animations for Open and Closed should be Looping animations, with one-shot
/// animations for the transitions.
/// 
/// Door supports an optional reference to a SECTR_Portal. If set, the Door will manage the Closed
/// flag of the Portal, which other systems will find useful. 
[RequireComponent(typeof(Animator))]
[AddComponentMenu("SECTR/Audio/SECTR Door")]
public class SECTR_Door : MonoBehaviour 
{
	#region Private Details
	private int controlParam = 0;
	private int canOpenParam = 0;
	private int closedState = 0;
	private int waitingState = 0;
	private int openingState = 0;
	private int openState = 0;
	private int closingState = 0;
	private int lastState = 0;
	private Animator cachedAnimator = null;

	private int openCount = 0;
	#endregion

	[SECTR_ToolTip("The portal this door affects (if any).")]
	public SECTR_Portal Portal = null;
	[SECTR_ToolTip("The name of the control param in the door.")]
	public string ControlParam = "Open";
	[SECTR_ToolTip("The name of the control param that indicates if we are allowed to open.")]
	public string CanOpenParam = "CanOpen";
	[SECTR_ToolTip("The full name (layer and state) of the Open state in the Animation Controller.")]
	public string OpenState = "Base Layer.Open";
	[SECTR_ToolTip("The full name (layer and state) of the Closed state in the Animation Controller.")]
	public string ClosedState = "Base Layer.Closed";
	[SECTR_ToolTip("The full name (layer and state) of the Opening state in the Animation Controller.")]
	public string OpeningState = "Base Layer.Opening";
	[SECTR_ToolTip("The full name (layer and state) of the Closing state in the Animation Controller.")]
	public string ClosingState = "Base Layer.Closing";
	[SECTR_ToolTip("The full name (layer and state) of the Wating state in the Animation Controller.")]
	public string WaitingState = "Base Layer.Waiting";

	/// Opens the door. Exposed for use by other script classes.
	public void OpenDoor()
	{
		++openCount;
	}
	
	/// Closes the door. Exposed for use by other script classes.
	public void CloseDoor()
	{
		--openCount;
	}
	
	public bool IsFullyOpen()
	{
		AnimatorStateInfo info = cachedAnimator.GetCurrentAnimatorStateInfo(0);
#if UNITY_4
		return info.nameHash == openState;
#else
		return info.fullPathHash == openState;
#endif
	}

	public bool IsClosed()
	{
		AnimatorStateInfo info = cachedAnimator.GetCurrentAnimatorStateInfo(0);
#if UNITY_4
		return info.nameHash == closedState;
#else
		return info.fullPathHash == closedState;
#endif
	}

	#region Unity Interface
	protected virtual void OnEnable()
	{
		cachedAnimator = GetComponent<Animator>();

		controlParam = Animator.StringToHash(ControlParam);
		canOpenParam = Animator.StringToHash(CanOpenParam);
		closedState = Animator.StringToHash(ClosedState);
		waitingState = Animator.StringToHash(WaitingState);
		openingState = Animator.StringToHash(OpeningState);
		openState = Animator.StringToHash(OpenState);
		closingState = Animator.StringToHash(ClosingState);
	}

	void Start()
	{
		if(controlParam != 0)
		{
			cachedAnimator.SetBool(controlParam, false);
		}
		if(canOpenParam != 0)
		{
			cachedAnimator.SetBool(canOpenParam, false);
		}
		if(Portal)
		{
			Portal.SetFlag(SECTR_Portal.PortalFlags.Closed, true);
		}
		openCount = 0;
		lastState = closedState;
		SendMessage("OnClose", SendMessageOptions.DontRequireReceiver);
	}
	
	void Update()
	{
		bool canOpen = CanOpen();
		if(canOpenParam != 0)
		{
			cachedAnimator.SetBool(canOpenParam, canOpen);
		}
		if(controlParam != 0 && (canOpen || canOpenParam != 0))
		{
			if(openCount > 0)
			{
				cachedAnimator.SetBool(controlParam, true);
			}
			else
			{
				cachedAnimator.SetBool(controlParam, false);
			}
		}

#if UNITY_4
		int currentState = cachedAnimator.GetCurrentAnimatorStateInfo(0).nameHash;
#else
		int currentState = cachedAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
#endif
		if(currentState != lastState)
		{
			if(currentState == closedState)
			{
				SendMessage("OnClose", SendMessageOptions.DontRequireReceiver);
			}
			if(currentState == waitingState)
			{
				SendMessage("OnWaiting", SendMessageOptions.DontRequireReceiver);
			}
			else if(currentState == openingState)
			{
				SendMessage("OnOpening", SendMessageOptions.DontRequireReceiver);
			}
			if(currentState == openState)
			{
				SendMessage("OnOpen", SendMessageOptions.DontRequireReceiver);
			}
			else if(currentState == closingState)
			{
				SendMessage("OnClosing", SendMessageOptions.DontRequireReceiver);
			}
			lastState = currentState;
		}
		
		if(Portal)
		{
			Portal.SetFlag(SECTR_Portal.PortalFlags.Closed, IsClosed());
		}
	}

	protected virtual void OnTriggerEnter(Collider other)
	{
		++openCount;
	}
	
	protected virtual void OnTriggerExit(Collider other)
	{
		--openCount;
	}
	#endregion

	#region Door Interface
	// For subclasses to override
	protected virtual bool CanOpen()
	{
		return true;
	}
	#endregion
}
