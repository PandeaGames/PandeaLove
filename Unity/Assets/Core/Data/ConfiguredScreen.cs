using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class ConfiguredScreen : ScriptableObject
{
    [SerializeField]
    private string _sceneId;

    [SerializeField]
    private ScreenController.Config _screenConfig;

    public string SceneId { get { return _sceneId; } }
    public ScreenController.Config ScreenConfig { get { return _screenConfig; } }

    public ConfiguredScreen(string sceneId, ScreenController.Config screenConfig)
    {
        _sceneId = sceneId;
        _screenConfig = screenConfig;
    }
}
