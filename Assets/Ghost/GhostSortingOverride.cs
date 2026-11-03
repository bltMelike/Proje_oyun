using UnityEngine;

// Bu sahnedeyken hayaletin katman sırasını yükseltir, hiçbir şeyin arkasında kalmaz.
// Sahne kapanınca hayaletin eski ayarını geri verir.
public class GhostSortingOverride : MonoBehaviour
{
    [SerializeField] private int sortingOrder = 100;
    [SerializeField] private SpriteRenderer matchLayerOf;   // Sorting Layer'ı bunun gibi yap (örn. yumurta)

    private SpriteRenderer ghostRenderer;
    private int originalOrder;
    private int originalLayerId;

    private void Start()
    {
        GhostController ghost = GhostController.Instance;
        if (ghost == null) return;

        ghostRenderer = ghost.GetComponentInChildren<SpriteRenderer>();
        if (ghostRenderer == null) return;

        originalOrder = ghostRenderer.sortingOrder;
        originalLayerId = ghostRenderer.sortingLayerID;

        if (matchLayerOf != null)
            ghostRenderer.sortingLayerID = matchLayerOf.sortingLayerID;
        ghostRenderer.sortingOrder = sortingOrder;
    }

    private void OnDestroy()
    {
        if (ghostRenderer == null) return;
        ghostRenderer.sortingLayerID = originalLayerId;
        ghostRenderer.sortingOrder = originalOrder;
    }
}