using System.Collections.Generic;
using UnityEngine;

public class PizzaTezgahi : MonoBehaviour
{
    [SerializeField] private Pizza pizzaPrefab;
    [SerializeField] private Firin firin;
    [SerializeField] private Transform cikisNoktasi; // HamNoktasi
    [SerializeField] private int enFazla = 3;
    [SerializeField] private PizzaTanimi[] pizzalar;

    private readonly List<Pizza> uretilenler = new List<Pizza>();
    private PizzaSecimPaneli panel;
    private OyuncuKontrol oyuncu;
    private bool oyuncuYakin;

    private void Start()
    {
        panel = gameObject.AddComponent<PizzaSecimPaneli>();
        panel.Kur(pizzalar, Uret, OyuncuyuSerbestBirak);
    }

    private void Update()
    {
        if (OyunKilidi.Aktif) return;
        if (oyuncuYakin && !panel.Acik && Input.GetKeyDown(KeyCode.E))
        {
            if (oyuncu != null) oyuncu.KontrolAktif(false);
            panel.Goster();
        }
    }

    private void Uret(PizzaTuru tur)
    {
        OyuncuyuSerbestBirak();

        uretilenler.RemoveAll(p => p == null || p.Durum == PizzaDurumu.Alindi);
        if (uretilenler.Count >= enFazla) return;

        PizzaTanimi tanim = System.Array.Find(pizzalar, p => p.tur == tur);
        if (tanim == null) return;

        Vector3 konum = cikisNoktasi.position + Vector3.right * uretilenler.Count * 0.8f;
        var pizza = Instantiate(pizzaPrefab, konum, Quaternion.identity);
        pizza.Hazirla(tur, tanim.ham, tanim.pismis, firin);
        uretilenler.Add(pizza);
    }

    private void OyuncuyuSerbestBirak()
    {
        if (oyuncu != null) oyuncu.KontrolAktif(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        oyuncu = other.GetComponent<OyuncuKontrol>();
        oyuncuYakin = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) oyuncuYakin = false;
    }
}