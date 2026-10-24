using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
        "Önce oyuncunun tavrını değerlendir: kibar ve makul bir sebep sunuyorsa \"kibar\"; hakaret, tehdit, emir ya da küfür varsa \"kaba\"; konuyla ilgisizse \"alakasiz\". " +
        "Sadece \"kibar\" ise action \"step_aside\", \"kaba\" ve \"alakasiz\" ise \"stay\" olmalı. " +
        "Sadece şu JSON biçiminde cevap ver: {\"attitude\":\"kibar|kaba|alakasiz\",\"reply\":\"...\",\"action\":\"step_aside|stay\"}. " +
        "reply: Hasan olarak oyuncuya söylenecek akıcı Türkçe cevap, en fazla 2 kısa cümle, İngilizce kelime yok. " +
        "Cevabı oyuncunun söylediğine göre kendin yaz, kalıp cümle kullanma. " +
        "Eşleşmeler: kibarca izin isteyen oyuncu -> kibar, step_aside; tehdit ya da hakaret eden oyuncu -> kaba, stay; maç, hava gibi konu dışı konuşan oyuncu -> alakasiz, stay.";

    [Header("Arayüz")]
    [SerializeField] private TMP_InputField input;
    [SerializeField] private Button sendButton;
    [SerializeField] private TMP_Text replyText;
    [SerializeField] private GameObject thinkingIndicator;   // "Düşünüyor..." yazısı

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

        _ = WarmUpAsync();   // modeli arka planda ısıt
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
            replyText.text = decision.reply;
            actionHandler?.Handle(decision.action);
        }

        busy = false;
        sendButton.interactable = true;
    }

    // Gecikme artmasın diye sadece son mesajları tut (system prompt hep kalır).
    private void TrimHistory()
    {
        while (history.Count > MaxHistory + 1)
            history.RemoveAt(1);
    }
}