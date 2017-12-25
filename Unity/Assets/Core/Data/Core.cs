using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Core", menuName = "Data/Core", order = 1)]
public class Core : ScriptableObject
{
    [SerializeField]
    private ServiceManager _serviceManager;

    public ServiceManager ServiceManager { get { return _serviceManager; } }
}