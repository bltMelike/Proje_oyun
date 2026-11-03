using UnityEngine;

// Restoran bölümünde hayalet garson ile müşteri arasındaki akış.
// İki karakter de LLM: oyuncu müdahale etmeden kendi aralarında konuşurlar.
public class WaiterFlow : MonoBehaviour
{
    public static WaiterFlow Instance { get; private set; }

    [Header("Noktalar")]
    [SerializeField] private Transform kasaOnu;                // hayaletin bekleyeceği yer, kasanın önü
    [SerializeField] private Transform tezgahAlmaNoktasi;      // pişmiş pizzanın geldiği tezgâhın önü

    [Header("Müşteri")]
    [SerializeField] private Transform customer;               // musteri_12
    [SerializeField] private Transform[] seatPoints;           // OturmaNoktasi1..4
    [SerializeField] private float seatRadius = 0.6f;          // oturdu sayılması için mesafe
    [SerializeField] private Vector2 serviceOffset = new Vector2(-1f, 0f);

    [Header("Menü")]
    [SerializeField] private string[] menu = { "Karışık", "Special" };

    [Header("Pizza görselleri (isteğe bağlı)")]
    [SerializeField] private GameObject pizzaOnCounter;        // tezgâhtaki pişmiş pizza (alınınca kapanır)
    [SerializeField] private GameObject carriedPizza;          // hayaletin taşıdığı pizza görseli
    [SerializeField] private Vector2 carryOffset = new Vector2(0f, 0.6f);

    [Header("Yapay zekâ")]
    [SerializeField] private string endpoint = "http://localhost:11434/api/chat";
    [SerializeField] private string model = "qwen2.5:3b";
    [SerializeField] private int timeoutSeconds = 60;

    [SerializeField, TextArea(4, 10)]
    private string ghostPersona =
        "Sen Midiş Pizza restoranında çalışan, tatlı ve nazik bir hayalet garsonsun. Kısa ve samimi konuşursun. " +
        "Menüde sadece iki pizza var: Karışık ve Special. " +
        "Sadece JSON ver: {\"reply\":\"...\"}. " +
        "reply: en fazla 2 kısa cümle, akıcı Türkçe, İngilizce kelime ve emoji yok.";

    [SerializeField, TextArea(4, 12)]
    private string customerPersona =
        "Sen Midiş Pizza restoranında masaya oturmuş bir müşterisin ve garson hayaletle konuşuyorsun. " +
        "Menüde iki pizza var: karışık ve special. Hangisini isteyeceğine sen karar verirsin. " +
        "Sadece JSON ver: {\"reply\":\"...\",\"pizza\":\"karisik|special\"}. " +
        "reply: en fazla 2 kısa cümle, akıcı Türkçe, İngilizce kelime ve emoji yok. " +
        "pizza alanı, reply içinde istediğin pizzayla aynı olmalı. " +
        "Örnek Garson: Hoş geldiniz! Karışık ve special pizzamız var, hangisini istersiniz? " +
        "Müşteri: {\"reply\":\"Karışık olsun lütfen, çok acıktım.\",\"pizza\":\"karisik\"} " +
        "Örnek Garson: Merhaba! Menümüzde karışık ve special pizza var, hangisi olsun? " +
        "Müşteri: {\"reply\":\"Special pizzayı deneyeyim, merak ettim.\",\"pizza\":\"special\"}";

    [Header("Yazı")]
    [SerializeField] private float messageHold = 1.5f;
    [SerializeField] private float typeSpeed = 40f;

    [Header("Test")]
    [SerializeField] private bool allowDebugKey = true;        // teslimden önce kapat
    [SerializeField] private KeyCode debugPizzaReadyKey = KeyCode.P;

    private class Pending
    {
        public bool Done;
        public AgentResult Result;
    }

    private GhostController ghost;
    private LlmAgent ghostAgent;
    private LlmAgent customerAgent;
    private bool pizzaReady;
    private bool carrying;
    private string chosenPizza;

    // Müşterinin seçtiği pizza: "Karışık" ya da "Special" (pizza yapma kodu doğru pizzayı kontrol etmek isterse).
    public string ChosenPizza => chosenPizza;

    private void Awake()
    {
        Instance = this;
        if (carriedPizza != null) carriedPizza.SetActive(false);

        var client = new OllamaClient(endpoint, model, timeoutSeconds);
        ghostAgent = new LlmAgent(client, ghostPersona);
        customerAgent = new LlmAgent(client, customerPersona);
    }

    // Pizza yapma kodu, pişmiş pizza tezgâha gelince bunu çağırmalı.
    public void NotifyPizzaReady()
    {
        pizzaReady = true;
    }

    private void Start()
    {
        _ = RunAsync();
    }

