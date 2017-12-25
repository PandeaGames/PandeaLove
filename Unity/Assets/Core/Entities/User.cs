using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class User:ScriptableObject {

    [SerializeField]
    private string _uid;

    public string UID { get { return _uid; } set { _uid = value; } }

    public User():base()
    {

    }

    public User(string uid)
    {
        _uid = uid;
    }
}
