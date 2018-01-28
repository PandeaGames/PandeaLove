using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GameService : Service
{
    private List<SECTR_Sector> _sectors;

    public IEnumerator<SECTR_Sector> SectorsEnumerator { get { return _sectors.GetEnumerator(); } }

    private Player _focusedPlayer;

    public Player FocusedPlayer { get { return _focusedPlayer; } }

    public void FocusPlayer(Player player)
    {
        _focusedPlayer = player;
    }

    public override void StartService(ServiceManager serviceManager)
    {
        base.StartService(serviceManager);
        _sectors = new List<SECTR_Sector>(FindObjectsOfType<SECTR_Sector>());
    }
}