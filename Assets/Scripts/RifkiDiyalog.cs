using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Final: oyuncu Rıfkı Amca'ya serbest metinle sorar, yerel LLM (Ollama) JSON ile cevap verir,
// cevaptaki "action" alanına göre oyun biter.
public class RifkiDiyalog : MonoBehaviour
{
    [Header("Yapay zekâ")]
    [SerializeField] private string endpoint = "http://localhost:11434/api/chat";
    [SerializeField] private string model = "qwen2.5:3b";
    [SerializeField] private int timeoutSeconds = 60;

    [Header("Akış")]
    [SerializeField] private int enFazlaTur = 4;          // takılma olmasın diye güvenlik sınırı
    [SerializeField] private string menuSahnesi = "AnaMenu";

    // ---- JSON sınıfları ----
    [Serializable] private class Msg { public string role; public string content; }
    [Serializable] private class Opt { public float temperature = 0.4f; public int num_predict = 150; }
    [Serializable]
    private class Req
    {
        public string model;
        public List<Msg> messages;
        public bool stream = false;
        public string format = "json";
        public string keep_alive = "10m";
        public Opt options = new Opt();
    }
    [Serializable] private class Resp { public Msg message; }
    [Serializable] private class Cevap { public string attitude; public string reply; public string action; }

    private class Sonuc
    {
        public bool Basarili;
        public string Reply;
        public string Action;
        public string Hata;
    }

    private readonly List<Msg> gecmis = new List<Msg>();
    private bool siparisBasarili;
    private bool bekliyor;
    private int tur;

    private GameObject diyalogPanel, bitisPanel;
    private TMP_Text isimYazi, cevapYazi, ipucuYazi, bitisYazi;
    private TMP_InputField giris;
    private Button gonderBtn;

    private void OnDestroy()
    {
        OyunKilidi.Aktif = false;
    }

    // RifkiAmca durağa ulaşınca çağrılır.
    public void Baslat(bool teslimBasarili)
    {
        OyunKilidi.Aktif = true;
        siparisBasarili = teslimBasarili;
        gecmis.Clear();
        gecmis.Add(new Msg { role = "system", content = SistemPromptu(teslimBasarili) });

        var oyuncu = FindFirstObjectByType<OyuncuKontrol>();
        if (oyuncu != null) oyuncu.KontrolAktif(false);   // yazarken kız yürümesin

        ArayuzuKur();
        diyalogPanel.SetActive(true);
        cevapYazi.text = "";
        ipucuYazi.text = "Rıfkı Amca'ya restoranı beğenip beğenmediğini sor ve Enter'a bas.";
        giris.ActivateInputField();
    }

    // ---------- Sistem promptu ----------
    private string SistemPromptu(bool basarili)
    {
        string sonuc = basarili
            ? "Müşteri doğru pizzayı aldı ve mutlu ayrıldı."
            : "Müşteri yanlış pizza aldı, kızdı ve kalkıp gitti.";

        string ornek = basarili
            ? "Oyuncu: Restoranı beğendin mi amca?\n" +
              "Sen: {\"attitude\":\"mutlu\",\"reply\":\"Beğenmek mi evladım, gözlerim doldu! Dükkân emin ellerde.\",\"action\":\"end_good\"}\n"
            : "Oyuncu: Restoranı beğendin mi amca?\n" +
              "Sen: {\"attitude\":\"uzgun\",\"reply\":\"Müşteri küstü evladım, biraz daha özen göstermen lazım.\",\"action\":\"end_bad\"}\n";

        return
            "Sen Rıfkı Amca'sın. Yıllarca Midiş Döner'i işlettin, öldükten sonra hayalet oldun ve dükkânı genç bir kıza devrettin. " +
            "Dükkân artık Midiş Pizza. Bu akşam kızın dükkânına geldin. Sıcak, babacan ve samimi konuşursun. Karşındaki oyuncu o kız. " +
            "Oyuncu sana dükkânı beğenip beğenmediğini soracak.\n" +
            "SİPARİŞ SONUCU: " + sonuc + "\n" +
            "Kurallar:\n" +
            "- Oyuncu dükkânı, restoranı veya yaptığı işi sorarsa SİPARİŞ SONUCU'na uygun cevap ver. " +
            (basarili
                ? "Sonuç olumlu, mutlu ve gururlu ol, action \"end_good\" olsun.\n"
                : "Sonuç olumsuz, üzgün ve kırgın ol ama kırıcı olma, action \"end_bad\" olsun.\n") +
            "- Oyuncu alakasız bir şey söylerse kısaca karşılık ver, restoranı sor, action \"continue\" olsun.\n" +
            "- reply: en fazla 2 kısa cümle, akıcı Türkçe, emoji ve İngilizce kelime yok.\n" +
            "Sadece şu JSON'u ver: {\"attitude\":\"mutlu|uzgun|kizgin|alakasiz\",\"reply\":\"...\",\"action\":\"end_good|end_bad|continue\"}\n" +
            "Örnekler:\n" + ornek +
            "Oyuncu: Hava nasıl?\n" +
            "Sen: {\"attitude\":\"alakasiz\",\"reply\":\"Hava güzel evladım ama asıl dükkânı merak ediyorum, anlatsana.\",\"action\":\"continue\"}";
    }

