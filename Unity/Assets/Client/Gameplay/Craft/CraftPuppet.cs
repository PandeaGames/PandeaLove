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

    [SerializeField]
    private float _driveMass = 1;

    [SerializeField]
    private float _driveLinearDrag = 1;

    [SerializeField]
    private float _driveAngularDrag = 1;

    [SerializeField]
    private float _driveGravityScale = 1;

    private float _mass;
    private float _linearDrag;
    private float _angularDrag;
    private float _gravityScale;

    public override void PuppetUpdate()
    {
        _body.AddForce(Vector2.right * _horizontalForceScale, ForceMode2D.Impulse);
    }

    public override void PuppetFocusOn()
    {
        _mass = _body.mass;
        _linearDrag = _body.drag;
        _angularDrag = _body.angularDrag;
        _gravityScale = _body.gravityScale;

        _body.mass = _driveMass;
        _body.drag = _driveLinearDrag;
        _body.angularDrag = _driveAngularDrag;
        _body.gravityScale = _driveGravityScale;
    }

    public override void PuppetFocusOff()
    {
        _body.mass = _mass;
        _body.drag = _linearDrag;
        _body.angularDrag = _angularDrag;
        _body.gravityScale = _gravityScale;
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

    public override void OnPointerStart(Vector3 target, int index)
    {
        throw new System.NotImplementedException();
    }

    public override void OnPointerEnd(int index)
    {
        throw new System.NotImplementedException();
    }

    public override void OnPointer(Vector3 target, int index)
    {
        throw new System.NotImplementedException();
    }
}
