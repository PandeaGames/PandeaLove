using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CraftOperator : MonoBehaviour
{
    [SerializeField]
    private ServiceManager _serviceManager;

    [SerializeField]
    private List<MonoBehaviour> _disableOnEnter;

    [SerializeField]
    private List<MonoBehaviour> _enableOnEnter;

    [SerializeField]
    private List<MonoBehaviour> _disableOnExit;

    [SerializeField]
    private List<MonoBehaviour> _enableOnExit;

    [SerializeField]
    private List<GameObject> _disableObjectsOnEnter;

    [SerializeField]
    private List<GameObject> _enableObjectsOnEnter;

    [SerializeField]
    private List<GameObject> _disableObjectsOnExit;

    [SerializeField]
    private List<GameObject> _enableObjectsOnExit;

    [SerializeField]
    private InputGroup _inputGroup;

    [SerializeField]
    private Rigidbody2D _rigidBody;

    [SerializeField]
    private InputMaster _returnControl;

    [SerializeField]
    private CameraAgent _focusCamera;

    private CameraService _cameraService;
    private bool _isKinematic;

    // Use this for initialization
    void Start()
    {
        _cameraService = _serviceManager.GetService<CameraService>();
    }

    public void OnCraftEntered(Craft craft)
    {
        foreach (MonoBehaviour script in _disableOnEnter)
            script.enabled = false;
        foreach (MonoBehaviour script in _enableOnEnter)
            script.enabled = true;

        foreach (GameObject go in _disableObjectsOnEnter)
            go.SetActive(false);
        foreach (GameObject go in _enableObjectsOnEnter)
            go.SetActive(true);

        if (_rigidBody)
        {
            _isKinematic = _rigidBody.isKinematic;
            _rigidBody.isKinematic = false;
        } 
    }

    public void OnCraftExited(Craft craft)
    {
        foreach (MonoBehaviour script in _disableOnExit)
            script.enabled = false;
        foreach (MonoBehaviour script in _enableOnExit)
            script.enabled = true;

        foreach (GameObject go in _disableObjectsOnExit)
            go.SetActive(false);
        foreach (GameObject go in _enableObjectsOnExit)
            go.SetActive(true);

        if (_rigidBody)
            _rigidBody.isKinematic = _isKinematic;
        if (_returnControl)
            _returnControl.FocusOn();
        if (_focusCamera && _cameraService)
            _cameraService.Focus(_focusCamera);
    }

    public void OnCollisionEnter2D(Collision2D coll)
    {
        Craft craft = coll.gameObject.GetComponent<Craft>();

        if (craft != null)
            craft.AttemptEnterCraft(this);
    }
}
