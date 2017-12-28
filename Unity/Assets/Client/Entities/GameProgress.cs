using UnityEngine;
using System;

[Serializable]
public class GameProgress
{
    [SerializeField]
    private string _sector;

    public string Sector
    {
        get
        {
            return _sector;
        }

        set
        {
            _sector = value;
        }
    }

    public GameProgress()
    {

    }
}