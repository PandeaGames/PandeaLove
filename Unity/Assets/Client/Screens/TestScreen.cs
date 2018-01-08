using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScreen : ScreenController {

    [SerializeField]
    private ServiceManager _serviceManager;

    private PandeaUserService userService;
    private PandeaUser _user;

	// Use this for initialization
	public override void Start () {
        userService = _serviceManager.GetService<PandeaUserService>();
        _user = userService.User;
        base.Start();
    }
}
