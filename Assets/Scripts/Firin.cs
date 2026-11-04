using System.Collections;
using UnityEngine;

public class Firin : MonoBehaviour
{
    [SerializeField] private Transform girisNoktasi;
    [SerializeField] private Transform pismisNoktasi; // sol tezgahtaki nokta
    [SerializeField] private float pisirmeSuresi = 5f;
    [SerializeField] private float araligi = 0.7f;

    private int pismisSayisi;

    public void Koy(Pizza pizza)
    {
        StartCoroutine(Pisir(pizza));
    }

    private IEnumerator Pisir(Pizza pizza)
    {
        pizza.DurumDegistir(PizzaDurumu.Pisiyor);
        yield return pizza.Tasi(girisNoktasi.position);

        pizza.Gizle(true); // fırının içinde görünmez
        yield return new WaitForSeconds(pisirmeSuresi);

        Vector3 hedef = pismisNoktasi.position + Vector3.right * (pismisSayisi % 3) * araligi;
        pismisSayisi++;
        pizza.transform.position = hedef;
        pizza.DurumDegistir(PizzaDurumu.Hazir);
        pizza.Gizle(false);

    }
}