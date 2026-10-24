using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Oyuncunun yazısını LLM'e gönderir, cevabı gösterir, kararı oyuna uygular.
public class NpcDialogue : MonoBehaviour
{
    [Header("Model")]
    [SerializeField] private string endpoint = "http://localhost:11434/api/chat";
    [SerializeField] private string model = "qwen2.5:3b";
    [SerializeField] private int timeoutSeconds = 45;

    [Header("Kişilik (system prompt)")]
    [SerializeField, TextArea(6, 14)]
    private string systemPrompt =
        "Sen küçük bir mahalle marketinin kapısında duran görevli Hasansın. Oyuncu markete girmek istiyor. " +
        "Oyuncuya \"siz\" diye hitap et ve oyuncunun cümlesini tekrar etme. " +
        "Önce oyuncunun tavrını değerlendir: kibar ve makul bir sebep sunuyorsa kibar, hakaret tehdit emir ya da küfür varsa kaba, market ile ilgisiz konuşuyorsa alakasiz. " +
        "Sadece kibar ise action step_aside, kaba ve alakasiz ise stay olmalı. " +
        "Sadece JSON ver: {\"attitude\":\"kibar|kaba|alakasiz\",\"reply\":\"...\",\"action\":\"step_aside|stay\"}. " +
        "reply: Hasan olarak söylenecek akıcı Türkçe, en fazla 2 kısa cümle, İngilizce kelime yok. " +
        "Aşağıdaki örneklerdeki cümleleri aynen kopyalama, her seferinde oyuncuya göre kendi cümleni kur. " +
        "Örnek 1 Oyuncu: Merhaba, restoranım için un ve peynir alabilir miyim? Hasan: {\"attitude\":\"kibar\",\"reply\":\"Tabii, buyurun içeri geçin.\",\"action\":\"step_aside\"} " +
        "Örnek 2 Oyuncu: Çekil şuradan yoksa fena olur! Hasan: {\"attitude\":\"kaba\",\"reply\":\"Böyle konuşana yol vermem.\",\"action\":\"stay\"} " +
        "Örnek 3 Oyuncu: Bugün hava nasıl sence? Hasan: {\"attitude\":\"alakasiz\",\"reply\":\"Burası market, ne almak istiyorsunuz?\",\"action\":\"stay\"} " +
        "Örnek 4 Oyuncu: Lütfen içeri girebilir miyim? Hasan: {\"attitude\":\"kibar\",\"reply\":\"Elbette, hoş geldiniz.\",\"action\":\"step_aside\"}";

    [Header("Arayüz")]
    [SerializeField] private TMP_InputField input;
    [SerializeField] private Button sendButton;
    [SerializeField] private TMP_Text replyText;
    [SerializeField] private GameObject thinkingIndicator;   // "Düşünüyor..." yazısı

    [Header("Yazarken kilitlenecek oyuncu")]
    [SerializeField] private OyuncuKontrol playerControl;

    private const int MaxHistory = 8;

    private ILlmClient client;
    private INpcResponseParser parser;
    private INpcActionHandler actionHandler;
    private readonly List<ChatMessage> history = new List<ChatMessage>();
    private bool busy;

    private void Awake()
    {
        client = new OllamaClient(endpoint, model, timeoutSeconds);
        parser = new NpcResponseParser(new[] { NpcActions.StepAside, NpcActions.Stay });
        actionHandler = GetComponent<INpcActionHandler>();
        history.Add(new ChatMessage("system", systemPrompt));
        thinkingIndicator.SetActive(false);

        sendButton.onClick.AddListener(OnSendClicked);
        input.onSubmit.AddListener(_ => OnSendClicked());               // Enter ile gönder
        input.onSelect.AddListener(_ => SetControlsLocked(true));       // kutuya odaklanınca kilitle
        input.onDeselect.AddListener(_ => SetControlsLocked(false));    // kutudan çıkınca aç

        if (playerControl == null)
            Debug.LogWarning("NpcDialogue: Player Control atanmamış! Oyuncu yazarken yürümeye devam eder.");

        _ = WarmUpAsync();   // modeli arka planda ısıt
    }

    private void Update()
    {
        // Esc: konuşmayı bırak, oyuncuyu serbest bırak.
        if (Input.GetKeyDown(KeyCode.Escape))
            ReleaseControls();
    }

    // Oyun açılırken modeli sessizce belleğe yükler (ilk soruda uzun beklemeyi önler).
    private async Awaitable WarmUpAsync()
    {
        var warm = new List<ChatMessage>
        {
            new ChatMessage("system", "Sadece {\"reply\":\"tamam\"} JSON'unu döndür."),
            new ChatMessage("user", "merhaba")
        };
        await client.ChatAsync(warm);   // sonuç önemsiz
    }

    // Panel açılınca yazı kutusuna otomatik odaklan: oyuncu hemen yazabilsin, yürümesin.
    public async Awaitable FocusInputAsync()
    {
        await Awaitable.NextFrameAsync();   // panelin aktif olmasını bekle
        if (this == null) return;
        SetControlsLocked(true);
        input.ActivateInputField();
    }

    // Oyuncuyu serbest bırak ve kutudan odağı al.
    // Oyuncuyu serbest bırak ve kutudan odağı al.
    public void ReleaseControls()
    {
        if (input != null) input.DeactivateInputField();
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        SetControlsLocked(false);
    }

    // Yazarken oyuncu yürümesin: oyuncunun kendi kilit metodunu kullanır.
    private void SetControlsLocked(bool locked)
    {
        if (playerControl != null)
            playerControl.KontrolAktif(!locked);
    }

    private void OnSendClicked()
    {
        _ = SubmitAsync();
    }

    private async Awaitable SubmitAsync()
    {
        string text = input.text.Trim();
        if (busy || string.IsNullOrEmpty(text)) return;

        busy = true;
        sendButton.interactable = false;
        input.text = "";
        thinkingIndicator.SetActive(true);

        history.Add(new ChatMessage("user", text));
        LlmResult result = await client.ChatAsync(history);

        if (this == null) return;   // sahne değişmiş olabilir
        thinkingIndicator.SetActive(false);

        bool opened = false;

        if (!result.Success)
        {
            history.RemoveAt(history.Count - 1);
            replyText.text = "[Sistem] " + result.Error;
        }
        else if (!parser.TryParse(result.Content, out NpcDecision decision, out string error))
        {
            history.RemoveAt(history.Count - 1);
            Debug.LogWarning("LLM çıktısı geçersiz: " + error + " | " + result.Content);
            replyText.text = "[Sistem] Görevli ne dediğini toparlayamadı, tekrar dener misin?";
        }
        else
        {
            history.Add(new ChatMessage("assistant", result.Content));
            TrimHistory();
            Debug.Log($"LLM kararı -> attitude: {decision.attitude}, action: {decision.action}");
            replyText.text = decision.reply;
            actionHandler?.Handle(decision.action);
            opened = decision.action == NpcActions.StepAside;
        }

        busy = false;
        sendButton.interactable = true;

        if (opened) ReleaseControls();          // görevli çekildi: oyuncu serbest
        else input.ActivateInputField();        // yazmaya devam edebilsin
    }

    // Gecikme artmasın diye sadece son mesajları tut (system prompt hep kalır).
    private void TrimHistory()
    {
        while (history.Count > MaxHistory + 1)
            history.RemoveAt(1);
    }
}