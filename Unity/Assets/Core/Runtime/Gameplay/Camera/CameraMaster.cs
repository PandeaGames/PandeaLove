using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMaster : MonoBehaviour {

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private CameraAgent _cameraAgent;

    [SerializeField]
    private CameraService _cameraService;

    public void Focus(CameraAgent agent)
    {
        _cameraAgent = agent;
    }

    // Use this for initialization
    void Start ()
    {
        _cameraService.RegisterMaster(this);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (_cameraAgent != null)
            UpdateCameraPosition(_cameraAgent);
	}

    private void UpdateCameraPosition(CameraAgent agent)
    {
        Vector3 pos = agent.GetCameraPosition();

        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
        transform.rotation = agent.GetCameraRotation();

        _camera.orthographicSize = agent.GetCameraOrthographicScale();
    }
}
