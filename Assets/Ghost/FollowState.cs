using UnityEngine;

// Hayaletin oyuncuyu takip ettiği durum. IGhostState sözleşmesine uyar.
public class FollowState : IGhostState
{
    // Duruma girince ajanın hareket etmesine izin ver.
    public void Enter(GhostController ghost)
    {
        ghost.Agent.isStopped = false;
    }

    // Her karede oyuncuya olan mesafeye bak.
    public void Tick(GhostController ghost)
    {
        float distance = Vector2.Distance(ghost.transform.position, ghost.Player.position);

        // Oyuncu uzaktaysa ona doğru yol bul (NavMesh yolu hesaplar).
        if (distance > ghost.FollowDistance)
            ghost.Agent.SetDestination(ghost.Player.position);
        // Yeterince yakınsa dur, oyuncunun üstüne çıkma.
        else
            ghost.Agent.ResetPath();
    }

    // Durumdan çıkarken eski yolu temizle.
    public void Exit(GhostController ghost)
    {
        ghost.Agent.ResetPath();
    }
}