using UnityEngine;

// Oyuncu bu nesneye tıklayınca hayalete "git ve bunu korkut" emri verir.
[RequireComponent(typeof(Collider2D))]
public class GhostCommandTarget : MonoBehaviour
{
    [SerializeField] private Transform approachPoint; // hayaletin duracağı nokta
    private bool used;

    private void OnMouseDown()
    {
        if (used) return;
        var ghost = GhostController.Instance;
        var fearable = GetComponent<IFearable>();
        if (ghost == null || fearable == null || approachPoint == null) return;

        used = true;
        ghost.CommandMoveTo(approachPoint, transform, fearable.OnScared);
    }
}