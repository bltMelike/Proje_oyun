using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Pizza : MonoBehaviour
{
    public PizzaTuru Tur { get; private set; }
    public PizzaDurumu Durum { get; private set; }

    private SpriteRenderer sr;
    private Collider2D col;
    private Sprite hamSprite, pismisSprite;
    private Firin firin;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    public void Hazirla(PizzaTuru tur, Sprite ham, Sprite pismis, Firin hedefFirin)
    {
        Tur = tur;
        hamSprite = ham;
        pismisSprite = pismis;
        firin = hedefFirin;
        DurumDegistir(PizzaDurumu.Ham);
    }

    public void DurumDegistir(PizzaDurumu yeni)
    {
        Durum = yeni;
        if (hamSprite != null && pismisSprite != null)
            sr.sprite = (yeni == PizzaDurumu.Hazir || yeni == PizzaDurumu.Alindi) ? pismisSprite : hamSprite;
    }

    public void Gizle(bool gizli)
    {
        sr.enabled = !gizli;
        col.enabled = !gizli;
    }

    public IEnumerator Tasi(Vector3 hedef, float hiz = 6f)
    {
        while (Vector3.Distance(transform.position, hedef) > 0.02f)
        {
            transform.position = Vector3.MoveTowards(transform.position, hedef, hiz * Time.deltaTime);
            yield return null;
        }
        transform.position = hedef;
    }

    private void OnMouseDown()
    {
        if (Durum == PizzaDurumu.Ham && firin != null) firin.Koy(this);
        else if (Durum == PizzaDurumu.Hazir) Al();
    }

    private void Al()
    {
        // Pizza tezgahta kalır, sadece "hazır ve seçildi" olarak işaretlenir
        DurumDegistir(PizzaDurumu.Alindi);
    }
}