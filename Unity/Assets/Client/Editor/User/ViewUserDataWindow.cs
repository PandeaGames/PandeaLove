using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class ViewUserDataWindow : EditorWindow
{
    public const string SERVICE_ASSET = "Assets/Client/Editor/Services/ClientEditorServiceManager.asset";
    public const string SERVICE_ASSET_SERVICE = "Assets/Client/Editor/Services/PandeaUserService.asset";

    [MenuItem("PandeaGames/User/View User Data")]
    public static void OnLaunchUserDataWindow()
    {
        // Get existing open window or if none, make a new one:
        ViewUserDataWindow window = (ViewUserDataWindow)EditorWindow.GetWindow(typeof(ViewUserDataWindow));
        window.InitWindow();
        window.Show();
    }

    public void InitWindow()
    {
        ServiceManager _serviceManager = AssetDatabaseUtility.GetAssetAtPath<ServiceManager>(SERVICE_ASSET);
        _serviceManager.StartServices();

        _user = _serviceManager.GetService<PandeaUserService>().User;

        _serviceManager.EndServices();
    }

    [SerializeField]
    public PandeaUser _user;

    private void LoadUser()
    {
        PandeaUserService service = AssetDatabaseUtility.GetAssetAtPath<PandeaUserService>(SERVICE_ASSET_SERVICE);
        service.StartService();

        _user = service.User;

        service.EndService();
        service = null;
    }

    void OnGUI()
    {
        if (GUILayout.Button("Clear User Data"))
            OnClearData();
        if (GUILayout.Button("Save User Data"))
            OnSaveData();

        if (_user == null)
            LoadUser();

        Editor editor = Editor.CreateEditor(_user);
        editor.OnInspectorGUI();
    }

    private void OnSaveData()
    {
        PandeaUserService service = AssetDatabaseUtility.GetAssetAtPath<PandeaUserService>(SERVICE_ASSET_SERVICE);
        service.Save(_user);
        service = null;

        EditorUtility.DisplayDialog("Success", "User Succesfuly saved: " + _user.UID, "ok");
    }

    private void OnClearData()
    {
        PandeaUserService service = AssetDatabaseUtility.GetAssetAtPath<PandeaUserService>(SERVICE_ASSET_SERVICE);
        service.ClearUserData();
        service = null;

        EditorUtility.DisplayDialog("Success", "User data cleared.", "ok");
    }
}
