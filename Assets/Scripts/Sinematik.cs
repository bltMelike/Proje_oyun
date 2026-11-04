using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Sinematik : MonoBehaviour
{
    // Sırayla: senaryo1, senaryo2, senaryo3, senaryo4, senaryo4_1
    [SerializeField] private Sprite[] gorseller;
    [SerializeField] private string sonrakiSahne = "SampleScene";
    [SerializeField] private float harfHizi = 0.04f;
    [SerializeField] private float gecisSuresi = 1f;
    [SerializeField] private float yaziBoyutu = 64f;

    // Boş bırakılan karede yazı şeridi gizlenir (yazı görselin içinde)
    private readonly string[] yazilar =
    {
        "Bir gün arkadaşımla eve dönerken gözüme bir tabela takıldı; 'MIDIŞ DÖNER'. Aklımdaysa tek bir şey vardı, pizza!",
        "Günler geçerken MIDIŞ DÖNER bizim favori yerimiz olmuştu. Fakat dönerci amca bir gün sessizce aramızdan ayrıldı ama dükkânını bırakıp gidemedi...",
        "Bir akşam, kapalı dükkânın önünde onunla karşılaştım.Biraz korkmuştum ama içimde de tanıdık bir his vardı. Bana dükkanının battığını ve bu hale geldiğini söyledi. Onu kurtarmanın tek yolu ise dükkanı ayağa kaldırmak.",
        "Kararımı verdim: burası artık bir pizzacı olacak. MIDIŞ PİZZA!",
        ""
    };

    private Image onGorsel, arkaGorsel, karartma;
    private TMP_Text yaziAlani, ipucu;
    private GameObject yaziSeridi;
    private bool yaziBitti, tikladi, atla, bitiyor;

    private void Start()
    {
        ArayuzuKur();
        StartCoroutine(Akis());
    }

    private void Update()
    {
        if (bitiyor) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (yaziBitti) tikladi = true; else atla = true;
        }
        if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene(sonrakiSahne);
    }

    private IEnumerator Akis()
    {
        int adet = Mathf.Min(gorseller.Length, yazilar.Length);

        for (int i = 0; i < adet; i++)
        {
            bool yaziVar = !string.IsNullOrEmpty(yazilar[i]);
            yaziSeridi.SetActive(yaziVar);

            yield return GorselGecisi(gorseller[i]);
            if (yaziVar) yield return Yazdir(yazilar[i]);

            yaziBitti = true;
            tikladi = false;
            ipucu.gameObject.SetActive(true);
            while (!tikladi) yield return null;
            ipucu.gameObject.SetActive(false);
            yaziBitti = false;
        }

        // Sonda ekranı karart, sonra sahneyi yükle
        bitiyor = true;
        yaziAlani.text = "";
        yield return Solukluk(karartma, 0f, 1f, 1f);
        SceneManager.LoadScene(sonrakiSahne);
    }

    private IEnumerator GorselGecisi(Sprite yeni)
    {
        arkaGorsel.sprite = onGorsel.sprite;
        arkaGorsel.color = onGorsel.sprite != null ? Color.white : Color.black;
        yaziAlani.text = "";

        onGorsel.sprite = yeni;
        yield return Solukluk(onGorsel, 0f, 1f, gecisSuresi);
    }

    private IEnumerator Yazdir(string metin)
    {
        atla = false;
        yaziAlani.text = metin;
        yaziAlani.maxVisibleCharacters = 0;
        yaziAlani.ForceMeshUpdate();
        int toplam = yaziAlani.textInfo.characterCount;

        for (int i = 1; i <= toplam; i++)
        {
            if (atla) break;
            yaziAlani.maxVisibleCharacters = i;
            yield return new WaitForSeconds(harfHizi);
        }
        yaziAlani.maxVisibleCharacters = toplam;
    }

    private IEnumerator Solukluk(Image img, float bas, float son, float sure)
    {
        float t = 0f;
        while (t < sure)
        {
            t += Time.deltaTime;
            var c = img.color; c.a = Mathf.Lerp(bas, son, t / sure); img.color = c;
            yield return null;
        }
        var s = img.color; s.a = son; img.color = s;
    }

    private void ArayuzuKur()
    {
        var canvasObj = new GameObject("SinematikCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObj.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        TamEkran(canvasObj.transform, "Zemin", Color.black);
        arkaGorsel = TamEkran(canvasObj.transform, "Arka", Color.black);
        arkaGorsel.preserveAspect = true;
        onGorsel = TamEkran(canvasObj.transform, "On", new Color(1, 1, 1, 0));
        onGorsel.preserveAspect = true;

        var serit = new GameObject("Serit", typeof(RectTransform), typeof(Image));
        serit.transform.SetParent(canvasObj.transform, false);
        var srt = serit.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0, 0); srt.anchorMax = new Vector2(1, 0);
        srt.pivot = new Vector2(0.5f, 0); srt.sizeDelta = new Vector2(0, 340);
        serit.GetComponent<Image>().color = new Color(0, 0, 0, 0.65f);
        yaziSeridi = serit;

        yaziAlani = YaziOlustur(serit.transform, yaziBoyutu, new Vector2(1760, 300));
        yaziAlani.alignment = TextAlignmentOptions.Center;
        yaziAlani.color = new Color(1f, 0.96f, 0.88f);
        yaziAlani.outlineWidth = 0.18f;
        yaziAlani.outlineColor = new Color32(20, 10, 5, 255);

        ipucu = YaziOlustur(canvasObj.transform, 36, new Vector2(600, 60));
        var irt = ipucu.rectTransform;
        irt.anchorMin = irt.anchorMax = new Vector2(1, 0);
        irt.pivot = new Vector2(1, 0);
        irt.anchoredPosition = new Vector2(-40, 20);
        ipucu.alignment = TextAlignmentOptions.Right;
        ipucu.text = "Devam için tıkla";
        ipucu.gameObject.SetActive(false);

        // En üstte: sonda ekranı karartmak için
        karartma = TamEkran(canvasObj.transform, "Karartma", new Color(0, 0, 0, 0));
        karartma.raycastTarget = false;
    }

    private Image TamEkran(Transform ebeveyn, string ad, Color renk)
    {
        var o = new GameObject(ad, typeof(RectTransform), typeof(Image));
        o.transform.SetParent(ebeveyn, false);
        var rt = o.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = o.GetComponent<Image>();
        img.color = renk;
        return img;
    }

    private TMP_Text YaziOlustur(Transform ebeveyn, float boyut, Vector2 olcu)
    {
        var o = new GameObject("Yazi", typeof(RectTransform));
        o.transform.SetParent(ebeveyn, false);
        var rt = o.GetComponent<RectTransform>();
        rt.sizeDelta = olcu; rt.anchoredPosition = Vector2.zero;
        var t = o.AddComponent<TextMeshProUGUI>();
        t.fontSize = boyut; t.color = Color.white;
        return t;
    }
}