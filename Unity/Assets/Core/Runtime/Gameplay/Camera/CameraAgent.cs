using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CameraAgent : MonoBehaviour {

    [SerializeField]
    protected ServiceManager _serviceManager;

    [SerializeField]
    private float scale = 1;

    protected CameraService _cameraService;

    public abstract Vector3 GetCameraPosition();

    public void Start()
    {
        _cameraService = _serviceManager.GetService<CameraService>();
    }

    public virtual Quaternion GetCameraRotation()
    {
        return new Quaternion();
    }

    public virtual void FocusStart(CameraMaster master)
    {

    }

    public virtual void FocusEnd(CameraMaster master)
    {

    }

    public float GetCameraOrthographicScale()
    {
        return scale;
    }
}