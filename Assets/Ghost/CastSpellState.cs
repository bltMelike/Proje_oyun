using System;
using UnityEngine;

// Hayaletin hedefin etrafında dönerek sihir yaptığı durum.
// Dönerken hedefin önünden/arkasından geçiyormuş gibi katman sırası değişir.
// onCast dönmenin başında çağrılır (hedef hayalet dönerken tepki verebilsin).
public class CastSpellState : IGhostState
{
    private readonly float duration;
    private readonly Transform center;
    private readonly float radius;
    private readonly float degreesPerSecond;
    private readonly Action onCast;
    private readonly Action onFinished;

    private float timer;
    private float angle;
    private Vector3 startPos;

    private SpriteRenderer ghostRenderer;
    private SpriteRenderer targetRenderer;
    private int originalOrder;

    public CastSpellState(float duration, Transform center, Action onCast, Action onFinished,
                          float radius = 1.5f, float degreesPerSecond = 240f)
    {
        this.duration = duration;
        this.center = center;
        this.onCast = onCast;
        this.onFinished = onFinished;
        this.radius = radius;
        this.degreesPerSecond = degreesPerSecond;
    }

    public void Enter(GhostController ghost)
    {
        timer = 0f;
        startPos = ghost.transform.position;
        ghost.Agent.ResetPath();
        ghost.Agent.isStopped = true;
        ghost.Agent.updatePosition = false;

        // Katman değiştirmek için hayaletin ve hedefin sprite'larını bul.
        ghostRenderer = ghost.GetComponentInChildren<SpriteRenderer>();
        targetRenderer = center.GetComponentInChildren<SpriteRenderer>();
        if (ghostRenderer != null) originalOrder = ghostRenderer.sortingOrder;

        Vector2 offset = startPos - center.position;
        angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;

        onCast?.Invoke();   // sihir başladı: hedef tepki vermeye başlasın
    }

    public void Tick(GhostController ghost)
    {
        timer += Time.deltaTime;
        angle += degreesPerSecond * Time.deltaTime;

        float rad = angle * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        Vector3 pos = center.position + new Vector3(Mathf.Cos(rad), sin, 0f) * radius;
        pos.z = ghost.transform.position.z;
        ghost.transform.position = pos;

        // Üst yarıda (sin > 0) hedefin arkasında, alt yarıda önünde çiz.
        if (ghostRenderer != null && targetRenderer != null)
        {
            int targetOrder = targetRenderer.sortingOrder;
            ghostRenderer.sortingOrder = sin > 0f ? targetOrder - 1 : targetOrder + 1;
        }

        if (timer >= duration)
            onFinished?.Invoke();
    }

    public void Exit(GhostController ghost)
    {
        // Katman sırasını eski haline getir.
        if (ghostRenderer != null) ghostRenderer.sortingOrder = originalOrder;

        ghost.Agent.updatePosition = true;
        ghost.Agent.Warp(startPos);
        ghost.Agent.isStopped = false;
    }
}