    private void LateUpdate()
    {
        if (carrying && carriedPizza != null && ghost != null)
            carriedPizza.transform.position = ghost.transform.position + (Vector3)carryOffset;
    }

    private async Awaitable RunAsync()
    {
        await Awaitable.NextFrameAsync();   // sahne yükleme olayları bitsin (hayalet doğsun)
        if (this == null) return;

        ghost = GhostController.Instance;
        if (ghost == null)
        {
            Debug.LogError("WaiterFlow: Sahnede hayalet yok. İlk sahneden başlayarak oynayın.");
            return;
        }

        // 1) Kasanın önünde bekle.
        ghost.HoldAt(kasaOnu);

        // 2) Müşteri oturana kadar bekle.
        await WaitForCustomerSeatedAsync();
        if (this == null) return;

        // 3) Hayalet müşteriye yürürken selamlama cümlesi arka planda üretilir.
        Pending greeting = Request(ghostAgent, GreetingSituation(), false);
        Transform servicePoint = MakeServicePoint();
        await GoToAsync(servicePoint);
        if (this == null) return;
        await WaitFor(greeting, "Hayalet düşünüyor...");
        if (this == null) return;

        // 4) Müşterinin cevabı hayalet konuşurken arka planda üretilir.
        string ghostLine = greeting.Result.Success ? greeting.Result.Reply : "Garson menüyü söyleyip siparişini sordu.";
        Pending customerReply = Request(customerAgent, CustomerSituation(ghostLine), true);
        await SayAgent("Hayalet", greeting.Result);
        if (this == null) return;
        await WaitFor(customerReply, "Müşteri düşünüyor...");
        if (this == null) return;

        string customerLine;
        if (customerReply.Result.Success)
        {
            chosenPizza = customerReply.Result.Pizza;
            customerLine = customerReply.Result.Reply;
            await Say("Müşteri", customerLine);
        }
        else
        {
            // Yapay zekâ cevap vermezse oyun devam etsin: sipariş rastgele belirlenir (cümle uydurulmaz).
            chosenPizza = menu[Random.Range(0, menu.Length)];
            customerLine = chosenPizza + " pizza istedi.";
            await Say("Sistem", "Müşteri cevap veremedi (" + customerReply.Result.Error + "). Sipariş rastgele belirlendi: " + chosenPizza);
        }
        if (this == null) return;

        // 5) Kasaya dönerken hayaletin ana karaktere söyleyeceği cümle hazırlanır.
        Pending relay = Request(ghostAgent, RelaySituation(customerLine, chosenPizza), false);
        await GoToAsync(kasaOnu);
        if (this == null) return;
        await WaitFor(relay, "Hayalet düşünüyor...");
        if (this == null) return;
        await SayAgent("Hayalet", relay.Result);
        if (this == null) return;

        // 6) Pizza hazır olana kadar bekle.
        await WaitForPizzaAsync();
        if (this == null) return;

        // 7) Teslim cümleleri (hayalet ve müşteri) hayalet yürürken arka planda hazırlanır.
        Pending serve = Request(ghostAgent, ServeSituation(chosenPizza), false);
        var thanks = new Pending();
        _ = ThanksChainAsync(serve, thanks);

        await GoToAsync(tezgahAlmaNoktasi);
        if (this == null) return;
        PickUpPizza();
        await GoToAsync(servicePoint);
        if (this == null) return;
        DropPizza();

        await WaitFor(serve, "Hayalet düşünüyor...");
        if (this == null) return;
        await SayAgent("Hayalet", serve.Result);
        if (this == null) return;
        await WaitFor(thanks, "Müşteri düşünüyor...");
        if (this == null) return;
        await SayAgent("Müşteri", thanks.Result);
        if (this == null) return;

        // 8) Kasanın önüne dön ve oyunu bitir.
        await GoToAsync(kasaOnu);
        if (this == null) return;
        EndScreen.Show("OYUN BİTTİ", "Pizza dükkânı açıldı, müşteri mutlu ve hayalet harika bir garson oldu!\nOynadığın için teşekkürler.");
    }

    // ---------- Durum cümleleri (LLM'e verilen bağlam) ----------

    private string GreetingSituation()
    {
        return "Müşteri masaya yeni oturdu. Onu sıcak karşıla, menüdeki iki pizzayı (" +
               string.Join(" ve ", menu) + ") söyle ve hangisini istediğini sor.";
    }

    private string CustomerSituation(string ghostLine)
    {
        string mood = menu[Random.Range(0, menu.Length)].ToLowerInvariant();
        return "Garson hayalet sana şunu dedi: \"" + ghostLine + "\" Bugün canın daha çok " + mood +
               " pizza çekiyor ama kararı sen ver. Siparişini ver.";
    }

