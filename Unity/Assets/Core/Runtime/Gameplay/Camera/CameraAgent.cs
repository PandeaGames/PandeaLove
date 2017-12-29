using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CameraAgent : MonoBehaviour {

    [SerializeField]
    protected CameraService _cameraService;

    [SerializeField]
    private float scale = 1;

    public abstract Vector3 GetCameraPosition();

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