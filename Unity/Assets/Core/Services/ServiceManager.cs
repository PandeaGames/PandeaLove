using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ServiceManager", menuName = "Services/Manager", order =0)]
public class ServiceManager : ScriptableObject {

    [SerializeField]
    private List<Service> _services;

    public delegate void serviceStatusDelegate();
    public event serviceStatusDelegate OnServicesStart;
    public event serviceStatusDelegate OnServicesEnd;

    private Dictionary<Type, Service> _serviceLookup = new Dictionary<Type, Service>();
    private bool _isRunning;

    public bool IsRunning { get { return _isRunning; } }

	public ServiceManager()
    {

    }

    public void StartServices()
    {
        //If we are calling start 2 times, fail out
        if (_isRunning)
        {
            Debug.LogWarning("Tried to Start ServiceManager 2 times.");
            return;
        }

        foreach(Service service in _services)
        {
            Debug.Log("Service starting: " + service.GetType());
            service.StartService();
            _serviceLookup.Add(service.GetType(), service);
        }

        Debug.Log("ServiceManager started with "+_services.Count +" services started.");
        _isRunning = true;

        if (OnServicesStart != null)
            OnServicesStart();
    }

    public void EndServices()
    {
        _serviceLookup.Clear();

        foreach (Service service in _services)
        {
            Debug.Log("Service ended: " + service.GetType());
            service.EndService();
        }

        Debug.Log("ServiceManager ended with " + _services.Count + " services ended.");
        _isRunning = false;

        if (OnServicesEnd != null)
            OnServicesEnd();
    }

    public T GetService<T>() where T:Service
    {
        Service service = null;
        _serviceLookup.TryGetValue(typeof(T), out service);

        if (!service)
            Debug.LogError("Requested service was not found: "+ typeof(T));

        return (T)service;
    }
}
