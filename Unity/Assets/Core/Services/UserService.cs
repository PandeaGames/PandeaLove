using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public abstract class UserService<T> : Service where T:User
{
    private const string USER_DATA_KEY = "user_data";
    private const string USER_DATA_BACKUP_KEY = "user_data_backup";

    private T _user;

    public override void StartService()
    {
        base.StartService();
        _user = Load();
    }

    private T Load()
    {
        T user = null;

        if (!PlayerPrefs.HasKey(USER_DATA_KEY))
        {
            Debug.Log("No user data found. Creating new with UID "+ SystemInfo.deviceUniqueIdentifier);
            user = GenericFactoryUtility<T>.Create(new object[] { SystemInfo.deviceUniqueIdentifier });
        }
        else
        {
            try
            {
                user = JsonUtility.FromJson<T>(PlayerPrefs.GetString(USER_DATA_KEY));
                Debug.Log("User Loaded: "+user.UID);
            }
            catch(Exception e)
            {
                Debug.LogError("Failed to load user. Creating new user: "+e);
                PlayerPrefs.SetString(USER_DATA_BACKUP_KEY, PlayerPrefs.GetString(USER_DATA_KEY));
                user = GenericFactoryUtility<T>.Create(new object[] { SystemInfo.deviceUniqueIdentifier });
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