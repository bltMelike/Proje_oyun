using System.Collections.Generic;
using UnityEngine;

// Bir sensörün okuduğu tek ışın sonucu: yön, mesafe, çarpma durumu.
public readonly struct SensorReading
{
    public readonly float AngleDeg;
    public readonly float Distance;
    public readonly bool Hit;

    public SensorReading(float angleDeg, float distance, bool hit)
    {
        AngleDeg = angleDeg;
        Distance = distance;
        Hit = hit;
    }
}

// Engel algılayan her sensörün uyduğu sözleşme.
public interface IObstacleSensor
{
    IReadOnlyList<SensorReading> Scan();
    bool IsLineBlocked(Vector2 from, Vector2 to);
    float NearestObstacleDistance { get; }
}