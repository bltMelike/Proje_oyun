using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Oyuncunun yazısını LLM'e gönderir, cevabı gösterir, kararı oyuna uygular.
// Tek prompt + doğrulama katmanı (NpcDecisionGuard).
public class NpcDialogue : MonoBehaviour
{
    [Header("Model")]
    [SerializeField] private string endpoint = "http://localhost:11434/api/chat";
    [SerializeField] private string model = "qwen2.5:3b";
    [SerializeField] private int timeoutSeconds = 45;

    [Header("Kişilik (system prompt)")]
    [SerializeField, TextArea(6, 14)]
    private string hasanPrompt =
        "Sen küçük bir mahalle marketinin kapısındaki görevli Hasansın. Oyuncu markete girmek istiyor. Oyuncuya \"siz\" diye hitap et. " +
        "Sadece JSON ver: {\"attitude\":\"kibar|kaba|alakasiz\",\"reply\":\"...\",\"action\":\"step_aside|stay\"}. " +
        "kibar: oyuncu nazikçe markete girmek istediğini söylüyor (lütfen, rica, miyim, misiniz gibi). Bu durumda action step_aside. " +
        "kaba: hakaret, tehdit, emir (çekil, aç, geç, çabuk ol) ya da sana ne yapacağını söylemek. Bu durumda action stay. " +
        "alakasiz: markete girme isteği yok, konu dışı ya da tek kelimelik cevap. Bu durumda action stay. " +
        "reply: Hasan'ın kısa cevabı, en fazla 2 kısa cümle, doğal Türkçe, İngilizce yok. Kendi adına konuş, oyuncunun cümlesini tekrar etme. " +
        "Örnek Oyuncu: Merhaba, restoranım için un ve peynir alabilir miyim? Hasan: {\"attitude\":\"kibar\",\"reply\":\"Tabii, buyurun içeri geçin.\",\"action\":\"step_aside\"} " +
        "Örnek Oyuncu: Lütfen içeri girebilir miyim? Hasan: {\"attitude\":\"kibar\",\"reply\":\"Elbette, hoş geldiniz.\",\"action\":\"step_aside\"} " +
        "Örnek Oyuncu: Üzgünüm, sinirliydim. Malzeme almam gerekiyor, izin verir misiniz? Hasan: {\"attitude\":\"kibar\",\"reply\":\"Özrünüz kabul, buyurun geçin.\",\"action\":\"step_aside\"} " +
        "Örnek Oyuncu: Çekil önümden! Hasan: {\"attitude\":\"kaba\",\"reply\":\"Bu tonla konuşursanız geçemezsiniz.\",\"action\":\"stay\"} " +
        "Örnek Oyuncu: Hadi çabuk ol çekil Hasan: {\"attitude\":\"kaba\",\"reply\":\"Acele ettirmeyin, böyle konuşana yol vermem.\",\"action\":\"stay\"} " +
        "Örnek Oyuncu: Aptal herif kapıyı aç! Hasan: {\"attitude\":\"kaba\",\"reply\":\"Hakaret ederseniz konuşmam.\",\"action\":\"stay\"} " +
        "Örnek Oyuncu: Önünden çekilebilirsin mesela. Hasan: {\"attitude\":\"kaba\",\"reply\":\"Kapıyı ne zaman açacağıma ben karar veririm.\",\"action\":\"stay\"} " +
        "Örnek Oyuncu: Dün akşam maçı izledin mi? Hasan: {\"attitude\":\"alakasiz\",\"reply\":\"Maç sohbetine vaktim yok, ne almak istiyorsunuz?\",\"action\":\"stay\"} " +
        "Örnek Oyuncu: Evet Hasan: {\"attitude\":\"alakasiz\",\"reply\":\"Neyi onaylıyorsunuz? Ne almak istiyorsunuz?\",\"action\":\"stay\"}";

    [Header("Arayüz")]
    [SerializeField] private TMP_InputField input;
    [SerializeField] private Button sendButton;
    [SerializeField] private TMP_Text replyText;
    [SerializeField] private GameObject thinkingIndicator;   // "Düşünüyor..." yazısı

    [Header("Yazarken kilitlenecek oyuncu")]
    [SerializeField] private OyuncuKontrol playerControl;

    private const int MaxHistory = 2;    // system prompt hariç son mesaj sayısı
    private const int MaxAttempts = 2;   // geçersiz JSON gelirse 1 kez otomatik tekrar dene

    private class DecisionAttempt
    {
        public bool Success;
        public NpcDecision Decision;
        public string Error;
    }

    private ILlmClient client;
    private INpcResponseParser parser;
    private INpcActionHandler actionHandler;
    private NpcDecisionGuard guard;
    private readonly List<ChatMessage> history = new List<ChatMessage>();
    private bool busy;

