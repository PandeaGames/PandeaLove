using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CoreController : MonoBehaviour {

    [SerializeField]
    private ServiceManager _serviceManager;

    public ServiceManager Services { get { return _serviceManager; } }

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        _serviceManager.StartServices();
    }

    // Use this for initialization
    void OnDestroy()
    {
        _serviceManager.EndServices();
    }
}
