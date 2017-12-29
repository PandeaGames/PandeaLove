using UnityEngine;
using System.Collections;

public class CraftPuppet : SimplePointerMaster.SimplePointerPuppet
{
    [SerializeField]
    private Rigidbody2D _body;

    [SerializeField]
    private float _verticalForceScale = 1;

    [SerializeField]
    private float _horizontalForceScale = 1;

    [SerializeField]
    private float _pointerStartForceScale = 1;

    public override void PuppetUpdate()
    {
        //transform.rotation = Quaternion.Euler(0, 0, 0);
        _body.AddForce(Vector2.right * _horizontalForceScale, ForceMode2D.Force);
    }

    public override void OnPointerStart(Vector3 target)
    {
        _body.AddForce(Vector2.right * _pointerStartForceScale, ForceMode2D.Impulse);
    }

    public override void OnPointerEnd()
    {
        
    }

    public override void OnPointer(Vector3 target)
    {
        Vector3 delta = target - transform.position;
        Vector2 force = new Vector2(0, delta.y) * _verticalForceScale;
        _body.AddForce(force, ForceMode2D.Impulse);
    }
}
