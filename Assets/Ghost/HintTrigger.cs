using System.Collections;
using UnityEngine;

// Sahnede bir ipucu gösterir: sahne açılınca ya da oyuncu bu alana girince.
public class HintTrigger : MonoBehaviour
{
    [SerializeField, TextArea(2, 5)] private string message = "";
    [SerializeField] private float delay = 0.5f;          // göstermeden önce bekleme (sn)
    [SerializeField] private float holdSeconds = 6f;      // yazı bittikten sonra ekranda kalma süresi
    [SerializeField] private float typeSpeed = 30f;       // saniyede kaç harf
    [SerializeField] private bool showOnStart;            // sahne açılınca göster
    [SerializeField] private bool showOnce = true;        // sadece bir kere göster
    [SerializeField] private string playerTag = "Player";

    private bool fired;

    private void Start()
    {
        if (showOnStart) Fire();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other)) Fire();
    }

    // Başka script'ler de çağırabilir (örn. araba kaçınca).
    public void Fire()
    {
        if (showOnce && fired) return;
        fired = true;
        StartCoroutine(ShowAfterDelay());
    }

    private IEnumerator ShowAfterDelay()
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        HintBar.Instance.Show(message, holdSeconds, typeSpeed);
    }

    private bool IsPlayer(Collider2D c)
    {
        if (c.CompareTag(playerTag)) return true;
        return c.attachedRigidbody != null && c.attachedRigidbody.CompareTag(playerTag);
    }
}