using UnityEngine;
using System.Collections;

public class SwimmerMaster : InputMaster
{
    [SerializeField]
    private SwimmerPuppet _puppet;

    private bool _mouseDown;

    // Use this for initialization
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _mouseDown = true;
            _puppet.OnSwimStart(target);
        }
            

        if (Input.GetMouseButtonUp(0))
        {
            Vector3 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _mouseDown = false;
            _puppet.OnSwimEnd(target);
        }

        if (_mouseDown)
        {
            Vector3 target = Input.mousePosition;
            target.z = 0.5f;
            _puppet.OnSwimTowards(Camera.main.ScreenToWorldPoint(target));
        }

        if (Input.touchCount > 0)
        {
            Vector3 target = Input.GetTouch(0).position;
            target.z = 0.5f;
            _puppet.OnSwimTowards(Camera.main.ScreenToWorldPoint(target));
        }
    }
}
