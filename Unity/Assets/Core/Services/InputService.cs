using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputService : Service
{
    public delegate void OnMultiPointer(Vector2 cameraPosition, Vector2 worldPosition, int index, RaycastHit2D[] raycast = null);
    public delegate void OnPointer(Vector2 cameraPosition, Vector2 worldPosition, RaycastHit2D[] raycast = null);

    public event OnPointer OnPointerDown;
    public event OnPointer OnPointerUp;
    public event OnPointer OnPointerMove;

    public event OnMultiPointer OnMultiPointerDown;
    public event OnMultiPointer OnMultiPointerUp;
    public event OnMultiPointer OnMultiPointerMove;

    [SerializeField]
    private bool _touchEnabled;
    [SerializeField]
    private bool _providePonterRaycast;
    [SerializeField]
    private bool _useTriggersInRaycast;
    [SerializeField]
    private int _maxRaycastResults = 1;

    private ContactFilter2D _contactFilter = default(ContactFilter2D);
    private bool _pointerDown;
    private int _touchCount;
    private Dictionary<int, Vector2> _touchDown;

    public override void StartService(ServiceManager serviceManager)
    {
        base.StartService(serviceManager);

        _touchDown = new Dictionary<int, Vector2>();
        _contactFilter.useTriggers = _useTriggersInRaycast;
    }

    public override void EndService(ServiceManager serviceManager)
    {
        base.EndService(serviceManager);

        _touchDown.Clear();
        _touchDown = null;
    }

    protected virtual void Update()
    {
        HandlePointers();

        if(_touchEnabled)
            HandleTouches();
    }

    private void HandlePointers()
    {
        if (Input.GetMouseButtonDown(0) && OnPointerDown!=null)
        {
            HandlePointerAction(Input.mousePosition, OnPointerDown);
            _pointerDown = true;
        }

        if (Input.GetMouseButtonUp(0) && _pointerDown)
        {
            HandlePointerAction(Input.mousePosition, OnPointerUp);
            _pointerDown = false;
        }
    }

    private void HandleTouches()
    {
        Touch touch;

        for (int i = Input.touchCount; i < _touchCount; i++)
        {
            Vector2 cameraPosition;
            _touchDown.TryGetValue(i, out cameraPosition);

            HandleMultiPointerAction(cameraPosition, i, OnMultiPointerUp, OnPointerUp);
            _touchDown.Remove(i);
        }

        for (int i = 0; i < Input.touchCount; i++)
        {
            touch = Input.touches[i];

            Vector2 cameraPosition = touch.position;

            Vector3 target = Camera.main.ScreenToWorldPoint(touch.position);
            target.z = 0.5f;

            if (!_touchDown.ContainsKey(i))
            {
                _touchDown.Add(i, cameraPosition);
                HandleMultiPointerAction(cameraPosition, i, OnMultiPointerDown, OnPointerDown);
            }

            HandleMultiPointerAction(cameraPosition, i, OnMultiPointerMove, OnPointerMove);
        }

        _touchCount = Input.touchCount;
    }

    private void HandlePointerAction(Vector2 cameraPosition, OnPointer eventToNotify)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(cameraPosition);

        RaycastHit2D[] results = null;

        if (_providePonterRaycast)
        {
            results = new RaycastHit2D[_maxRaycastResults];
            Physics2D.Raycast(worldPosition, Vector2.zero, _contactFilter, results);
        }

        if(eventToNotify != null)
            eventToNotify(cameraPosition, worldPosition, results);
    }

    private void HandleMultiPointerAction(Vector2 cameraPosition, int index, OnMultiPointer eventToNotify, OnPointer pointerEventToNotify)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(cameraPosition);

        RaycastHit2D[] results = null;

        if (_providePonterRaycast)
        {
            results = new RaycastHit2D[_maxRaycastResults];
            Physics2D.Raycast(worldPosition, Vector2.zero, _contactFilter, results);
        }

        if (eventToNotify != null)
            eventToNotify(cameraPosition, worldPosition, index, results);

        if(index == 0 && pointerEventToNotify != null)
            pointerEventToNotify(cameraPosition, worldPosition, results);
    }
}