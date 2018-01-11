using UnityEngine;
using System.Collections;

public class SwimmerFocusService : Service
{
    public delegate void SwimmerFocusDelegate(GameObject obj);
    public delegate void SwimmerBlurDelegate();

    public event SwimmerFocusDelegate OnSwimmerFocus;
    public event SwimmerBlurDelegate OnSwimmerBlur;

    private GameObject _focusedObject;

    public GameObject FocusedObject { get { return _focusedObject; } }

    public void Focus(GameObject obj)
    {
        _focusedObject = obj;

        if (OnSwimmerFocus != null)
            OnSwimmerFocus(obj);
    }

    public void Blur()
    {
        _focusedObject = null;

        if (OnSwimmerBlur != null)
            OnSwimmerBlur();
    }
}
