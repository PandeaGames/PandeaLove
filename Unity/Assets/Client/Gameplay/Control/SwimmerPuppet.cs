using UnityEngine;
using System.Collections;

public class SwimmerPuppet : InputPuppet
{

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private Rigidbody2D _rigidBody;

    public void OnSwimTowards(Vector3 position)
    {
        Vector3 targetDir = position - transform.position;
        float step = 1 * Time.deltaTime;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
        transform.rotation = Quaternion.LookRotation(newDir);

       /* Vector2 v = transform.TransformDirection(transform.position) - transform.TransformDirection(position);
        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        _rigidBody.AddTorque((angle / 10) * 1);
        _rigidBody.AddRelativeForce(Vector3.forward*20, ForceMode2D.Impulse);*/
    }
}
