// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

public abstract class SECTR_Editor : Editor 
{
	#region Private Details
	private class PropertyData
	{
		public PropertyData(System.Type propertyType, SerializedProperty property, SerializedProperty dependentProperty, SECTR_ToolTip toolTip)
		{
			this.propertyType = propertyType;
			this.property = property;
			this.dependentProperty = dependentProperty;
			// Fancy reg-ex to insert spaces between lower and upper case letters while preserving acronyms.
			this.niceName = ObjectNames.NicifyVariableName(property.name);
			this.niceName = char.ToUpper(this.niceName[0]) + this.niceName.Substring(1);
			if(toolTip != null)
			{
				this.toolTip = toolTip;
			}
			else
			{
				this.toolTip = new SECTR_ToolTip(null);
			}
		}

		public System.Type propertyType;
		public SerializedProperty property;
		public SerializedProperty dependentProperty;
		public string niceName;
		public SECTR_ToolTip toolTip;
	}

	private List<PropertyData> properties = new List<PropertyData>(64);
	private Dictionary<string, PropertyData> propertyTable = new Dictionary<string, PropertyData>(64);
	private Object proxy = null;
	private SerializedObject mySerializedObject = null;

	// A hack for the some GUI drawing.
	public int WidthOverride = 0;
	#endregion

	#region Unity Interface
	public virtual void OnEnable()
	{
		proxy = null;
		_ExtractProperties();
	}

	public override void OnInspectorGUI()
	{
		mySerializedObject.Update();
		int numProperties = properties.Count;
		for(int propertyIndex = 0; propertyIndex < numProperties; ++propertyIndex)
		{
			PropertyData property = properties[propertyIndex];
			_DrawProperty(property);
		}
		mySerializedObject.ApplyModifiedProperties();
	}
	#endregion

	#region Utilities for Derived Classes 
	protected void SetProxy(Object proxy)
	{
		if(this.proxy != proxy)
		{
			this.proxy = proxy;
			_ExtractProperties();
		}
	}

	protected void DrawProperty(string propertyName)
	{
		PropertyData property;
		if(propertyTable.TryGetValue(propertyName, out property))
		{
			_DrawProperty(property);
		}
	}

	protected void DrawSliderProperty(string propertyName, float min, float max)
	{
		PropertyData property;
		if(propertyTable.TryGetValue(propertyName, out property))
		{
			if(_ShouldDraw(property))
			{
				if(property.property.propertyType == SerializedPropertyType.Float)
				{
					float oldValue = property.property.floatValue;
					GUI.SetNextControlName(property.niceName + "_Slider");
					EditorGUILayout.Slider(property.property, min, max, new GUIContent(property.niceName,  property.toolTip.TipText));
					if(property.property.floatValue != oldValue)
					{
						property.property.floatValue = Mathf.Clamp(property.property.floatValue, min, max);
					}
				}
				else if(property.property.propertyType == SerializedPropertyType.Integer)
				{
					int oldValue = property.property.intValue;
					GUI.SetNextControlName(property.niceName + "_Slider");
					EditorGUILayout.IntSlider(property.property, (int)min, (int)max, new GUIContent(property.niceName,  property.toolTip.TipText));
					if(property.property.intValue != oldValue)
					{
						property.property.intValue = Mathf.Clamp(property.property.intValue, (int)min, (int)max);
					}
				}
				else
				{
					_DrawProperty(property);
				}
			}
		}
	}