    // ---------- Gönderme ----------
    private void Gonder()
    {
        if (bekliyor || giris == null) return;
        string soru = giris.text.Trim();
        if (string.IsNullOrEmpty(soru)) return;
        _ = KonusAsync(soru);
    }

    private async Awaitable KonusAsync(string soru)
    {
        bekliyor = true;
        giris.text = "";
        giris.interactable = false;
        gonderBtn.interactable = false;
        ipucuYazi.text = "Sen: " + soru;
        cevapYazi.text = "Rıfkı Amca düşünüyor...";   // bekleme geri bildirimi

        gecmis.Add(new Msg { role = "user", content = soru });
        Sonuc s = await SorAsync();
        if (this == null) return;

        if (!s.Basarili)
        {
            // Hata: oyun çökmez, kullanıcıya bilgi verilir, tekrar denenebilir.
            gecmis.RemoveAt(gecmis.Count - 1);
            cevapYazi.text = s.Hata;
            ipucuYazi.text = "Tekrar yazıp Enter'a basabilirsin.";
            SerbestBirak();
            return;
        }

        cevapYazi.text = s.Reply;
        tur++;

        bool bitir = s.Action == "end_good" || s.Action == "end_bad";
        bool basarili = s.Action == "end_good";

        // Güvenlik ağı: model üst üste "continue" derse sohbet sonsuza gitmesin.
        if (!bitir && tur >= enFazlaTur)
        {
            bitir = true;
            basarili = siparisBasarili;
        }

        if (bitir)
        {
            await Awaitable.WaitForSecondsAsync(3f);
            if (this == null) return;
            Bitir(basarili);
            return;
        }

        SerbestBirak();
    }

    private void SerbestBirak()
    {
        bekliyor = false;
        giris.interactable = true;
        gonderBtn.interactable = true;
        giris.ActivateInputField();
    }

