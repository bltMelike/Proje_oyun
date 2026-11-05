using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RifkiAmca : MonoBehaviour
{
    [Header("Hareket")]
    [SerializeField] private float hiz = 2.2f;
    [SerializeField] private float kareHizi = 8f;

    [Header("Mutlu set (4'er kare)")]
    [SerializeField] private Sprite[] mutluOn;
    [SerializeField] private Sprite[] mutluArka;
    [SerializeField] private Sprite[] mutluYan;

    [Header("Üzgün set (4'er kare)")]
    [SerializeField] private Sprite[] uzgunOn;
    [SerializeField] private Sprite[] uzgunArka;
    [SerializeField] private Sprite[] uzgunYan;

    [Header("Sahne")]
    [SerializeField] private Transform girisNoktasi;
    [SerializeField] private Transform durakNoktasi;
    [SerializeField] private bool yanKarelerSagaBakar = true;
    [SerializeField] private PufEfekti puf;

    [Header("Puf efekti (senin kısmın)")]
    [SerializeField] private float pufSuresi = 1.5f;

    public event Action Ulasti;
    public bool Mutlu => mutlu;
    private SpriteRenderer sr;
    private WaiterFlow dinlenenAkis;
    private bool mutlu;
    private bool basladi;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;   // obje açık kalır, sadece görüntü gizlenir
    }
    
    // FinalBaglayici çağırır: teslim bitince final akışı başlar.
    public void Dinle(WaiterFlow akis)
    {
        dinlenenAkis = akis;
        akis.ServiceFinished += TeslimBitti;
    }

    private void OnDestroy()
    {
        if (dinlenenAkis != null) dinlenenAkis.ServiceFinished -= TeslimBitti;
    }

    private void TeslimBitti()
    {
        StartCoroutine(FinalAkisi(dinlenenAkis.ServedCorrect));
    }

    // Test için: doğrudan başlatmak istersen.
    public void Basla(bool siparisBasarili)
    {
        StartCoroutine(FinalAkisi(siparisBasarili));
    }

    private IEnumerator FinalAkisi(bool siparisBasarili)
    {
        if (basladi) yield break;
        basladi = true;
        Debug.Log("FinalAkisi başladı, başarılı mı: " + siparisBasarili);

        // Hayalet puf efekti burada olacak (senin kısmın).
        if (puf != null) yield return puf.Oynat();
        else yield return new WaitForSeconds(pufSuresi);

        mutlu = siparisBasarili;
        transform.position = girisNoktasi.position;
        sr.enabled = true;
        yield return Yuru();
    }

    private IEnumerator Yuru()
    {
        Vector3 hedef = durakNoktasi.position;
        while (Vector2.Distance(transform.position, hedef) > 0.05f)
        {
            KareSec(hedef - transform.position);
            transform.position = Vector2.MoveTowards(transform.position, hedef, hiz * Time.deltaTime);
            yield return null;
        }

        transform.position = hedef;
        var duruyor = mutlu ? mutluOn : uzgunOn;
        if (duruyor != null && duruyor.Length > 0) sr.sprite = duruyor[0];
        sr.flipX = false;
        Ulasti?.Invoke();
    }

    private void KareSec(Vector2 yon)
    {
        Sprite[] set;
        if (Mathf.Abs(yon.x) >= Mathf.Abs(yon.y))
        {
            set = mutlu ? mutluYan : uzgunYan;
            sr.flipX = (yon.x > 0f) != yanKarelerSagaBakar;
        }
        else
        {
            set = yon.y > 0f ? (mutlu ? mutluArka : uzgunArka) : (mutlu ? mutluOn : uzgunOn);
            sr.flipX = false;
        }

        if (set == null || set.Length == 0) return;
        sr.sprite = set[(int)(Time.time * kareHizi) % set.Length];
    }
}