	protected void DrawMinMaxProperty(string propertyName, float min, float max)
	{
		PropertyData property;
		if(propertyTable.TryGetValue(propertyName, out property))
		{
			if(_ShouldDraw(property))
			{
				if(property.property.propertyType == SerializedPropertyType.Vector2)
				{
					GUIStyle labelStyle = new GUIStyle(EditorStyles.label);

					float propMin = property.property.vector2Value.x;
					float propMax = property.property.vector2Value.y;
					#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(new GUIContent(property.niceName, property.toolTip.TipText));
					labelStyle.alignment = TextAnchor.MiddleLeft;
					GUILayout.Label("Min", labelStyle);
					GUI.SetNextControlName(property.niceName + "_Min");
					propMin = EditorGUILayout.FloatField(propMin);
					GUI.SetNextControlName(property.niceName + "_Slider");
					EditorGUILayout.MinMaxSlider(new GUIContent("",  property.toolTip.TipText), ref propMin, ref propMax, min, max);
					GUI.SetNextControlName(property.niceName + "_Max");
					propMax = EditorGUILayout.FloatField(propMax);
					labelStyle.alignment = TextAnchor.MiddleLeft;
					GUILayout.Label("Max");
					EditorGUILayout.EndHorizontal();
					#else
					float windowWidth = WidthOverride > 0 ? WidthOverride - 25 : Screen.width;
					Rect controlRect;
					if(WidthOverride > 0)
					{
						controlRect = EditorGUILayout.GetControlRect(true, GUILayout.Width(windowWidth));
					}
					else
					{
						controlRect = EditorGUILayout.GetControlRect(true);
					}
					Rect valueRect = EditorGUI.PrefixLabel(controlRect, 0, new GUIContent(property.niceName, property.toolTip.TipText));
					float labelWidth = 25;
					float fieldWidth = 45;
					float sliderWidth = (valueRect.width) - (labelWidth * 2f) - (fieldWidth * 2f);
					float insertPos = valueRect.x - 1;
					labelStyle.alignment = TextAnchor.MiddleLeft;
					GUI.Label(new Rect(insertPos, controlRect.y, labelWidth, controlRect.height), "Min", labelStyle);
					insertPos += labelWidth;
					GUI.SetNextControlName(property.niceName + "_Min");
					propMin = EditorGUI.FloatField(new Rect(insertPos, controlRect.y, fieldWidth, controlRect.height), propMin);
					insertPos += fieldWidth;
					if(sliderWidth > 0)
					{
						GUI.SetNextControlName(property.niceName + "_Slider");
					#if UNITY_5_5_OR_NEWER
						EditorGUI.MinMaxSlider(new Rect(insertPos, controlRect.y, sliderWidth, controlRect.height), new GUIContent("",  property.toolTip.TipText), ref propMin, ref propMax, min, max);
					#else
						EditorGUI.MinMaxSlider(new GUIContent("",  property.toolTip.TipText), new Rect(insertPos, controlRect.y, sliderWidth, controlRect.height), ref propMin, ref propMax, min, max);
					#endif
						insertPos += sliderWidth;
					}
					GUI.SetNextControlName(property.niceName + "_Max");
					propMax = EditorGUI.FloatField(new Rect(insertPos, controlRect.y, fieldWidth, controlRect.height), "", propMax);
					insertPos += fieldWidth;
					labelStyle.alignment = TextAnchor.MiddleLeft;
					GUI.Label(new Rect(insertPos, controlRect.y, labelWidth, controlRect.height), "Max");
					#endif

					propMin = Mathf.Max(min, propMin);
					propMax = Mathf.Min(max, propMax);
					propMax = Mathf.Max(propMin, propMax);
					propMin = Mathf.Min(propMin, propMax);
					Vector2 newValue = new Vector2(propMin, propMax);
					if(newValue != property.property.vector2Value)
					{
						property.property.vector2Value = newValue;
					}
				}
				else
				{
					_DrawProperty(property);
				}
			}
		}
	}

	protected T ObjectField<T>(string fieldName, string toolTip, Object targetObject, bool allowSceneObjects) where T : Object
	{
		GUI.SetNextControlName(fieldName + "_Control");
		return (T)EditorGUILayout.ObjectField(new GUIContent(fieldName, toolTip), targetObject, typeof(T), allowSceneObjects);
	}
	#endregion