    // ---------- Ollama isteği ----------
    private async Awaitable<Sonuc> SorAsync()
    {
        try
        {
            string json = JsonUtility.ToJson(new Req { model = model, messages = gecmis });

            using (var req = new UnityWebRequest(endpoint, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = timeoutSeconds;

                var op = req.SendWebRequest();
                while (!op.isDone) await Awaitable.NextFrameAsync();   // oyun donmaz
                if (this == null) return new Sonuc { Hata = "" };

                if (req.result == UnityWebRequest.Result.ConnectionError)
                    return new Sonuc { Hata = "Yapay zekâ sunucusuna ulaşılamadı. Ollama açık mı?" };
                if (req.result != UnityWebRequest.Result.Success)
                    return new Sonuc { Hata = "Sunucu hatası (" + req.responseCode + "). Model yüklü mü?" };

                var resp = JsonUtility.FromJson<Resp>(req.downloadHandler.text);
                if (resp == null || resp.message == null || string.IsNullOrWhiteSpace(resp.message.content))
                    return new Sonuc { Hata = "Sunucudan boş yanıt geldi." };

                return Ayristir(resp.message.content);
            }
        }
        catch (Exception e)
        {
            return new Sonuc { Hata = "Beklenmeyen hata: " + e.Message };
        }
    }

    // Modelin metninden JSON'u ayıklar. Bozuksa sohbet "continue" gibi devam eder.
    private Sonuc Ayristir(string ham)
    {
        try
        {
            int bas = ham.IndexOf('{');
            int son = ham.LastIndexOf('}');
            if (bas < 0 || son <= bas) throw new Exception("JSON yok");

            var c = JsonUtility.FromJson<Cevap>(ham.Substring(bas, son - bas + 1));
            if (c == null || string.IsNullOrWhiteSpace(c.reply)) throw new Exception("reply yok");

            string aksiyon = (c.action ?? "continue").Trim().ToLowerInvariant();
            if (aksiyon != "end_good" && aksiyon != "end_bad") aksiyon = "continue";

            gecmis.Add(new Msg { role = "assistant", content = ham });
            return new Sonuc { Basarili = true, Reply = c.reply.Trim(), Action = aksiyon };
        }
        catch (Exception)
        {
            // Geçersiz JSON: oyun çalışmaya devam eder, kullanıcıya bilgi verilir.
            return new Sonuc { Hata = "Amca ne dediğini toparlayamadı, tekrar sorar mısın?" };
        }
    }

    // ---------- Bitiş ----------
    private void Bitir(bool basarili)
    {
        diyalogPanel.SetActive(false);
        bitisPanel.SetActive(true);
        bitisYazi.text = basarili
            ? "TEBRİKLER!\nRıfkı Amca mutlu: MIDIŞ Pizza emin ellerde."
            : "TEKRAR DENEYİN...\nRıfkı Amca üzüldü. Dükkân için yeniden dene.";
    }

    // ---------- Arayüz ----------
    private void ArayuzuKur()
    {
        if (diyalogPanel != null) return;

        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var canvasObj = new GameObject("FinalCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObj.transform.SetParent(transform, false);
        canvasObj.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.GetComponent<Canvas>().sortingOrder = 860;
        var scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Diyalog paneli (ekranın altı)
        diyalogPanel = Kutu(canvasObj.transform, "DiyalogPanel", new Color(0.12f, 0.07f, 0.04f, 0.92f));
        var prt = diyalogPanel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0, 0); prt.anchorMax = new Vector2(1, 0);
        prt.pivot = new Vector2(0.5f, 0); prt.sizeDelta = new Vector2(0, 430);

        isimYazi = Yazi(diyalogPanel.transform, "Rıfkı Amca", 44, FontStyles.Bold, new Color(1f, 0.85f, 0.5f));
        var nrt = isimYazi.rectTransform;
        nrt.anchorMin = new Vector2(0, 1);
        nrt.anchorMax = new Vector2(1, 1);
        nrt.pivot = new Vector2(0, 1);
        nrt.offsetMin = new Vector2(60, -80);
        nrt.offsetMax = new Vector2(-60, -20);
        isimYazi.alignment = TextAlignmentOptions.Left;

        cevapYazi = Yazi(diyalogPanel.transform, "", 46, FontStyles.Normal, new Color(1f, 0.96f, 0.88f));
        var crt = cevapYazi.rectTransform;
        crt.anchorMin = new Vector2(0, 0); crt.anchorMax = new Vector2(1, 1);
        crt.offsetMin = new Vector2(60, 190); crt.offsetMax = new Vector2(-60, -85);
        cevapYazi.alignment = TextAlignmentOptions.TopLeft;

        ipucuYazi = Yazi(diyalogPanel.transform, "", 30, FontStyles.Italic, new Color(0.8f, 0.7f, 0.55f));
        var irt = ipucuYazi.rectTransform;
        irt.anchorMin = new Vector2(0, 0); irt.anchorMax = new Vector2(1, 0);
        irt.pivot = new Vector2(0.5f, 0);
        irt.offsetMin = new Vector2(60, 125); irt.offsetMax = new Vector2(-60, 175);
        ipucuYazi.alignment = TextAlignmentOptions.Left;

        GirisAlaniKur(diyalogPanel.transform);

        // Gönder butonu
        var btn = Kutu(diyalogPanel.transform, "GonderButonu", new Color(0.55f, 0.33f, 0.16f));
        var brt = btn.GetComponent<RectTransform>();
        brt.anchorMin = brt.anchorMax = new Vector2(1, 0);
        brt.pivot = new Vector2(1, 0);
        brt.anchoredPosition = new Vector2(-60, 25); brt.sizeDelta = new Vector2(200, 80);
        gonderBtn = btn.AddComponent<Button>();
        gonderBtn.onClick.AddListener(Gonder);
        var by = Yazi(btn.transform, "SOR", 40, FontStyles.Bold, new Color(1f, 0.93f, 0.78f));
        Doldur(by.rectTransform);

        diyalogPanel.SetActive(false);

        // Bitiş paneli (tam ekran)
        bitisPanel = Kutu(canvasObj.transform, "BitisPanel", new Color(0, 0, 0, 0.9f));
        Doldur(bitisPanel.GetComponent<RectTransform>());
        bitisYazi = Yazi(bitisPanel.transform, "", 72, FontStyles.Bold, new Color(1f, 0.96f, 0.88f));
        var yrt = bitisYazi.rectTransform;
        yrt.anchorMin = yrt.anchorMax = new Vector2(0.5f, 0.5f);
        yrt.sizeDelta = new Vector2(1500, 400); yrt.anchoredPosition = new Vector2(0, 100);

        var menu = Kutu(bitisPanel.transform, "MenuButonu", new Color(0.55f, 0.33f, 0.16f));
        var mrt = menu.GetComponent<RectTransform>();
        mrt.anchorMin = mrt.anchorMax = new Vector2(0.5f, 0.5f);
        mrt.anchoredPosition = new Vector2(0, -200); mrt.sizeDelta = new Vector2(520, 110);
        menu.AddComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(menuSahnesi));
        var my = Yazi(menu.transform, "ANA MENÜ", 48, FontStyles.Bold, new Color(1f, 0.93f, 0.78f));
        Doldur(my.rectTransform);

        bitisPanel.SetActive(false);
    }

