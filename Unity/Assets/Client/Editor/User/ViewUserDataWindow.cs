using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ViewUserDataWindow : EditorWindow
{
    public const string SERVICE_ASSET = "Assets/Client/Editor/Services/ClientEditorServiceManager.asset";

    [MenuItem("PandeaGames/User/View User Data")]
    public static void OnLaunchUserDataWindow()
    {
        // Get existing open window or if none, make a new one:
        ViewUserDataWindow window = (ViewUserDataWindow)EditorWindow.GetWindow(typeof(ViewUserDataWindow));
        window.Show();
    }

    [SerializeField]
    private PandeaUser _user;
    [SerializeField]
    private ServiceManager _serviceManager;

    void Initialize()
    {
        _serviceManager = AssetDatabaseUtility.GetAssetAtPath<ServiceManager>(SERVICE_ASSET);
        _serviceManager.StartServices();

        _user = _serviceManager.GetService<PandeaUserService>().User;
    }

    private void OnDestroy()
    {
        _serviceManager.EndServices();
        _serviceManager = null;
    }

    void OnGUI()
    {
        if (_serviceManager == null || _user == null)
            Initialize();

        Editor editor = Editor.CreateEditor(_user);
        editor.OnInspectorGUI();
    }
}