	#region Private Details
	private void _DrawProperty(PropertyData property)
	{
		if(_ShouldDraw(property))
		{
			GUIContent label = new GUIContent(property.niceName, property.toolTip.TipText);
			GUI.SetNextControlName(property.niceName + "_Control");
			if(property.toolTip.EnumType != null && property.property.propertyType == SerializedPropertyType.Enum)
			{
				System.Enum enumValue = System.Enum.ToObject(property.toolTip.EnumType, property.property.intValue) as System.Enum;
				int newValue = System.Convert.ToInt32(EditorGUILayout.EnumMaskField(label, enumValue));
				if(property.property.intValue != newValue)
				{
					property.property.intValue = newValue;
				}
			}
			else if(property.toolTip.HasRange && property.property.propertyType == SerializedPropertyType.Integer)
			{
				if(property.toolTip.Max >= property.toolTip.Min)
				{
					int oldValue = property.property.intValue;
					EditorGUILayout.IntSlider(property.property, (int)property.toolTip.Min, (int)property.toolTip.Max, label);
					if(property.property.intValue != oldValue)
					{
						property.property.intValue = Mathf.Clamp(property.property.intValue, (int)property.toolTip.Min, (int)property.toolTip.Max);
					}
				}
				else
				{
					int newValue = EditorGUILayout.IntField(label, property.property.intValue);
					if(property.property.intValue != newValue)
					{
						property.property.intValue = Mathf.Max((int)property.toolTip.Min, newValue);
					}
				}
			}
			else if(property.toolTip.HasRange && property.property.propertyType == SerializedPropertyType.Float)
			{
				if(property.toolTip.Max >= property.toolTip.Min)
				{
					float oldValue = property.property.floatValue;
					EditorGUILayout.Slider(property.property, property.toolTip.Min, property.toolTip.Max, label);
					if(property.property.floatValue != oldValue)
					{
						property.property.floatValue = Mathf.Clamp(property.property.floatValue, property.toolTip.Min, property.toolTip.Max);
					}
				}
				else
				{
					float newValue = EditorGUILayout.FloatField(label, property.property.floatValue);
					if(property.property.floatValue != newValue)
					{
						property.property.floatValue = Mathf.Max(property.toolTip.Min, newValue);
					}
				}
			}
			else if(property.toolTip.SceneObjectOverride && property.property.propertyType == SerializedPropertyType.ObjectReference)
			{
				Object newValue = EditorGUILayout.ObjectField(label, property.property.objectReferenceValue, property.propertyType, property.toolTip.AllowSceneObjects);
				if(property.property.objectReferenceValue != newValue)
				{
					property.property.objectReferenceValue = newValue;
				}
			}
			else if(property.toolTip.TreatAsLayer && property.property.propertyType == SerializedPropertyType.Integer)
			{
				int newValue = EditorGUILayout.LayerField(label, property.property.intValue);
				if(property.property.intValue != newValue)
				{
					property.property.intValue = newValue;
				}
			}
			else
			{
				EditorGUILayout.PropertyField(property.property, label, true);
			}
		}
	}

	private void _ExtractProperties()
	{
		properties.Clear();
		propertyTable.Clear();

		Object targetObject;
		if(proxy != null)
		{
			mySerializedObject = new SerializedObject(proxy);
			targetObject = proxy;
		}
		else if(serializedObject != null)
		{
			mySerializedObject = serializedObject;
			targetObject = target;
		}
		else
		{
			return;
		}

		FieldInfo[] fields = targetObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach(FieldInfo field in fields)
		{
			if((field.Attributes & (FieldAttributes.NotSerialized | FieldAttributes.Static)) == 0)
			{
				if(!field.IsPublic)
				{
					object[] serialize = field.GetCustomAttributes(typeof(SerializeField), true);
					if(serialize == null || serialize.Length <= 0)
					{
						continue;
					}
				}

				object[] hide = field.GetCustomAttributes(typeof(HideInInspector), true);
				if(hide != null && hide.Length > 0)
				{
					continue;
				}
				
				SerializedProperty dependentProperty = null;
				SECTR_ToolTip toolTip = null;
				object[] tooltips = field.GetCustomAttributes(typeof(SECTR_ToolTip), true);
				if(tooltips.Length > 0)
				{
					toolTip = ((SECTR_ToolTip)tooltips[0]);
					if(!string.IsNullOrEmpty(toolTip.DependentProperty))
					{
						dependentProperty = mySerializedObject.FindProperty(toolTip.DependentProperty);
					}
				}
				PropertyData newProperty = new PropertyData(field.FieldType, mySerializedObject.FindProperty(field.Name), dependentProperty, toolTip);
				properties.Add(newProperty);
				propertyTable.Add(field.Name, newProperty);
			}
		}
	}

	private bool _ShouldDraw(PropertyData property)
	{
		if(property.toolTip.DevOnly && !SECTR_Modules.DEV)
		{
			return false;
		}
		else if(property.dependentProperty != null)
		{
			switch(property.dependentProperty.propertyType)
			{
			case SerializedPropertyType.ObjectReference:
				return property.dependentProperty.objectReferenceValue != null;
			case SerializedPropertyType.Boolean:
				return property.dependentProperty.boolValue;
			case SerializedPropertyType.Integer:
				return property.dependentProperty.intValue != 0;
			}

			if(property.dependentProperty.isArray)
			{
				return property.dependentProperty.arraySize > 0;
			}
		}
		return true;
	}
	#endregion
}
