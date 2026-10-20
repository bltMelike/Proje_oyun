using System;
using UnityEngine;

// Hayaletin verilen hedefe NavMesh ile yürüdüğü durum. Sensör verisini kullanır.
public class MoveToTargetState : IGhostState
{
    private const float NormalSpeed = 3f;
    private const float CautiousSpeed = 1.5f;
    private const float CautiousDistance = 0.8f;

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

        // Sensör: hedefle aramızda engel var mı? (doğrudan mı, dolanarak mı gidilecek)
        if (ghost.Sensor != null)
        {
            ghost.LastRouteWasDetour = ghost.Sensor.IsLineBlocked(ghost.transform.position, target.position);
            Debug.Log(ghost.LastRouteWasDetour
                ? "Hayalet: doğrudan yol engelli, dolanıyorum"
                : "Hayalet: yol açık, doğrudan gidiyorum");
        }

        ghost.Agent.SetDestination(target.position);
    }

    public void Tick(GhostController ghost)
    {
        var agent = ghost.Agent;

        // Sensör: önünde engel yakınsa yavaşla.
        if (ghost.Sensor != null)
            agent.speed = ghost.Sensor.NearestObstacleDistance < CautiousDistance ? CautiousSpeed : NormalSpeed;

        float distance = Vector2.Distance(ghost.transform.position, target.position);
        bool pathFinished = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f;

        if (distance <= arriveDistance || pathFinished)
            onArrived?.Invoke();
    }

    public void Exit(GhostController ghost)
    {
        ghost.Agent.speed = NormalSpeed;
        ghost.Agent.ResetPath();
    }
}