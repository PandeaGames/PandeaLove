﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CameraService", menuName = "Services/Camera", order = 0)]
public class CameraService : Service
{
    public delegate void cameraMasterChange(CameraMaster master);
    public delegate void cameraAgentChange(CameraAgent agent);
    public event cameraMasterChange OnMasterChange;
    public event cameraAgentChange OnAgentChange;

    private CameraMaster _activeMaster;
    private CameraAgent _activeAgent;
    private List<CameraMaster> _masters = new List<CameraMaster>();
    private List<CameraAgent> _agents = new List<CameraAgent>();

    public CameraService() : base()
    {

    }

    public override void StartService()
    {
        base.StartService();
    }

    public override void EndService()
    {
        base.EndService();

        _masters.Clear();
        _agents.Clear();

        _activeAgent = null;
        _activeMaster = null;
    }

    public void RegisterMaster(CameraMaster master)
    {
        if (_activeMaster == null)
            SetMaster(master);

        _masters.Add(master);
    }

    public void UnregisterMaster(CameraMaster master)
    {
        if (!_masters.Contains(master))
        {
            Debug.LogWarning("Trying to remove CameraMaster from CameraService that was not registerd.");
            return;
        }

        if(master == _activeMaster)
        {
            Debug.LogWarning("Unregistering the active CameraMaster.");
            SetMaster(null);
        }

        _masters.Remove(master);
    }

    public void RegisterAgent(CameraAgent agent)
    {
        if (_activeAgent == null)
            SetAgent(agent);

        _agents.Add(agent);
    }

    public void UnregisterAgent(CameraAgent agent)
    {
        if (!_agents.Contains(agent))
        {
            Debug.LogWarning("Trying to remove CameraAgent from CameraService that was not registerd.");
            return;
        }

        if (agent == _activeAgent)
        {
            Debug.LogWarning("Unregistering the active CameraAgent.");
            SetAgent(null);
        }

        _agents.Remove(agent);
    }

    private void SetMaster(CameraMaster master)
    {
        _activeMaster = master;

        if(OnMasterChange != null)
            OnMasterChange(master);
    }
     
    private void SetAgent(CameraAgent agent)
    {
        _activeAgent = agent;

        if (OnAgentChange != null)
            OnAgentChange(agent);
    }
}