    private string RelaySituation(string customerLine, string pizza)
    {
        return "Masadaki müşteri şunu söyledi: \"" + customerLine + "\" Sipariş: " + pizza +
               " pizza. Kasadaki ana karaktere (arkadaşına) siparişi söyle ve pizzayı hazırlamasını iste.";
    }

    private string ServeSituation(string pizza)
    {
        return "Pişmiş " + pizza + " pizzayı müşterinin masasına getirdin. Pizzayı ver ve afiyet olsun de.";
    }

    private string ThanksSituation(string serveLine)
    {
        return "Garson hayalet pizzanı getirdi ve şunu dedi: \"" + serveLine + "\" Teşekkür et ve pizzayı kısaca öv.";
    }

    // ---------- Yardımcılar ----------

    // Arka planda bir LLM isteği başlatır, sonucu Pending'e yazar.
    private Pending Request(LlmAgent agent, string situation, bool requirePizza)
    {
        var pending = new Pending();
        _ = RunRequestAsync(pending, agent, situation, requirePizza);
        return pending;
    }

    private async Awaitable RunRequestAsync(Pending pending, LlmAgent agent, string situation, bool requirePizza)
    {
        pending.Result = await agent.SpeakAsync(situation, requirePizza);
        pending.Done = true;
    }

    // Teslim sırasında: önce hayaletin cümlesi gelsin, sonra müşterinin teşekkürü üretilsin.
    private async Awaitable ThanksChainAsync(Pending serve, Pending thanks)
    {
        while (!serve.Done)
        {
            await Awaitable.NextFrameAsync();
            if (this == null) return;
        }
        string serveLine = serve.Result.Success ? serve.Result.Reply : "Pizzanı getirdim, afiyet olsun.";
        thanks.Result = await customerAgent.SpeakAsync(ThanksSituation(serveLine), false);
        thanks.Done = true;
    }

    private async Awaitable WaitFor(Pending pending, string thinkingText)
    {
        if (!pending.Done)
            HintBar.Instance.Show(thinkingText, 0f, typeSpeed);   // "düşünüyor" göstergesi

        while (!pending.Done)
        {
            await Awaitable.NextFrameAsync();
            if (this == null) return;
        }
    }

    private async Awaitable Say(string speaker, string text)
    {
        HintBar.Instance.Show(speaker + ": " + text, messageHold, typeSpeed);
        while (HintBar.Instance.IsShowing)
        {
            await Awaitable.NextFrameAsync();
            if (this == null) return;
        }
    }

    private async Awaitable SayAgent(string speaker, AgentResult result)
    {
        if (result != null && result.Success)
            await Say(speaker, result.Reply);
        else
            await Say("Sistem", "Yapay zekâ cevap veremedi: " + (result != null ? result.Error : "bilinmeyen hata"));
    }

    private async Awaitable GoToAsync(Transform target)
    {
        bool arrived = false;
        ghost.GoTo(target, () => arrived = true);

        float timeout = 30f;   // takılırsa akış durmasın
        while (!arrived && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            await Awaitable.NextFrameAsync();
            if (this == null) return;
        }
    }

    private async Awaitable WaitForCustomerSeatedAsync()
    {
        float seatedTime = 0f;
        while (seatedTime < 0.7f)
        {
            bool seated = customer != null && customer.gameObject.activeInHierarchy && IsAtSeat(customer.position);
            seatedTime = seated ? seatedTime + Time.deltaTime : 0f;
            await Awaitable.NextFrameAsync();
            if (this == null) return;
        }
    }

    private bool IsAtSeat(Vector3 position)
    {
        foreach (Transform seat in seatPoints)
            if (seat != null && Vector2.Distance(position, seat.position) <= seatRadius)
                return true;
        return false;
    }

    private async Awaitable WaitForPizzaAsync()
    {
        pizzaReady = false;
        while (!pizzaReady)
        {
            if (allowDebugKey && Input.GetKeyDown(debugPizzaReadyKey))
                pizzaReady = true;
            await Awaitable.NextFrameAsync();
            if (this == null) return;
        }
    }

    // Müşterinin yanında hayaletin duracağı noktayı üretir.
    private Transform MakeServicePoint()
    {
        var go = new GameObject("ServisNoktasi");
        go.transform.position = customer.position + (Vector3)serviceOffset;
        return go.transform;
    }

    private void PickUpPizza()
    {
        carrying = true;
        if (pizzaOnCounter != null) pizzaOnCounter.SetActive(false);
        if (carriedPizza != null) carriedPizza.SetActive(true);
    }

    private void DropPizza()
    {
        carrying = false;
        if (carriedPizza != null) carriedPizza.SetActive(false);
    }
}