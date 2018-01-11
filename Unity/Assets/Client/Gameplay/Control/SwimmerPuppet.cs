using UnityEngine;
using System.Collections;

public class SwimmerPuppet : SimplePointerMaster.SimplePointerPuppet
{
    private const string ANIM_PARAM_SWIMMING = "Swimming";

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private Rigidbody2D _rigidBody;

    [SerializeField]
    private ServiceManager _serviceManager;

    [SerializeField]
    private SimpleFollowAgent _swimFollowAgent;

    [SerializeField]
    private AnchoredFollowAgent _anchoredFollowAgent;

    [SerializeField]
    private Transform _cameraAnchorTarget;

    [SerializeField]
    private Balancer _balancer;

    [SerializeField]
    private AnimationCurve _swimForceCurve;

    [SerializeField]
    private float _swimForce = 1;

    [SerializeField]
    private float _forceDistance = 4;

    [SerializeField]
    private float _torque = 1;

    [SerializeField]
    private float _swimMass = 1;

    [SerializeField]
    private float _swimLinearDrag = 1;

    [SerializeField]
    private float _swimAngularDrag = 1;

    [SerializeField]
    private float _swimGravityScale = 1;

    private CameraService _cameraService;
    private bool _flipped;
    private float _mass;
    private float _linearDrag;
    private float _angularDrag;
    private float _gravityScale;

    public void Start()
    {
        _cameraService = _serviceManager.GetService<CameraService>();
    }

    public void Update()
    {
        UpdateFlip();
    }

    public override void PuppetUpdate()
    {

    }

    public void OnAimStart(Vector3 target)
    {
    }

    public void OnAimEnd()
    {
    }

    public void OnAim(Vector3 target)
    {
    }

    public override void OnPointerStart(Vector3 target)
    {
        _animator.SetBool(ANIM_PARAM_SWIMMING, true);
        _balancer.enabled = false;

        Game2DMathUtils.LookAt(fromTransform: transform, toTarget: target);

        _mass = _rigidBody.mass;
        _linearDrag = _rigidBody.drag;
        _angularDrag = _rigidBody.angularDrag;
        _gravityScale = _rigidBody.gravityScale;

        _rigidBody.mass = _swimMass;
        _rigidBody.drag = _swimLinearDrag;
        _rigidBody.angularDrag = _swimAngularDrag;
        _rigidBody.gravityScale = _swimGravityScale;
    }

    public override void OnPointerEnd()
    {
        _animator.SetBool(ANIM_PARAM_SWIMMING, false);
        _balancer.enabled = true;

        _rigidBody.mass = _mass;
        _rigidBody.drag = _linearDrag;
        _rigidBody.angularDrag = _angularDrag;
        _rigidBody.gravityScale = _gravityScale;
    }

    public override void OnPointer(Vector3 target)
    {
        Game2DMathUtils.ApplyTorqueAndForce(
            fromTransform:transform,
            toTarget:target,
            body:_rigidBody,
            torque: _torque,
            force:_swimForce,
            rotationCurve:_swimForceCurve,
            distanceClamp: _forceDistance,
            distanceCurve:_swimForceCurve
            );
    }

    private void UpdateFlip()
    {
        float rotation = transform.eulerAngles.z;

        if (!_flipped)
            _flipped |= rotation > 110 && rotation < 230;
        else if (rotation < 80 || rotation > 280)
        {
            _flipped = false;
        }

        transform.localScale = _flipped ? new Vector3(1, -1, 1): new Vector3(1, 1, 1);
    }

    public override void PuppetFocusOn()
    {
    }

    public override void PuppetFocusOff()
    {
    }

    public override void OnPointerStart(Vector3 target, int index)
    {
        if (index == 0)
        {
            OnPointerStart(target);
        }
        else if (index == 1)
        {
            OnAimStart(target);
        }
    }

    public override void OnPointerEnd(int index)
    {
        if (index == 0)
        {
            OnPointerEnd();
        }
        else if (index == 1)
        {
            OnAimEnd();
        }
    }

    public override void OnPointer(Vector3 target, int index)
    {
        if (index == 0)
        {
            OnPointer(target);
        }
        else if (index == 1)
        {
            OnAim(target);
        }
    }
}
