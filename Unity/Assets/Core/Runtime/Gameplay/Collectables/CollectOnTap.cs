using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectOnTap : MonoBehaviour {

    [SerializeField]
    private Collectable _collectable;

    [SerializeField]
    private CircleCollider2D _alternateRadius;

    protected void OnMouseDown()
    {
        _collectable.Collect();
    }

    private void OnPointer(Vector2 cameraPosition, Vector2 worldPosition, RaycastHit2D[] raycast = null)
    {

    }

    protected void Update()
    {
        if (Input.GetMouseButtonDown(0) && _alternateRadius)
        {
            Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float dist = Vector2.Distance(target, (Vector2)transform.position + _alternateRadius.offset);

            if (dist < _alternateRadius.radius)
                _collectable.Collect();
        }
    }
}