    private void GirisAlaniKur(Transform ebeveyn)
    {
        var obj = Kutu(ebeveyn, "Giris", new Color(0.96f, 0.89f, 0.72f));
        var rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0);
        rt.offsetMin = new Vector2(60, 25); rt.offsetMax = new Vector2(-290, 105);

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(obj.transform, false);
        var vrt = viewport.GetComponent<RectTransform>();
        vrt.anchorMin = Vector2.zero; vrt.anchorMax = Vector2.one;
        vrt.offsetMin = new Vector2(20, 8); vrt.offsetMax = new Vector2(-20, -8);

        var metin = Yazi(viewport.transform, "", 40, FontStyles.Normal, new Color(0.23f, 0.14f, 0.07f));
        Doldur(metin.rectTransform);
        metin.alignment = TextAlignmentOptions.MidlineLeft;

        var ph = Yazi(viewport.transform, "Rıfkı Amca'ya bir şey sor...", 40, FontStyles.Italic, new Color(0.5f, 0.4f, 0.3f));
        Doldur(ph.rectTransform);
        ph.alignment = TextAlignmentOptions.MidlineLeft;

        giris = obj.AddComponent<TMP_InputField>();
        giris.textViewport = vrt;
        giris.textComponent = metin;
        giris.placeholder = ph;
        giris.characterLimit = 120;
        giris.lineType = TMP_InputField.LineType.SingleLine;
        giris.onSubmit.AddListener(_ => Gonder());
    }

    private static GameObject Kutu(Transform ebeveyn, string ad, Color renk)
    {
        var o = new GameObject(ad, typeof(RectTransform), typeof(Image));
        o.transform.SetParent(ebeveyn, false);
        o.GetComponent<Image>().color = renk;
        return o;
    }

    private static TMP_Text Yazi(Transform ebeveyn, string metin, float boyut, FontStyles stil, Color renk)
    {
        var o = new GameObject("Yazi", typeof(RectTransform));
        o.transform.SetParent(ebeveyn, false);
        var t = o.AddComponent<TextMeshProUGUI>();
        t.text = metin; t.fontSize = boyut; t.fontStyle = stil; t.color = renk;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }

    private static void Konum(RectTransform rt, Vector2 capa, Vector2 konum, Vector2 olcu,
                              TextAlignmentOptions hiza, TMP_Text yazi)
    {
        rt.anchorMin = rt.anchorMax = capa;
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = konum; rt.sizeDelta = olcu;
        yazi.alignment = hiza;
    }

    private static void Doldur(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }
}