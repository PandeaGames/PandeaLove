using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class SECTR_Undo 
{
	public static void Record(Object undoObject, string undoString)
	{
		#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
		Undo.RegisterUndo(undoObject, undoString);
		#else
		Undo.RecordObject(undoObject, undoString);
		#endif
	}

	public static void Created(Object undoObject, string undoString)
	{
		Undo.RegisterCreatedObjectUndo(undoObject, undoString);
	}

	public static void Destroy(GameObject undoObject, string undoString)
	{
		#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
		GameObject.DestroyImmediate(undoObject);
		#else
		Undo.DestroyObjectImmediate(undoObject);
		#endif
	}

	public static void Parent(GameObject parent, GameObject child, string undoString)
	{
		#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2
		Undo.RegisterSetTransformParentUndo(child.transform, parent.transform, undoString);
		child.transform.parent = parent.transform;
		#else
		Undo.SetTransformParent(child.transform, parent.transform, undoString);
		#endif
	}
}
