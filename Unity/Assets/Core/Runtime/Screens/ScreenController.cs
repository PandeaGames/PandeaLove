using UnityEngine;
using System.Collections;
using System;

public class ScreenController : MonoBehaviour
{
    [Serializable]
    public class Config : ScriptableObject
    {

    }

    public delegate void ScreenControllerDelegate(ScreenController controller);

    public event ScreenControllerDelegate OnTransitionComplete;

    private WindowController _window;
    private Config _config;
    private RectTransform _rectTransform;

    public void Setup(WindowController window, Config config)
    {
        _window = window;
        _config = config;
    }

    public void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Update()
    {
        if(_rectTransform != null && _rectTransform.hasChanged)
        {
            _rectTransform.anchorMin = Vector2.zero;
            _rectTransform.anchorMax = Vector2.one;
            _rectTransform.sizeDelta = Vector2.zero;

            _rectTransform.hasChanged = false;
        }
    }

    public void Transition(ScreenTransition transition)
    {
        StartCoroutine(DelayedTransitionComplete());
    }

    private IEnumerator DelayedTransitionComplete()
    {
        yield return null;

        TransitionComplete();
    }

    protected void TransitionComplete()
    {
        if (OnTransitionComplete != null)
            OnTransitionComplete(this);
    }
}
