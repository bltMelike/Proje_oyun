using System.Collections.Generic;
using UnityEngine;

// Hayaletin etrafına Raycast ışınları atıp engelleri algılar.
public class RaycastSensor : MonoBehaviour, IObstacleSensor
{
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private int rayCount = 8;
    [SerializeField] private float range = 3f;
    [SerializeField] private bool drawGizmos = true;

    private readonly List<SensorReading> readings = new List<SensorReading>();

    public float NearestObstacleDistance { get; private set; } = float.MaxValue;

    public IReadOnlyList<SensorReading> Scan()
    {
        readings.Clear();
        NearestObstacleDistance = float.MaxValue;

        float step = 360f / rayCount;
        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * step;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, range, obstacleMask);

            bool hasHit = hit.collider != null;
            float dist = hasHit ? hit.distance : range;
            readings.Add(new SensorReading(angle, dist, hasHit));

            if (hasHit && dist < NearestObstacleDistance)
                NearestObstacleDistance = dist;
        }
        return readings;
    }

    public bool IsLineBlocked(Vector2 from, Vector2 to)
    {
        return Physics2D.Linecast(from, to, obstacleMask).collider != null;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        float step = 360f / rayCount;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < rayCount; i++)
        {
            float rad = i * step * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            Gizmos.DrawLine(transform.position, transform.position + dir * range);
        }
    }
}