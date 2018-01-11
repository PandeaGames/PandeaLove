using UnityEngine;
using System.Collections;

public class FocusSwimmerOnTouch : MonoBehaviour
{
    [SerializeField]
    private ServiceManager _serviceManager;

    private SwimmerFocusService _swimmerFocusService;

    public void Start()
    {
        _swimmerFocusService = _serviceManager.GetService<SwimmerFocusService>();
    }

    public void OnMouseUp()
    {
        _swimmerFocusService.Focus(gameObject);
    }
}
