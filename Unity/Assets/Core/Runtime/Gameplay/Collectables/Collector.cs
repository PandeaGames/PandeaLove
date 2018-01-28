using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collector : MonoBehaviour {

    protected void OnCollisionEnter2D(Collision2D collision)
    {
        Collectable collectable = collision.gameObject.GetComponent<Collectable>();

        if(collectable)
        {
            collectable.Collect();
        }
    }
}