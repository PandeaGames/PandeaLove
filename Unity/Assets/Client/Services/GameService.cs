using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "GameService", menuName = "Services/Game", order = 0)]
public class GameService : Service
{
    private List<SECTR_Sector> _sectors;

    public IEnumerator<SECTR_Sector> SectorsEnumerator { get { return _sectors.GetEnumerator(); } }

    public GameService() : base()
    {

    }

    public override void StartService()
    {
        base.StartService();
        _sectors = new List<SECTR_Sector>(FindObjectsOfType<SECTR_Sector>());
    }
}