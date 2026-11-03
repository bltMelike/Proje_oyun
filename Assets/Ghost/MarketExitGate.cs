using UnityEngine;

// Liste tamamlanana kadar marketten çıkışı kapatır, tamamlanınca açar ve uyarı verir.
public class MarketExitGate : MonoBehaviour
{
    [SerializeField] private ShoppingList shoppingList;
    [SerializeField] private GameObject exitTrigger;      // CikisTetik
    [SerializeField, TextArea(2, 4)]
    private string lockedMessage =
        "Önce listedeki tüm malzemeleri toplamalısın. Ürünlere çift tıkla, hayaletin gidip alsın!";
    [SerializeField, TextArea(2, 4)]
    private string unlockedMessage =
        "Alışveriş tamam! Artık marketten çıkabilirsin: kapıya doğru yürü.";
    [SerializeField] private float holdSeconds = 6f;
    [SerializeField] private string playerTag = "Player";

    private bool unlocked;

    private void Awake()
    {
        if (exitTrigger != null) exitTrigger.SetActive(false);
    }

    private void OnEnable()
    {
        if (shoppingList != null) shoppingList.AllCollected += Unlock;
    }

    private void OnDisable()
    {
        if (shoppingList != null) shoppingList.AllCollected -= Unlock;
    }

    private void Unlock()
    {
        unlocked = true;
        if (exitTrigger != null) exitTrigger.SetActive(true);
        HintBar.Instance.Show(unlockedMessage, holdSeconds);
    }

    // Bu nesnenin kapının önündeki Trigger collider'ına oyuncu girerse uyar.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (unlocked) return;
        bool isPlayer = other.CompareTag(playerTag) ||
                        (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(playerTag));
        if (isPlayer) HintBar.Instance.Show(lockedMessage, holdSeconds);
    }
}