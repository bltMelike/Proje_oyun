using System;
using UnityEngine;

// Hayaletin durduğu durum (oyuncuyu takip etmez). Girişte istenirse bir olay tetikler.
public class IdleState : IGhostState
{
    private readonly Action onEnter;

    public IdleState(Action onEnter = null)
    {
        this.onEnter = onEnter;
    }

    public void Enter(GhostController ghost)
    {
        ghost.Agent.ResetPath();
        ghost.Agent.isStopped = true;
        onEnter?.Invoke();
    }

    public void Tick(GhostController ghost) { }

    public void Exit(GhostController ghost)
    {
        ghost.Agent.isStopped = false;
    }
}