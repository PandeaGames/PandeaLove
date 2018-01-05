using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimplePointerMaster : InputMaster
{
    public abstract class SimplePointerPuppet : InputPuppet
    {
        public abstract void OnPointerStart(Vector3 target, int index);
        public abstract void OnPointerStart(Vector3 target);
        public abstract void OnPointerEnd(int index);
        public abstract void OnPointerEnd();
        public abstract void OnPointer(Vector3 target, int index);
        public abstract void OnPointer(Vector3 target);
        public abstract void PuppetFocusOn();
        public abstract void PuppetFocusOff();
    }

    [SerializeField]
    private SimplePointerPuppet _puppet;

    private bool _pointerDown;
    private Dictionary<int, bool> _touchDown = new Dictionary<int, bool>();
    private int _touchCount;

    // Update is called once per frame
    void Update()
    {
        _puppet.PuppetUpdate();

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target.z = 0.5f;

            _pointerDown = true;
            _puppet.OnPointerStart(target);
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector3 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target.z = 0.5f;

            _pointerDown = false;
            _puppet.OnPointerEnd();
        }

        if (_pointerDown)
        {
            Vector3 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target.z = 0.5f;

            _puppet.OnPointer(target);
        }

        Touch touch;

        for(int i = Input.touchCount; i < _touchCount; i++)
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
