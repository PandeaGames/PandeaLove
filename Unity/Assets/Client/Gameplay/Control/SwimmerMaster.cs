using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwimmerMaster : InputMaster
{
    [SerializeField]
    private SwimmerPuppet _puppet;

    [SerializeField]
    private ServiceManager _serviceManager;

    private bool _pointerDown;
    private Dictionary<int, bool> _touchDown = new Dictionary<int, bool>();
    private int _touchCount;
    private SwimmerFocusService _swimmerFocusService;

    public void Start()
    {
        _swimmerFocusService = _serviceManager.GetService<SwimmerFocusService>();
    }

    void Update()
    {
        GameObject focusedObject = _swimmerFocusService.FocusedObject;
        bool hasFocus = focusedObject != null;
        _puppet.PuppetUpdate();

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target.z = 0.5f;

            _pointerDown = true;
            _puppet.OnPointerStart(target);
            _swimmerFocusService.Blur();
        }

        if (Input.GetMouseButtonUp(0) && _pointerDown)
        {
            _pointerDown = false;
            _puppet.OnPointerEnd();
        }

        if (_pointerDown || hasFocus)
        {
            Vector3 target = hasFocus ? focusedObject.transform.position : Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target.z = 0.5f;

            _puppet.OnPointer(target);
        }

        Touch touch;

        for (int i = Input.touchCount; i < _touchCount; i++)
        {
            _puppet.OnPointerEnd(i);
            _touchDown.Add(0, false);
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            touch = Input.touches[i];
            Vector3 target = Camera.main.ScreenToWorldPoint(touch.position);
            target.z = 0.5f;

            if (!_touchDown.ContainsKey(i) || !_touchDown[i])
            {
                _touchDown.Add(0, true);
                _puppet.OnPointerStart(target);
            }

            _puppet.OnPointer(target, i);
        }

        _touchCount = Input.touchCount;
    }

    public override void FocusOn()
    {
        base.FocusOn();
        _puppet.PuppetFocusOn();
    }

    public override void FocusOff()
    {
        base.FocusOff();
        _puppet.PuppetFocusOff();
    }
}