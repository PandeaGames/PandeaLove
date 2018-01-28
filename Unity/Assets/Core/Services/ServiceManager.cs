using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ServiceManager : MonoBehaviour {

    [SerializeField]
    private List<Service> _services;

    public delegate void serviceStatusDelegate();
    public event serviceStatusDelegate OnServicesStart;
    public event serviceStatusDelegate OnServicesEnd;

    private Dictionary<Type, Service> _serviceLookup = new Dictionary<Type, Service>();
    private bool _isRunning;

    public bool IsRunning { get { return _isRunning; } }

    public virtual void Awake()
    {
        StartServices();
    }

    private void OnDestroy()
    {
        EndServices();
    }

    public void StartServices()
    {
        Debug.Log("Service Manager starting: " + name);

        //If we are calling start 2 times, fail out
        if (_isRunning)
        {
            Debug.LogWarning("Tried to Start ServiceManager 2 times.");
            return;
        }

        foreach (Service service in _services)
        {
            Debug.Log("Service "+service.name+" starting: Type(" + service.GetType()+")");
            service.StartService(this);
            _serviceLookup.Add(service.GetType(), service);
        }

        Debug.Log("ServiceManager started with "+_services.Count +" services started.");
        _isRunning = true;

        if (OnServicesStart != null)
            OnServicesStart();
    }

    public void EndServices()
    {
        Debug.Log("Service Manager ending: " + name);

        _serviceLookup.Clear();

        foreach (Service service in _services)
        {
            Debug.Log("Service " + service.name + " ending: Type(" + service.GetType() + ")");
            service.EndService(this);
        }

        Debug.Log("ServiceManager ending with " + _services.Count + " services suspended.");
        _isRunning = false;

        if (OnServicesEnd != null)
            OnServicesEnd();
    }

    public bool ContainsService<T>() where T : Service
    {
        Service service = null;
        _serviceLookup.TryGetValue(typeof(T), out service);
        return service != null;
    }

    public T GetService<T>() where T:Service
    {
        Service service = null;
        _serviceLookup.TryGetValue(typeof(T), out service);

        if (!service)
            service = FetchServiceDependancy<T>();

        if (!service)
            Debug.LogError("Requested service was not found: "+ typeof(T));

        return (T)service;
    }
    
    private T FetchServiceDependancy<T>() where T : Service
    {
        T service = null;
        ServiceManager[] serviceManagers = FindObjectsOfType<ServiceManager>();

        foreach(ServiceManager serviceManager in serviceManagers)
        {
            if (serviceManager.ContainsService<T>())
            {
                service = serviceManager.GetService<T>();
                break;
            }
        }

        if (!service)
        {
            service = gameObject.AddComponent<T>();
            service.StartService(this);
            _services.Add(service);
            Debug.Log("Service " + service.name + " starting: Type(" + service.GetType() + ")");
        }

        _serviceLookup.Add(typeof(T), service);

        return service;
    }
}
