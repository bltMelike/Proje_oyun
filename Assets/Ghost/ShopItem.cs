using UnityEngine;

// Rafta duran ürün. ÇİFT TIKLANINCA hayalet gidip alır, ürün listeden silinir (üründe değişiklik olmaz).
[RequireComponent(typeof(Collider2D))]
public class ShopItem : MonoBehaviour
{
    [SerializeField] private string itemName = "Un";            // listedeki adla aynı olmalı
    [SerializeField] private ShoppingList shoppingList;
    [SerializeField] private Transform approachPoint;           // boş bırakırsan hayalet doğrudan ürüne gider
    [SerializeField] private float doubleClickWindow = 0.4f;    // iki tık arası en fazla süre (sn)

    private static ShopItem pending;   // aynı anda tek bir emir
    private bool collected;
    private float lastClickTime = -10f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { pending = null; }

    private void OnDisable()
    {
        if (pending == this) pending = null;
    }

    private void OnMouseDown()
    {
        if (collected || pending != null) return;

        float now = Time.unscaledTime;
        bool isDoubleClick = now - lastClickTime <= doubleClickWindow;
        lastClickTime = now;
        if (!isDoubleClick) return;
        lastClickTime = -10f;

        if (shoppingList == null)
        {
            Debug.LogError("ShopItem: Shopping List atanmamış!", this);
            return;
        }
        if (!shoppingList.Contains(itemName)) return;

        GhostController ghost = GhostController.Instance;
        if (ghost == null)
        {
            Debug.LogWarning("ShopItem: Sahnede hayalet bulunamadı.");
            return;
        }

        pending = this;
        Transform target = approachPoint != null ? approachPoint : transform;
        ghost.CommandFetch(target, OnGhostCollected);
    }

    private void OnGhostCollected()
    {
        pending = null;
        collected = true;
        shoppingList.Collect(itemName);
    }
}