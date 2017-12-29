using UnityEngine;

public class SimpleFollowAgent : CameraAgent
{
    [SerializeField]
    private Transform _target;

    [SerializeField]
    private Vector3 offset;

    public override Vector3 GetCameraPosition()
    {
        if (_target == null)
            return Vector3.zero;

        Vector3 position = new Vector3(_target.position.x, _target.position.y, -1);

        return position + offset;
    }

    public override Quaternion GetCameraRotation()
    {
        return new Quaternion();
    }
}