using UnityEngine;
using System.Collections;

public class InputFocusOnStart : MonoBehaviour
{

    [SerializeField]
    private InputMaster _master;

    // Use this for initialization
    void Start()
    {
        _master.FocusOn();
    }
}
