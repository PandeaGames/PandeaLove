using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CameraAgent : MonoBehaviour {

    [SerializeField]
    protected CameraService _cameraService;
    public abstract Vector3 GetCameraPosition();
    public abstract Quaternion GetCameraRotation();

    public virtual void FocusStart(CameraMaster master)
    {

    }

    public virtual void FocusEnd(CameraMaster master)
    {

    }
}