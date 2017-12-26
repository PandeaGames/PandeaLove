using UnityEngine;

public abstract class InputMaster : MonoBehaviour
{
    public delegate void focusDelgate(InputMaster master);

    public event focusDelgate OnFocusOn;
    public event focusDelgate OnFocusOff;

    public void FocusOn()
    {
        if (OnFocusOn != null)
            OnFocusOn(this);
    }

    public void FocusOff()
    {
        if (OnFocusOff != null)
            OnFocusOff(this);
    }
}