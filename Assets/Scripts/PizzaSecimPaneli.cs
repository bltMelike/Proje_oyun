using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PizzaSecimPaneli : MonoBehaviour
{
    private GameObject panel;
    private Action<PizzaTuru> secildi;
    private Action kapandi;

    public bool Acik => panel != null && panel.activeSelf;

    public void Kur(PizzaTanimi[] tanimlar, Action<PizzaTuru> secildiCb, Action kapandiCb)
    {
        secildi = secildiCb;
        kapandi = kapandiCb;

        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var canvasObj = new GameObject("PizzaSecimCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObj.transform.SetParent(transform, false);
        var canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 850;
        var scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasObj.transform, false);
        var prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);

        var kutu = new GameObject("Kutu", typeof(RectTransform), typeof(Image));
        kutu.transform.SetParent(panel.transform, false);
        kutu.GetComponent<RectTransform>().sizeDelta = new Vector2(1200, 760);
        kutu.GetComponent<Image>().color = new Color(0.96f, 0.89f, 0.72f);

        var baslik = YaziYap(kutu.transform, "HANGİ PİZZAYI PİŞİRELİM?", 60, FontStyles.Bold);
        var brt = baslik.rectTransform;
        brt.anchorMin = new Vector2(0, 1); brt.anchorMax = new Vector2(1, 1);
        brt.pivot = new Vector2(0.5f, 1);
        brt.anchoredPosition = new Vector2(0, -30); brt.sizeDelta = new Vector2(-60, 90);

        for (int i = 0; i < tanimlar.Length; i++)
        {
            float x = (i - (tanimlar.Length - 1) / 2f) * 430f;
            KartYap(kutu.transform, tanimlar[i], new Vector2(x, -10));
        }

        var vazgec = new GameObject("Vazgec", typeof(RectTransform), typeof(Image), typeof(Button));
        vazgec.transform.SetParent(kutu.transform, false);
        var vrt = vazgec.GetComponent<RectTransform>();
        vrt.anchorMin = vrt.anchorMax = new Vector2(0.5f, 0);
        vrt.anchoredPosition = new Vector2(0, 60); vrt.sizeDelta = new Vector2(320, 80);
        vazgec.GetComponent<Image>().color = new Color(0.55f, 0.33f, 0.16f);
        vazgec.GetComponent<Button>().onClick.AddListener(Gizle);
        var vy = YaziYap(vazgec.transform, "VAZGEÇ", 40, FontStyles.Bold);
        vy.color = new Color(1f, 0.93f, 0.78f);
        Doldur(vy.rectTransform);

        panel.SetActive(false);
    }

    private void Update()
    {
        if (Acik && Input.GetKeyDown(KeyCode.Escape)) Gizle();
    }

    public void Goster()
    {
        panel.SetActive(true);
    }

    public void Gizle()
    {
        panel.SetActive(false);
        kapandi?.Invoke();
    }

    private void KartYap(Transform ebeveyn, PizzaTanimi t, Vector2 konum)
    {
        var kart = new GameObject("Kart_" + t.ad, typeof(RectTransform), typeof(Image), typeof(Button));
        kart.transform.SetParent(ebeveyn, false);
        var rt = kart.GetComponent<RectTransform>();
        rt.anchoredPosition = konum; rt.sizeDelta = new Vector2(380, 480);
        kart.GetComponent<Image>().color = new Color(0.85f, 0.7f, 0.45f);

        var tur = t.tur;
        kart.GetComponent<Button>().onClick.AddListener(() =>
        {
            panel.SetActive(false);
            secildi?.Invoke(tur);
        });

        var ikon = new GameObject("Ikon", typeof(RectTransform), typeof(Image));
        ikon.transform.SetParent(kart.transform, false);
        var irt = ikon.GetComponent<RectTransform>();
        irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f);
        irt.anchoredPosition = new Vector2(0, 50); irt.sizeDelta = new Vector2(300, 300);
        var img = ikon.GetComponent<Image>();
        img.sprite = t.pismis != null ? t.pismis : t.ham;
        img.preserveAspect = true;
        img.raycastTarget = false;

        var yazi = YaziYap(kart.transform, t.ad, 42, FontStyles.Bold);
        var yrt = yazi.rectTransform;
        yrt.anchorMin = new Vector2(0, 0); yrt.anchorMax = new Vector2(1, 0);
        yrt.pivot = new Vector2(0.5f, 0);
        yrt.anchoredPosition = new Vector2(0, 25); yrt.sizeDelta = new Vector2(-20, 80);
        yazi.raycastTarget = false;
    }

    private static void Doldur(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private TMP_Text YaziYap(Transform ebeveyn, string metin, float boyut, FontStyles stil)
    {
        var o = new GameObject("Yazi", typeof(RectTransform));
        o.transform.SetParent(ebeveyn, false);
        var t = o.AddComponent<TextMeshProUGUI>();
        t.text = metin; t.fontSize = boyut; t.fontStyle = stil;
        t.color = new Color(0.23f, 0.14f, 0.07f);
        t.alignment = TextAlignmentOptions.Center;
        return t;
    }
}