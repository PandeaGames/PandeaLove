using UnityEngine;
using System.Collections;
using System;

public class ScreenController : MonoBehaviour
{
    public delegate void ScreenControllerDelegate(ScreenController controller);

    public event ScreenControllerDelegate OnTransitionComplete;

    [Serializable]
    public class Config : ScriptableObject
    {

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
