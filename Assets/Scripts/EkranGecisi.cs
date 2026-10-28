using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EkranGecisi : MonoBehaviour
{
    private static EkranGecisi ornek;
    private CanvasGroup siyah;
    private CanvasGroup yaziGrubu;
    private TMP_Text yazi;
    private bool mesgul;

    public static EkranGecisi Ornek
    {
        get
        {
            if (ornek == null)
            {
                var go = new GameObject("EkranGecisi");
                ornek = go.AddComponent<EkranGecisi>();
            }
            return ornek;
        }
    }

    private void Awake()
    {
        if (ornek != null && ornek != this) { Destroy(gameObject); return; }
        ornek = this;
        DontDestroyOnLoad(gameObject);
        ArayuzuKur();
    }

    private void ArayuzuKur()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        siyah = YuzeyOlustur("Siyah", new Color(0, 0, 0, 1));

        var yaziObj = new GameObject("Yazi", typeof(RectTransform));
        yaziObj.transform.SetParent(transform, false);
        var rt = yaziObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        yazi = yaziObj.AddComponent<TextMeshProUGUI>();
        yazi.alignment = TextAlignmentOptions.Center;
        yazi.fontSize = 64;
        yazi.color = Color.white;
        yaziGrubu = yaziObj.AddComponent<CanvasGroup>();

        siyah.alpha = 0f; yaziGrubu.alpha = 0f;
        siyah.blocksRaycasts = false;
    }

    private CanvasGroup YuzeyOlustur(string ad, Color renk)
    {
        var obj = new GameObject(ad, typeof(RectTransform));
        obj.transform.SetParent(transform, false);
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        obj.AddComponent<Image>().color = renk;
        return obj.AddComponent<CanvasGroup>();
    }

    public void Gecis(string sahne, string mesaj = "", float yaziSuresi = 2.5f)
    {
        if (mesgul) return;
        StartCoroutine(Akis(sahne, mesaj, yaziSuresi));
    }

    private IEnumerator Akis(string sahne, string mesaj, float yaziSuresi)
    {
        mesgul = true;
        siyah.blocksRaycasts = true;

        yield return Solukluk(siyah, 0f, 1f, 1f);

        if (!string.IsNullOrEmpty(mesaj))
        {
            yazi.text = mesaj;
            yield return Solukluk(yaziGrubu, 0f, 1f, 0.6f);
            yield return new WaitForSecondsRealtime(yaziSuresi);
            yield return Solukluk(yaziGrubu, 1f, 0f, 0.6f);
        }

        var yukleme = SceneManager.LoadSceneAsync(sahne);
        while (yukleme != null && !yukleme.isDone) yield return null;

        yield return Solukluk(siyah, 1f, 0f, 1f);
        siyah.blocksRaycasts = false;
        mesgul = false;
    }

    private IEnumerator Solukluk(CanvasGroup grup, float baslangic, float bitis, float sure)
    {
        float gecen = 0f;
        while (gecen < sure)
        {
            gecen += Time.unscaledDeltaTime;
            grup.alpha = Mathf.Lerp(baslangic, bitis, gecen / sure);
            yield return null;
        }
        grup.alpha = bitis;
    }
}