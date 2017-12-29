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
    private Balancer _balancer;

    [SerializeField]
    private AnimationCurve _swimForceCurve;

    [SerializeField]
    private float _swimForce = 1;

    [SerializeField]
    private float _forceDistance = 4;

    private bool _flipped;

    public void Update()
    {
        UpdateFlip();
    }

    public override void PuppetUpdate()
    {

    }

    public override void OnPointerStart(Vector3 target)
    {
        _animator.SetBool(ANIM_PARAM_SWIMMING, true);
        _balancer.enabled = false;

        Game2DMathUtils.LookAt(fromTransform: transform, toTarget: target);
    }

    public override void OnPointerEnd()
    {
        _animator.SetBool(ANIM_PARAM_SWIMMING, false);
        _balancer.enabled = true;
    }

    public override void OnPointer(Vector3 target)
    {
        Game2DMathUtils.ApplyTorqueAndForce(
            fromTransform:transform,
            toTarget:target,
            body:_rigidBody,
            torque:0.01f,
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
}
