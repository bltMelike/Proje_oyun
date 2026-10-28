using UnityEngine;

public class SahneGecisi : MonoBehaviour
{
    [SerializeField] private string hedefSahne;
    [SerializeField, TextArea] private string mesaj;
    [SerializeField] private bool baslangictaAcik = true;

    private bool acik;
    private bool gecti;

    private void Awake()
    {
        acik = baslangictaAcik;
    }

    public void Ac()
    {
        acik = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!acik || gecti || !other.CompareTag("Player")) return;
        gecti = true;

        var kontrol = other.GetComponent<OyuncuKontrol>();
        if (kontrol != null) kontrol.KontrolAktif(false);

        EkranGecisi.Ornek.Gecis(hedefSahne, mesaj);
    }
}