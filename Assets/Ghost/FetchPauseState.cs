using System;
using UnityEngine;

// Hayaletin olduğu yerde kısa bir an durduğu durum (ürünü alırken).
public class FetchPauseState : IGhostState
{
    private readonly float duration;
    private readonly Action onStart;
    private readonly Action onFinished;
    private float timer;

    public FetchPauseState(float duration, Action onStart, Action onFinished)
    {
        this.duration = duration;
        this.onStart = onStart;
        this.onFinished = onFinished;
    }

    public void Enter(GhostController ghost)
    {
        timer = 0f;
        ghost.Agent.ResetPath();
        ghost.Agent.isStopped = true;
        onStart?.Invoke();
    }

    public void Tick(GhostController ghost)
    {
        timer += Time.deltaTime;
        if (timer >= duration)
            onFinished?.Invoke();
    }

    public void Exit(GhostController ghost)
    {
        ghost.Agent.isStopped = false;
    }
}