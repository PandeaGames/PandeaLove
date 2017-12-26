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
        PandeaUserService _userService = new PandeaUserService();
        _userService.StartService();

        _user = _userService.User;

        _userService.EndService();
    }

    [SerializeField]
    public PandeaUser _user;

    private void LoadUser()
    {
        PandeaUserService _userService = new PandeaUserService();
        _userService.StartService();

        _user = _userService.User;

        _userService.EndService();
        _userService = null;
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
        PandeaUserService _userService = new PandeaUserService();
        _userService.Save(_user);
        _userService = null;

        EditorUtility.DisplayDialog("Success", "User Succesfuly saved: " + _user.UID, "ok");
    }

    private void OnClearData()
    {
        PandeaUserService _userService = new PandeaUserService();
        _userService.ClearUserData();
        _userService = null;

        EditorUtility.DisplayDialog("Success", "User data cleared.", "ok");
    }
}
