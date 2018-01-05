using UnityEngine;

public abstract class InputMaster : MonoBehaviour
{
    public delegate void focusDelgate(InputMaster master);

    public event focusDelgate OnFocusOn;
    public event focusDelgate OnFocusOff;

    public virtual void FocusOn()
    {
        if (OnFocusOn != null)
            OnFocusOn(this);
    }

    public virtual void FocusOff()
    {
        if (OnFocusOff != null)
            OnFocusOff(this);
    }
}