using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PandeaUser : User {

    [SerializeField]
    private GameProgress _gameProgress = new GameProgress();

    public GameProgress GameProgress { get { return _gameProgress; } }

    public PandeaUser()
    {

    }

    public PandeaUser(string uid):base(uid)
    {

    }
}