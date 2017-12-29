using UnityEngine;
using System.Collections;

public class AnchoredFollowAgent : CameraAgent
{
    [SerializeField]
    private Vector3 _scale;

    [SerializeField]
    private Transform _anchor;

    [SerializeField]
    private Transform _chained;

    [SerializeField]
    private Vector3 _offset;

    public override Vector3 GetCameraPosition()
    {
        Vector3 delta = _anchor.position - _chained.position;
        Vector3 scaledDelta = new Vector3(delta.x * _scale.x + _offset.x, delta.y * _scale.y + _offset.y, delta.z*_scale.z + _offset.z);

        return _anchor.position + scaledDelta;
    }
}
