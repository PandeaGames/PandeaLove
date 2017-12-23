// Copyright (c) 2014 Make Code Now! LLC

[System.AttributeUsage(System.AttributeTargets.Field)]
public class SECTR_ToolTip : System.Attribute
{
	#region Private Details
	private string tipText = null;
	private string dependentProperty = null;
	private float min = 0;
	private float max = 0;
	private System.Type enumType = null;
	private bool hasRange = false;
	private bool devOnly = false;
	private bool sceneObjectOverride = false;
	private bool allowSceneObjects = false;
	private bool treatAsLayer = false;
	#endregion

	#region Public Interface
	public SECTR_ToolTip(string tipText)
	{
		this.tipText = tipText;
	}

	public SECTR_ToolTip(string tipText, float min, float max)
	{
		this.tipText = tipText;
		this.min = min;
		this.max = max;
		this.hasRange = true;
	}

	public SECTR_ToolTip(string tipText, string dependentProperty)
	{
		this.tipText = tipText;
		this.dependentProperty = dependentProperty;
	}

	public SECTR_ToolTip(string tipText, string dependentProperty, float min, float max)
	{
		this.tipText = tipText;
		this.dependentProperty = dependentProperty;
		this.min = min;
		this.max = max;
		this.hasRange = true;
	}

	public SECTR_ToolTip(string tipText, bool devOnly)
	{
		this.tipText = tipText;
		this.devOnly = devOnly;
	}

	public SECTR_ToolTip(string tipText, bool devOnly, bool treatAsLayer)
	{
		this.tipText = tipText;
		this.devOnly = devOnly;
		this.treatAsLayer = treatAsLayer;
	}

	public SECTR_ToolTip(string tipText, string dependentProperty, System.Type enumType)
	{
		this.tipText = tipText;
		this.dependentProperty = dependentProperty;
		this.enumType = enumType;
	}

	public SECTR_ToolTip(string tipText, string dependentProperty, bool allowSceneObjects)
	{
		this.tipText = tipText;
		this.dependentProperty = dependentProperty;
		this.sceneObjectOverride = true; 
		this.allowSceneObjects = allowSceneObjects;
	}

	public string TipText 					{ get { return tipText; } }
	public string DependentProperty 		{ get { return dependentProperty; } }
	public float Min 						{ get { return min; } }
	public float Max 						{ get { return max; } }
	public System.Type EnumType 			{ get { return enumType; } }
	public bool HasRange 					{ get { return hasRange; } }
	public bool DevOnly 					{ get { return devOnly; } }
	public bool SceneObjectOverride 		{ get { return sceneObjectOverride; } }
	public bool AllowSceneObjects 			{ get { return allowSceneObjects; } }
	public bool TreatAsLayer	 			{ get { return treatAsLayer; } }
	#endregion
}