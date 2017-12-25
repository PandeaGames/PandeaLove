using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMaster : MonoBehaviour {

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private CameraService _cameraService;

	// Use this for initialization
	void Start () {
        _cameraService.RegisterMaster(this);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
