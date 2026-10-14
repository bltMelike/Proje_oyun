using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Sahne2Olay : MonoBehaviour
{
    [SerializeField] private OyuncuKontrol oyuncu;
    [SerializeField] private ArabaKirma araba;
    [SerializeField] private GameObject unlem;
    [SerializeField] private UnityEvent olayBitti;

    private bool basladi;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (basladi || !other.CompareTag("Player")) return;
        basladi = true;
        StartCoroutine(Akis());
    }

    private IEnumerator Akis()
    {
        oyuncu.KontrolAktif(false);
        oyuncu.SagaBak(true);

        yield return araba.Gel();

        unlem.SetActive(true);
        yield return Sars(0.6f);
        yield return new WaitForSeconds(0.8f);
        unlem.SetActive(false);

        oyuncu.KontrolAktif(true);
        olayBitti.Invoke();
    }

    private IEnumerator Sars(float sure)
    {
        Vector3 baslangic = oyuncu.transform.position;
        float gecen = 0f;
        while (gecen < sure)
        {
            oyuncu.transform.position = baslangic + (Vector3)(Random.insideUnitCircle * 0.05f);
            gecen += Time.deltaTime;
            yield return null;
        }
        oyuncu.transform.position = baslangic;
    }
}