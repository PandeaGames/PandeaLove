using UnityEngine;
using UnityEditor;

public class SimpleFollowAgent : CameraAgent
{
    [SerializeField]
    private Transform _target;

    public void Start()
    {
    }

    public override Vector3 GetCameraPosition()
    {
        if (_target == null)
            return Vector3.zero;

        Vector3 position = new Vector3(_target.position.x, _target.position.y, -1);

        return position;
    }

    public override Quaternion GetCameraRotation()
    {
        //if (_target != null)
           // return _target.rotation;
        return new Quaternion();
    }
}