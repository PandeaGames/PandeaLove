using UnityEngine;
using System.Collections;

public class CollectionService : Service
{
    public delegate void CollectDelegate(Collectable collectable);
    public event CollectDelegate OnCollect;

    [SerializeField]
    private bool _enforceSingleFrameCollection = true;

    private Collectable _pendingCollection;
    private InputService _inputService;

    public override void StartService(ServiceManager serviceManager)
    {
        base.StartService(serviceManager);

        _inputService = serviceManager.GetService<InputService>();

        _inputService.OnPointerDown += OnPointer;
    }

    public override void EndService(ServiceManager serviceManager)
    {
        base.EndService(serviceManager);

        _inputService.OnPointerDown -= OnPointer;
        _inputService = null;
    }

    protected void Update()
    {
        if(_pendingCollection)
        {
            _pendingCollection.Collected();

            if (OnCollect != null)
                OnCollect(_pendingCollection);

            _pendingCollection = null;
        }
    }

    public bool TryCollect(Collectable collectable)
    {
        if (_pendingCollection && _enforceSingleFrameCollection)
            return false;

        _pendingCollection = collectable;

        return true;
    }

    private void OnPointer(Vector2 cameraPosition, Vector2 worldPosition, RaycastHit2D[] raycast = null)
    {
        Collectable collectable = null;

        if(raycast != null)
        {
            foreach (RaycastHit2D hit in raycast)
            {
                if (hit.collider.tag == Tags.COLLECTABLE)
                {
                    collectable = hit.collider.gameObject.GetComponent<Collectable>();
                }
            }
        }

        if (collectable != null)
        {
            collectable.Collect();
        }
    }
}
