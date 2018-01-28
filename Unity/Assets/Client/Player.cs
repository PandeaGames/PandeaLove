using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [SerializeField]
    private ServiceManager _serviceManager;

    private GameService _gameService;

    protected void Start()
    {
        _gameService = _serviceManager.GetService<GameService>();
    }
}
