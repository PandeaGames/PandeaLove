using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PauseService : Service
{
    private List<Pausable> _pausables;
    private bool _isPaused;

    public override void StartService()
    {
        _pausables = new List<Pausable>();

        base.StartService();
    }

    public override void EndService()
    {
        _pausables.Clear();
        _pausables = null;
        base.EndService();
    }

    public void RegisterPausable(Pausable pausable)
    {
        _pausables.Add(pausable);
    }

    public void Toggle()
    {
        if (_isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        foreach(Pausable pausable in _pausables)
        {
            pausable.Pause();
        }

        _isPaused = true;
    }

    public void Pause(Pausable pausableFocus)
    {
        foreach (Pausable pausable in _pausables)
        {
            if(pausableFocus != pausable)
                pausable.Pause();
        }

        _isPaused = true;
    }

    public void Pause(List<Pausable> pausableFocus)
    {
        foreach (Pausable pausable in _pausables)
        {
            if (!pausableFocus.Contains(pausable))
                pausable.Pause();
        }

        _isPaused = true;
    }

    public void Resume()
    {
        foreach (Pausable pausable in _pausables)
        {
            pausable.Resume();
        }

        _isPaused = false;
    }
}
