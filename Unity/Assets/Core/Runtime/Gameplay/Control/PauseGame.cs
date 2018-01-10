using UnityEngine;
using System.Collections;

public class PauseGame : MonoBehaviour
{
    [SerializeField]
    private ServiceManager _serviceManager;

    private PauseService _pauseService;

    // Use this for initialization
    void Start()
    {
        _pauseService = _serviceManager.GetService<PauseService>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            _pauseService.Toggle();
    }
}
