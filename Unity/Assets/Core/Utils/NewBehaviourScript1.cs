using UnityEngine;
using System.Collections;

public class InputUtils
{
    private static RaycastHit2D _pointerRaycastHit;
    private static double _pointerRaycastTimestamp;
    
    public static Vector2 GetPointer()
    {
        return Vector2.zero;
    }
}