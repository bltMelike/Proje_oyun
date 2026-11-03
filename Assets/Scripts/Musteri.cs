using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Musteri : MonoBehaviour
{
    [Header("Hareket")]
    [SerializeField] private float hiz = 2.5f;
    [SerializeField] private float gelmeGecikmesi = 3f;
    [SerializeField] private float kareHizi = 8f;

    [Header("Yürüme kareleri (4'er adet)")]
    [SerializeField] private Sprite[] onKareler;
    [SerializeField] private Sprite[] arkaKareler;
    [SerializeField] private Sprite[] yanKareler;

    [Header("Diğer görseller")]
    [SerializeField] private Sprite beklemeSprite;
    [SerializeField] private Sprite oturmaSprite;

    [Header("Sprite hangi yöne bakıyor?")]
    [SerializeField] private bool yanKarelerSagaBakar = true;
    [SerializeField] private bool oturmaSpriteSagaBakar = true;

    [Header("Sahne")]
    [SerializeField] private Transform girisNoktasi;
    [SerializeField] private OturmaNoktasi[] oturmaNoktalari;

    private SpriteRenderer sr;
    private OturmaNoktasi benimNoktam;

    public bool Oturdu { get; private set; }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        transform.position = girisNoktasi.position;
        if (beklemeSprite != null) sr.sprite = beklemeSprite;
        StartCoroutine(Gel());
    }

    private IEnumerator Gel()
    {
        yield return new WaitForSeconds(gelmeGecikmesi);

        benimNoktam = BosNoktaSec();
        if (benimNoktam == null)
        {
            Debug.LogWarning("Müşteri: boş oturma noktası yok.");
            yield break;
        }
        benimNoktam.Isgal(true);

        Vector3 hedef = benimNoktam.transform.position;
        while (Vector2.Distance(transform.position, hedef) > 0.05f)
        {
            Vector2 yon = hedef - transform.position;
            YurumeKaresiniSec(yon);
            transform.position = Vector2.MoveTowards(transform.position, hedef, hiz * Time.deltaTime);
            yield return null;
        }

        transform.position = hedef;
        Otur();
    }

    private void YurumeKaresiniSec(Vector2 yon)
    {
        Sprite[] set;
        if (Mathf.Abs(yon.x) >= Mathf.Abs(yon.y))
        {
            set = yanKareler;
            sr.flipX = (yon.x > 0f) != yanKarelerSagaBakar;
        }
        else
        {
            set = yon.y > 0f ? arkaKareler : onKareler;
            sr.flipX = false;
        }

        if (set == null || set.Length == 0) return;
        int i = (int)(Time.time * kareHizi) % set.Length;
        sr.sprite = set[i];
    }

    private void Otur()
    {
        if (oturmaSprite != null) sr.sprite = oturmaSprite;
        sr.flipX = benimNoktam.SagaBakar != oturmaSpriteSagaBakar;
        Oturdu = true;
    }

    private OturmaNoktasi BosNoktaSec()
    {
        var bos = new List<OturmaNoktasi>();
        foreach (var n in oturmaNoktalari)
            if (n != null && !n.Dolu) bos.Add(n);

        return bos.Count == 0 ? null : bos[Random.Range(0, bos.Count)];
    }
}