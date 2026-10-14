using UnityEngine;
using UnityEngine.SceneManagement;

public class SahneGecisi : MonoBehaviour
{
    [SerializeField] private string hedefSahne = "Sahne2";

    private bool gecti;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (gecti || !other.CompareTag("Player")) return;
        gecti = true;
        SceneManager.LoadScene(hedefSahne);
    }
}