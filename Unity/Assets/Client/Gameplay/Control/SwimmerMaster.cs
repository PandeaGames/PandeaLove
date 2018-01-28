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
    private ContactFilter2D _contactFiler2D;

    public void Start()
    {
        _swimmerFocusService = _serviceManager.GetService<SwimmerFocusService>();
        _contactFiler2D = new ContactFilter2D();
        _contactFiler2D.useTriggers = true;
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

            if (IsValidTap(target))
            {
                _pointerDown = true;
                _puppet.OnPointerStart(target);
                _swimmerFocusService.Blur();
            }
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

    private bool IsValidTap(Vector2 position)
    {
        RaycastHit2D[] results = new RaycastHit2D[1];
        int resultsCount = Physics2D.Raycast(position, Vector2.zero, _contactFiler2D, results);

        if (resultsCount > 0)
        {
            foreach(RaycastHit2D hit in results)
            {
                if (hit.collider.tag == Tags.COLLECTABLE)
                    return false;
            }
        }

        return true;
    }
}