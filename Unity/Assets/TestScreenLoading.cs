using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScreenLoading : MonoBehaviour {

    [SerializeField]
    private WindowController _windowController;
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.P))
            _windowController.LaunchScreen(new ScreenTransition("TestScreenP", ScriptableObject.CreateInstance<ScreenController.Config>()));
        if (Input.GetKeyDown(KeyCode.O))
            _windowController.LaunchScreen(new ScreenTransition("TestScreenO", ScriptableObject.CreateInstance<ScreenController.Config>()));
        if (Input.GetKeyDown(KeyCode.I))
            _windowController.LaunchScreen(new ScreenTransition("TestScreenI", ScriptableObject.CreateInstance<ScreenController.Config>()));
    }
}
