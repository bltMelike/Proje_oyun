using UnityEngine;

// Oyuncu bu bölgeye girince diyalog panelini açar ve yazı kutusuna odaklanır, çıkınca kapatır.
[RequireComponent(typeof(Collider2D))]
public class DialogueZone : MonoBehaviour
{
    [SerializeField] private GameObject panel;        // DialogPanel
    [SerializeField] private NpcDialogue dialogue;
    [SerializeField] private string playerTag = "Player";

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;
        if (panel != null) panel.SetActive(true);
        if (dialogue != null) _ = dialogue.FocusInputAsync();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;

        // Sahne kapanırken nesneler önce yok edilmiş olabilir, kontrol et.
        if (dialogue != null) dialogue.ReleaseControls();
        if (panel != null) panel.SetActive(false);
    }

    private bool IsPlayer(Collider2D other)
    {
        return other.CompareTag(playerTag)
            || (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(playerTag));
    }
}