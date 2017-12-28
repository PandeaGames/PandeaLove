using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balancer : MonoBehaviour {

    [SerializeField]
    private float _scale = 1;

    private Rigidbody2D _target;

	// Use this for initialization
	void Start () {
        _target = GetComponent<Rigidbody2D>();
    }
	
	// Update is called once per frame
	void Update () {

        Vector3 balance = new Vector3(transform.position.x, transform.position.y+5, transform.position.z);

        Vector3 targetDelta = balance - transform.position;

        //get the angle between transform.forward and target delta
        float angleDiff = Vector3.Angle(transform.right, targetDelta);

        // get its cross product, which is the axis of rotation to
        // get from one vector to the other
        Vector3 cross = Vector3.Cross(transform.right, targetDelta);

        // apply torque along that axis according to the magnitude of the angle.
        _target.AddTorque(cross.z * angleDiff * _scale);
    }
}
