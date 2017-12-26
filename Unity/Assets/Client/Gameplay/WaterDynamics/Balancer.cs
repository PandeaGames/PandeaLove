using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balancer : MonoBehaviour {

    [SerializeField]
    private Transform _bottomWeight;

    [SerializeField]
    private Transform _topWeight;

    [SerializeField]
    private Rigidbody2D _target;

    [SerializeField]
    private float _scale = 1;

	// Use this for initialization
	void Start () {
       // _target.centerOfMass = _bottomWeight.position;
    }
	
	// Update is called once per frame
	void Update () {
        Vector2 v = transform.TransformDirection(_bottomWeight.position) - transform.TransformDirection(_topWeight.position);
        float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        _target.AddTorque((angle / -1000)*_scale);
    }
}
