using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSmooth : MonoBehaviour {

    private Vector3 speed = Vector3.zero;
    Camera camera;
    public float x = 0.5f, y = 0.5f;
    public float Delay = 0.15f;
    public Transform Focus;

    private void Start()
    {
        camera = GetComponent<Camera>();
    }
    void FixedUpdate()
    {
        if (Focus)
        {
            Vector3 point = camera.WorldToViewportPoint(Focus.position);
            Vector3 delta = Focus.position - camera.ViewportToWorldPoint(new Vector3(x, y, point.z)); 
            Vector3 destination = transform.position + delta;
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref speed, Delay);
        }

    }
}
