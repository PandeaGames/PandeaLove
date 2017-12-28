using UnityEngine;

public class Core : ScriptableObject
{
    [SerializeField]
    private ServiceManager _serviceManager;

    public ServiceManager ServiceManager { get { return _serviceManager; } }
}