    private void Awake()
    {
        client = new OllamaClient(endpoint, model, timeoutSeconds);
        parser = new NpcResponseParser(new[] { NpcActions.StepAside, NpcActions.Stay });
        actionHandler = GetComponent<INpcActionHandler>();
        guard = new NpcDecisionGuard();
        history.Add(new ChatMessage("system", hasanPrompt));
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
        if (Input.GetKeyDown(KeyCode.Escape))
            ReleaseControls();
    }

    // Oyun açılırken modeli sessizce belleğe yükler.
    // Oyun açılırken modeli Hasan'ın gerçek prompt'uyla ısıtır:
    // Ollama prompt'u bellekte tutar, ilk mesaj çok daha hızlı gelir.
    private async Awaitable WarmUpAsync()
    {
        var warm = new List<ChatMessage>
        {
            new ChatMessage("system", hasanPrompt),
            new ChatMessage("user", "Merhaba")
        };
        await client.ChatAsync(warm);   // sonuç önemsiz
    }

    public async Awaitable FocusInputAsync()
    {
        await Awaitable.NextFrameAsync();
        if (this == null) return;
        SetControlsLocked(true);
        input.ActivateInputField();
    }

    public void ReleaseControls()
    {
        if (input != null) input.DeactivateInputField();
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        SetControlsLocked(false);
    }

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

        NpcDecision finalDecision = null;
        string failMessage = null;

        // 1) Tek istek: model tavrı, cevabı ve kararı verir.
        DecisionAttempt attempt = await RequestDecisionAsync(history);
        if (this == null) return;   // sahne değişmiş olabilir

        if (!attempt.Success)
        {
            failMessage = attempt.Error;
        }
        else
        {
            finalDecision = attempt.Decision;

            // 2) Güvenlik katmanı: mantıksız "step_aside" kararını engelle.
            string reason;
            if (guard.ShouldBlock(finalDecision, text, out reason))
            {
                Debug.Log($"Guard engelledi: {reason} (modelin kararı: {finalDecision.action})");

                // Nadir durum: reddetme cevabını LLM üretsin (sabit cümle yok).
                DecisionAttempt refusal = await RequestRefusalAsync(reason);
                if (this == null) return;

                if (refusal.Success && refusal.Decision.action == NpcActions.Stay)
                {
                    finalDecision = refusal.Decision;
                }
                else
                {
                    finalDecision = null;
                    failMessage = refusal.Error ?? "Görevli ne dediğini toparlayamadı, tekrar dener misin?";
                }
            }
        }

        thinkingIndicator.SetActive(false);

        bool opened = false;

        if (finalDecision == null)
        {
            history.RemoveAt(history.Count - 1);   // başarısız soruyu geçmişten çıkar
            replyText.text = "[Sistem] " + failMessage;
        }
        else
        {
            guard.Register(finalDecision);
            history.Add(new ChatMessage("assistant", JsonUtility.ToJson(finalDecision)));
            TrimHistory();
            Debug.Log($"LLM kararı -> attitude: {finalDecision.attitude}, action: {finalDecision.action}");
            replyText.text = finalDecision.reply;
            actionHandler?.Handle(finalDecision.action);
            opened = finalDecision.action == NpcActions.StepAside;
        }

        busy = false;
        sendButton.interactable = true;

        if (opened) ReleaseControls();
        else input.ActivateInputField();
    }

    // Modele sorar. Ağ hatasında tekrar denemez, geçersiz JSON'da MaxAttempts kadar dener.
    private async Awaitable<DecisionAttempt> RequestDecisionAsync(List<ChatMessage> messages)
    {
        var attempt = new DecisionAttempt
        {
            Error = "Görevli ne dediğini toparlayamadı, tekrar dener misin?"
        };

        for (int i = 0; i < MaxAttempts; i++)
        {
            LlmResult result = await client.ChatAsync(messages);
            if (this == null) return attempt;

            if (!result.Success)
            {
                attempt.Error = result.Error;
                return attempt;
            }

            if (parser.TryParse(result.Content, out NpcDecision decision, out string error))
            {
                attempt.Success = true;
                attempt.Decision = decision;
                return attempt;
            }

            Debug.LogWarning($"LLM çıktısı geçersiz (deneme {i + 1}/{MaxAttempts}): {error} | {result.Content}");
        }

        return attempt;
    }

    // Guard kapıyı açmayı engelleyince: modelden girişi reddeden bir cevap iste.
    private async Awaitable<DecisionAttempt> RequestRefusalAsync(string reason)
    {
        var messages = new List<ChatMessage>(history);
        messages.Add(new ChatMessage("system",
            "Oyuncuyu içeri alma, action stay olsun. Sebep: " + reason +
            " Bunu Hasan olarak kısaca ve sakince söyle. Aynı JSON biçimini kullan."));

        return await RequestDecisionAsync(messages);
    }

    private void TrimHistory()
    {
        while (history.Count > MaxHistory + 1)
            history.RemoveAt(1);
    }
}