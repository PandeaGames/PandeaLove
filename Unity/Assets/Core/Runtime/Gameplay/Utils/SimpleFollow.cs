using UnityEngine;
using System.Collections;

public class SimpleFollow : MonoBehaviour
{
    [SerializeField]
    private Transform _target;

    [SerializeField]
    private bool overrideX;

    [SerializeField]
    private bool overrideY;

    [SerializeField]
    private bool overrideZ;

    [SerializeField]
    private Vector3 overrides;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(
            overrideX ? overrides.x: _target.position.x,
            overrideY ? overrides.y : _target.position.y,
            overrideZ ? overrides.z : _target.position.z
            );
    }
}
