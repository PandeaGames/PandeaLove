using UnityEngine;
using System.Collections;

public interface IInputPuppet
{
    void PuppetUpdate();
    void PuppetFocusOn();
    void PuppetFocusOff();
}
