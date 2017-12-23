using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    private float _speedX = 0.5f;
    private float _speedY = 0.5f;

    // Use this for initialization
    void Start () {
		
	}

    public void Update()
    {
        float moveX = Input.GetAxis("Horizontal") * _speedX;
        float moveY = Input.GetAxis("Vertical") * _speedY;

        gameObject.transform.position = new Vector2(transform.position.x + moveX, transform.position.y + moveY);
    }
}
