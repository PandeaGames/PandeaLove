using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorServiceProvider {

    public const string SERVICE_ASSET = "Assets/Client/Editor/Services/ClientEditorServiceManager.asset";

    private static ServiceManager _serviceManager;

    public static ServiceManager ServiceManager
    {
        get
        {
            if (_serviceManager == null)
            {
                _serviceManager = AssetDatabaseUtility.GetAssetAtPath<ServiceManager>(SERVICE_ASSET);
                _serviceManager.StartServices();
            }

            return _serviceManager;
        }
    }
}