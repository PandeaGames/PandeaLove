using UnityEngine;
using System.Collections;

public class SimplePointerMaster : InputMaster
{
    public abstract class SimplePointerPuppet : InputPuppet
    {
        public abstract void OnPointerStart(Vector3 target);
        public abstract void OnPointerEnd();
        public abstract void OnPointer(Vector3 target);
    }

    [SerializeField]
    private SimplePointerPuppet _puppet;

    private bool _pointerDown;
    private bool _touchDown;

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

        if (Input.touchCount > 0)
        {
            Vector3 target = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            target.z = 0.5f;

            if (!_touchDown)
            {
                _touchDown = true;
                _puppet.OnPointerStart(target);
            }
            
            _puppet.OnPointer(target);
        }
        else if (_touchDown)
        {
            _touchDown = false;
            _puppet.OnPointerEnd();
        }
    }
}
