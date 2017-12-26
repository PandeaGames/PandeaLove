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
            _mouseDown = true;

        if (Input.GetMouseButtonUp(0))
            _mouseDown = false;

        if (_mouseDown)
        {
            Vector3 mouse = Input.mousePosition;
            mouse.z = 10;
            _puppet.OnSwimTowards(Camera.main.ScreenToWorldPoint(mouse));
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 mouse = touch.position;
            mouse.z = 10;
            _puppet.OnSwimTowards(Camera.main.ScreenToWorldPoint(mouse));
        }
    }
}
