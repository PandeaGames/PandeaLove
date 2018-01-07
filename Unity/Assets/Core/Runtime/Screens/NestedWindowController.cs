using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class NestedWindowController : WindowController
{
    private List<ConfiguredScreen> _stack;

    public override void LaunchScreen(string sceneId, ScreenController.Config screenConfig)
    {
        _stack.Add(new ConfiguredScreen(sceneId, screenConfig));
        base.LaunchScreen(sceneId, screenConfig);
    }

    public void Back()
    {
        //there is no stack, so no navigation can happen
        if (_stack.Count == 0)
            return;

        //we are currently viewing the last screen. Clear the window completely.
        if(_stack.Count == 1)
        {
            RemoveScreen();
            _stack.Clear();
            return;
        }

        _stack.RemoveAt(_stack.Count - 1);
        ConfiguredScreen screen = _stack[_stack.Count - 1];

        LaunchScreen(new ScreenTransition(screen.SceneId, screen.ScreenConfig, Direction.FROM));
    }
}
