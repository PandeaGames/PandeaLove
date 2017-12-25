// Copyright (c) 2014 Make Code Now! LLC
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
#define UNITY_4
#endif

using UnityEngine;
using System.Collections.Generic;

/// \ingroup Demo
/// A simple harness for demo messages and input handling. 
[RequireComponent(typeof(SECTR_FPSController))]
[AddComponentMenu("SECTR/Demos/SECTR Demo UI")]
public class SECTR_DemoUI : MonoBehaviour 
{
	#region Private Details
	protected enum WatermarkLocation
	{
		UpperLeft,
		UpperCenter,
		UpperRight,
	};
		
	protected delegate void DemoButtonPressedDelegate(bool active);
	
	protected bool passedIntro = false;
	protected SECTR_FPSController cachedController = null;
	protected GUIStyle demoButtonStyle = null;
	protected WatermarkLocation watermarkLocation = WatermarkLocation.UpperRight;

	private class DemoButton
	{
		public DemoButton(KeyCode key, string activeHint, string inactiveHint, DemoButtonPressedDelegate demoButtonPressed)
		{
			this.key = key;
			this.activeHint = activeHint;
			if(string.IsNullOrEmpty(inactiveHint))
			{
				this.inactiveHint = activeHint;
			}
			else
			{
				this.inactiveHint = inactiveHint;
			}
			this.demoButtonPressed = demoButtonPressed;
		}

		public KeyCode key;
		public string activeHint;
		public string inactiveHint;
		public bool active = false;
		public bool pressed = false;
		public DemoButtonPressedDelegate demoButtonPressed = null;
	}
	private List<DemoButton> demoButtons = new List<DemoButton>(4);
	#endregion

	#region Public Interface
	[SECTR_ToolTip("Texture to display as a watermark.")]
	public Texture2D Watermark;
	[SECTR_ToolTip("Link to a controllable ghost/spectator camera.")]
	public SECTR_GhostController PipController;
	[SECTR_ToolTip("Message to display at start of demo.")]
	[Multiline] public string DemoMessage;
	[SECTR_ToolTip("Disables HUD for video captures.")]
	public bool CaptureMode = false;

	public bool PipActive
	{
		get { return PipController && PipController.enabled; }
	}
	#endregion

	#region Unity Interface
	protected virtual void OnEnable()
	{
		if(PipController)
		{
			PipController.enabled = false;
			AddButton(KeyCode.P, "Control Player", "Control PiP", PressedPip);
		}
		cachedController = GetComponent<SECTR_FPSController>();
		passedIntro = string.IsNullOrEmpty(DemoMessage) || CaptureMode;
		if(!passedIntro)
		{
			cachedController.enabled = false;
			if(PipController)
			{
				PipController.GetComponent<Camera>().enabled = false;
			}
		}
	}

	protected virtual void OnDisable()
	{
		if(PipController)
		{
			PipController.enabled = false;
		}
		cachedController = null;
		demoButtons.Clear();
	}

