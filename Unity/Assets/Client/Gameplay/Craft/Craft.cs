﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Craft : MonoBehaviour
{
    [SerializeField]
    private List<MonoBehaviour> _disableOnEnter;

    [SerializeField]
    private List<MonoBehaviour> _enableOnEnter;

    [SerializeField]
    private List<MonoBehaviour> _disableOnExit;

    [SerializeField]
    private List<MonoBehaviour> _enableOnExit;

    [SerializeField]
    private InputMaster _inputMaster;

    [SerializeField]
    private CameraAgent _cameraAgent;

    [SerializeField]
    private ServiceManager _serviceManager;

    private CameraService _cameraService;
    private CraftOperator _craftOperator;

    // Use this for initialization
    void Start()
    {
        _cameraService = _serviceManager.GetService<CameraService>();
    }

    // Update is called once per frame
    void Update()
    {
        if(_craftOperator)
            _craftOperator.transform.position = transform.position;
    }

    public void DestroyCraft()
    {
        _inputMaster.FocusOff();
        _cameraService.Focus();
        _craftOperator.OnCraftExited(this);
        Destroy(gameObject);
    }

    public bool IsOperable(CraftOperator craftOperator)
    {
        return true;
    }

    public bool AttemptEnterCraft(CraftOperator craftOperator)
    {
        if (IsOperable(craftOperator))
        {
            EnterCraft(craftOperator);
            return true;
        }

        return false;
    }

    protected void EnterCraft(CraftOperator craftOperator)
    {
        _craftOperator = craftOperator;

        foreach (MonoBehaviour script in _disableOnEnter)
            script.enabled = false;
        foreach (MonoBehaviour script in _enableOnEnter)
            script.enabled = true;

        _inputMaster.FocusOn();
        _cameraService.Focus(_cameraAgent);
        craftOperator.OnCraftEntered(this);
    }

    protected void ExitCraft(CraftOperator craftOperator)
    {
        foreach (MonoBehaviour script in _disableOnExit)
            script.enabled = false;
        foreach (MonoBehaviour script in _enableOnExit)
            script.enabled = true;
    }
}
