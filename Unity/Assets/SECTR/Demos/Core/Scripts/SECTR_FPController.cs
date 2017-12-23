// Copyright (c) 2014 Make Code Now! LLC
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
#define UNITY_4
#endif

using UnityEngine;
using System.Collections.Generic;

/// \ingroup Demo
/// Simple abstract base class for first person style controllers.
/// 
/// This base class provides common services for FP style controllers,
/// like translating both touch and mouse based inputs into camera
/// rotation. 
[RequireComponent(typeof(Camera))]
public abstract class SECTR_FPController : MonoBehaviour 
{
	#region Private Details
	Vector2 _mouseAbsolute;
	Vector2 _smoothMouse;
	Vector2 _clampInDegrees = new Vector2(360f, 180f);
	Vector2 _targetDirection;
	bool focused = true;

	protected class TrackedTouch
	{
		public Vector2 startPos;
		public Vector2 currentPos;
	}
	protected Dictionary<int, TrackedTouch> _touches = new Dictionary<int, TrackedTouch>();
	#endregion

	#region Public Interface
	[SECTR_ToolTip("Whether to lock the cursor when this camera is active.")]
    public bool LockCursor = true;
	[SECTR_ToolTip("Scalar for mouse sensitivity.")]
    public Vector2 Sensitivity = new Vector2(2f, 2f);
	[SECTR_ToolTip("Scalar for mouse smoothing.")]
    public Vector2 Smoothing = new Vector2(3f, 3f);
	[SECTR_ToolTip("Adjusts the size of the virtual joystick.")]
	public float TouchScreenLookScale = 1f;
    #endregion

	#region Unity Interface
	// Use this for initialization
	void Start() 
	{
		// Set target direction to the camera's initial orientation.
        _targetDirection = transform.localRotation.eulerAngles;
	}

	void OnApplicationFocus(bool focused)
	{
		this.focused = focused;
	}
	
	protected virtual void Update()
	{
		if(!focused)
		{
			return;
		}

		// Ensure the cursor is always locked when set
#if UNITY_4
		Screen.lockCursor = true;
#else
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
#endif
		Quaternion targetOrientation = Quaternion.Euler(_targetDirection);

		Vector2 mouseDelta;
		if(Input.multiTouchEnabled && !Application.isEditor)
		{
			_UpdateTouches();
			mouseDelta = GetScreenJoystick(true);
		}
		else
		{
	        // Get raw mouse input for a cleaner reading on more sensitive mice.
			mouseDelta.x = Input.GetAxisRaw("Mouse X");
			mouseDelta.y = Input.GetAxisRaw("Mouse Y");
		}

		// Scale input against the sensitivity setting and multiply that against the smoothing value.
		mouseDelta = Vector2.Scale(mouseDelta, new Vector2(Sensitivity.x * Smoothing.x, Sensitivity.y * Smoothing.y));

		if(Input.multiTouchEnabled)
		{
			_smoothMouse = mouseDelta;
		}
		else
		{
			// Interpolate mouse movement over time to apply smoothing delta.
			_smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / Smoothing.x);
			_smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / Smoothing.y);
		}
		
		// Find the absolute mouse movement value from point zero.
		_mouseAbsolute += _smoothMouse;
		
		// Clamp and apply the local x value first, so as not to be affected by world transforms.
		if(_clampInDegrees.x < 360)
		{
			_mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -_clampInDegrees.x * 0.5f, _clampInDegrees.x * 0.5f);
		}
		
		Quaternion xRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right);
		transform.localRotation = xRotation;
		
		// Then clamp and apply the global y value.
		if (_clampInDegrees.y < 360)
		{
			_mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -_clampInDegrees.y * 0.5f, _clampInDegrees.y * 0.5f);
		}
		
		transform.localRotation *= targetOrientation;
		Quaternion yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
		transform.localRotation *= yRotation;
	}
	#endregion
	
	#region Private Methods
	protected Vector2 GetScreenJoystick(bool left)
	{
		foreach(TrackedTouch touch in _touches.Values)
		{
			float halfScreenWidth = Screen.width * 0.5f;
			if((left && touch.startPos.x < halfScreenWidth) ||
			   (!left && touch.startPos.x > halfScreenWidth))
			{
				Vector2 screenJoy = touch.currentPos - touch.startPos;
				screenJoy.x = Mathf.Clamp(screenJoy.x / (halfScreenWidth * 0.5f * TouchScreenLookScale), -1f, 1f);
				screenJoy.y = Mathf.Clamp(screenJoy.y / (Screen.height * 0.5f * TouchScreenLookScale), -1f, 1f);
				return screenJoy;
			}
		}
		return Vector2.zero;
	}
	
	void _UpdateTouches()
	{
		int numTouches = Input.touchCount;
		for(int touchIndex = 0; touchIndex < numTouches; ++touchIndex)
		{
			Touch touch = Input.touches[touchIndex];
			if(touch.phase == TouchPhase.Began)
			{
				Debug.Log("Touch " + touch.fingerId + "Started at : " + touch.position);
				TrackedTouch newTouch = new TrackedTouch();
				newTouch.startPos = touch.position;
				newTouch.currentPos = touch.position;
				_touches.Add(touch.fingerId, newTouch);
			}
			else if(touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
			{
				Debug.Log("Touch " + touch.fingerId + "Ended at : " + touch.position);
				_touches.Remove(touch.fingerId);
			}
			else
			{
				TrackedTouch currentTouch;
				if(_touches.TryGetValue(touch.fingerId, out currentTouch))
				{
					currentTouch.currentPos = touch.position;
				}
			}
		}

	}
	#endregion
}