using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectorSaveService : Service {

    [SerializeField]
    private GameService _gameService;

    public override void StartService(ServiceManager serviceManager)
    {
        base.StartService(serviceManager);

        IEnumerator<SECTR_Sector> sectors = _gameService.SectorsEnumerator;

        while (sectors.MoveNext())
        {
            SECTR_Sector sector = sectors.Current;
        }
    }

    public override void EndService(ServiceManager serviceManager)
    {
        base.EndService(serviceManager);
    }
}
