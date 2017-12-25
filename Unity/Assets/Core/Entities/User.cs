using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class User {

    [SerializeField]
    private string _uid;

    public string UID { get { return _uid; } }

    public User(string uid)
    {
        _uid = uid;
    }
}
