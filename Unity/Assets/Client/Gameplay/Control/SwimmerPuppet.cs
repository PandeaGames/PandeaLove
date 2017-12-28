using UnityEngine;
using System.Collections;

public class SwimmerPuppet : InputPuppet
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

    public void OnSwimStart(Vector3 target)
    {
        _animator.SetBool(ANIM_PARAM_SWIMMING, true);
        _balancer.enabled = false;

        Vector3 dir = target - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void OnSwimEnd(Vector3 target)
    {
        _animator.SetBool(ANIM_PARAM_SWIMMING, false);
        _balancer.enabled = true;
    }

    public void OnSwimTowards(Vector3 target)
    {
        Vector3 targetDelta = target - transform.position;
        float dist = Vector3.Distance(target, transform.position);

        //get the angle between transform.forward and target delta
        float angleDiff = Vector3.Angle(transform.right, targetDelta);

        // get its cross product, which is the axis of rotation to
        // get from one vector to the other
        Vector3 cross = Vector3.Cross(transform.right, targetDelta);

        // apply torque along that axis according to the magnitude of the angle.
        _rigidBody.AddTorque(cross.z * angleDiff * 0.01f);

        float force = _swimForce;
        float aDelta = Mathf.PI - Mathf.Abs(cross.z);
        float pDelta = aDelta / Mathf.PI;

        force *= _swimForceCurve.Evaluate(pDelta);
        force *= _swimForceCurve.Evaluate(Mathf.Clamp(dist, 0, _forceDistance) / _forceDistance);

        Debug.Log(dist);

        _rigidBody.AddForce(transform.right * force, ForceMode2D.Force);
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