	protected virtual void OnGUI()
	{
		if(CaptureMode)
		{
			return;
		}

		float gutter = 25;

		if(Watermark)
		{
			float height = Screen.height * 0.1f;
			float width = ((float)Watermark.width / (float)Watermark.height) * height;
			GUI.color = new Color(1f, 1f, 1f, 0.2f);
			switch(watermarkLocation)
			{
			case WatermarkLocation.UpperLeft:
				GUI.DrawTexture(new Rect(gutter, gutter, width, height), Watermark);
				break;
			case WatermarkLocation.UpperCenter:
				GUI.DrawTexture(new Rect(Screen.width * 0.5f - width * 0.5f, gutter, width, height), Watermark);
				break;
			case WatermarkLocation.UpperRight:
				GUI.DrawTexture(new Rect(Screen.width - gutter - width, gutter, width, height), Watermark);
				break;
			}
			GUI.color = Color.white;
		}

		if(demoButtonStyle == null)
		{
			demoButtonStyle = new GUIStyle(GUI.skin.box);
			demoButtonStyle.alignment = TextAnchor.UpperCenter;
			demoButtonStyle.wordWrap = true;
			demoButtonStyle.fontSize = 20;
			demoButtonStyle.normal.textColor = Color.white;
		}

		if(!passedIntro)
		{
#if UNITY_4
			Screen.lockCursor = true;
#else
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
#endif
			string message = DemoMessage;
			if(Input.multiTouchEnabled && !Application.isEditor)
			{
				message += "\n\nPress to Continue";
			}
			else
			{
				message += "\n\nPress Any Key to Continue";
			}

			GUIContent messageContent = new GUIContent(message);
			float width = Screen.width * 0.75f;
			float height = demoButtonStyle.CalcHeight(messageContent, width);
			Rect messageRect = new Rect(Screen.width * 0.5f - width * 0.5f, Screen.height * 0.5f - height * 0.5f, width, height);
			GUI.Box(messageRect, messageContent, demoButtonStyle);
			if((Event.current.type == EventType.KeyDown))
			{
				passedIntro = true;
				cachedController.enabled = true;
				if(PipController)
				{
					PipController.GetComponent<Camera>().enabled = true;
				}
			}
		}
		else if(demoButtons.Count > 0)
		{
			int numDemoButtons = demoButtons.Count;
			float maxPackedWidth = (Screen.width / (numDemoButtons)) - gutter * 2;
			float buttonWidth = Mathf.Min(150f, maxPackedWidth);
			float totalWidth = (numDemoButtons * buttonWidth) + ((numDemoButtons - 1) * gutter);
			float start = Screen.width * 0.5f - totalWidth * 0.5f;

			for(int buttonIndex = 0; buttonIndex < numDemoButtons; ++buttonIndex)
			{
				DemoButton demoButton = demoButtons[buttonIndex];
				bool onMultiTouch = Input.multiTouchEnabled && !Application.isEditor;

				string inputLabel = demoButton.key.ToString();
				switch(demoButton.key)
				{
				case KeyCode.Alpha0:
				case KeyCode.Alpha1:
				case KeyCode.Alpha2:
				case KeyCode.Alpha3:
				case KeyCode.Alpha4:
				case KeyCode.Alpha5:
				case KeyCode.Alpha6:
				case KeyCode.Alpha7:
				case KeyCode.Alpha8:
				case KeyCode.Alpha9:
					inputLabel = inputLabel.Replace("Alpha", "");
					break;
				}
				GUIContent inputContent = new GUIContent(inputLabel);
				float inputGap = onMultiTouch ? 0f : 5f;
				float inputWidth = 50;
				float inputHeight = onMultiTouch ? 0 : demoButtonStyle.CalcHeight(inputContent, inputWidth);


				string message = demoButton.active ? demoButton.activeHint : demoButton.inactiveHint;
				GUIContent messageContent = new GUIContent(message);
				float height = demoButtonStyle.CalcHeight(messageContent, buttonWidth);
				Rect messageRect = new Rect(start + ((buttonWidth + gutter) * buttonIndex), Screen.height - height - inputGap - inputHeight - gutter, buttonWidth, height);
				
				if(onMultiTouch && !demoButton.pressed)
				{
					demoButton.pressed = GUI.Button(messageRect, messageContent, demoButtonStyle);
				}
				else if(!onMultiTouch)
				{
					GUI.Box(messageRect, messageContent, demoButtonStyle);
					Rect inputRect = new Rect(start + ((buttonWidth + gutter) * buttonIndex) + buttonWidth * 0.5f - inputWidth * 0.5f, Screen.height - inputHeight - gutter, inputWidth, inputHeight);
					GUI.Box(inputRect, inputContent, demoButtonStyle);
				}

				if(demoButton.pressed || (Event.current.type == EventType.KeyUp && Event.current.keyCode == demoButton.key))
				{
					demoButton.pressed = false;
					demoButton.active = !demoButton.active;
					if(demoButton.demoButtonPressed != null)
					{
						demoButton.demoButtonPressed(demoButton.active);
					}
				}
			}
		}
	}
	#endregion

	#region Private Details
	protected void AddButton(KeyCode key, string activeHint, string inactiveHint, DemoButtonPressedDelegate buttonPressedDelegate)
	{
		demoButtons.Add(new DemoButton(key, activeHint, inactiveHint, buttonPressedDelegate));
	}

	private void PressedPip(bool active)
	{
		if(PipActive)
		{
			PipController.enabled = false;
			cachedController.enabled = true;
		}
		else
		{
			PipController.enabled = true;
			cachedController.enabled = false;
		}
	}
	#endregion
}
