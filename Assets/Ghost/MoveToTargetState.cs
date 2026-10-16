using System;
using UnityEngine;

// Hayaletin verilen hedefe NavMesh ile yürüdüğü durum.
public class MoveToTargetState : IGhostState
{
    private readonly Transform target;
    private readonly float arriveDistance;
    private readonly Action onArrived;

    public MoveToTargetState(Transform target, float arriveDistance, Action onArrived)
    {
        this.target = target;
        this.arriveDistance = arriveDistance;
        this.onArrived = onArrived;
    }

    public void Enter(GhostController ghost)
    {
        ghost.Agent.isStopped = false;
        ghost.Agent.SetDestination(target.position);
    }
    public void Tick(GhostController ghost)
    {
        var agent = ghost.Agent;
        float distance = Vector2.Distance(ghost.transform.position, target.position);

        // Ya hedefe yeterince yaklaştı ya da ajan yolun sonuna geldi.
        bool pathFinished = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f;
        if (distance <= arriveDistance || pathFinished)
            onArrived?.Invoke();
    }

    public void Exit(GhostController ghost)
    {
        ghost.Agent.ResetPath();
    }
}