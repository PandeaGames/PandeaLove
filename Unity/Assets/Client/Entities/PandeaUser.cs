using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PandeaUser : User {

    [SerializeField]
    private GameProgress _gameProgress;

    public GameProgress GameProgress { get { return _gameProgress; } }

    public PandeaUser(string uid):base(uid)
    {

    }
}
