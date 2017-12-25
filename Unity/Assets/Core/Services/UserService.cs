using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[Serializable]
public abstract class UserService<T> : Service where T:User
{
    private const string USER_DATA_KEY = "user_data";
    private const string USER_DATA_BACKUP_KEY = "user_data_backup";

    [SerializeField]
    private T _user;

    public T User { get { return _user; } }

    public override void StartService()
    {
        base.StartService();
        _user = Load();
    }

    private T Load()
    {
        T user = ScriptableObject.CreateInstance<T>();
        user.UID = SystemInfo.deviceUniqueIdentifier;

        if (!PlayerPrefs.HasKey(USER_DATA_KEY))
        {
            Debug.Log("No user data found. Creating new with UID "+ SystemInfo.deviceUniqueIdentifier);
        }
        else
        {
            try
            {
                JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(USER_DATA_KEY), user);
                Debug.Log("User Loaded: "+user.UID);
            }
            catch(Exception e)
            {
                Debug.LogError("Failed to load user. Creating new user: "+e);
                PlayerPrefs.SetString(USER_DATA_BACKUP_KEY, PlayerPrefs.GetString(USER_DATA_KEY));
            }
        }

        return user;
    }

    public void Save()
    {
        try
        {
            string data = JsonUtility.ToJson(_user);
            PlayerPrefs.SetString(USER_DATA_KEY, data);
            Debug.Log("User Data saved: " + _user.UID);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save user: " + e);
        }
    }

    public void ClearUserData()
    {
        PlayerPrefs.SetString(USER_DATA_KEY, "");
        Debug.Log("User Data cleared.");
    }
}