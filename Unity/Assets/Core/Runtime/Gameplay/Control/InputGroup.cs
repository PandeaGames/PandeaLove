using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputGroup : MonoBehaviour
{
    [SerializeField]
    private List<InputMaster> _masters;

    private InputMaster _focused;

    // Use this for initialization
    void Awake()
    {
        foreach(InputMaster master in _masters)
        {
            master.enabled = false;
            master.OnFocusOff += OnMasterFocusOff;
            master.OnFocusOn += OnMasterFocusOn;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnDestroy()
    {
        foreach (InputMaster master in _masters)
        {
            master.OnFocusOff -= OnMasterFocusOff;
            master.OnFocusOn -= OnMasterFocusOn;
        }
    }

    private void OnMasterFocusOn(InputMaster master)
    {
        if (_focused != null)
            _focused.enabled = false;

        master.enabled = true;
        _focused = master;
    }

    private void OnMasterFocusOff(InputMaster master)
    {
        master.enabled = false;
        _focused = null;
    }
}
