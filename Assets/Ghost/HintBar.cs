using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Ekranın altında harf harf yazılan ipucu çubuğu. Sahneye elle koymak gerekmez,
// ilk kullanıldığında kendini oluşturur.
public class HintBar : MonoBehaviour
{
    private static HintBar instance;

    public static HintBar Instance
    {
        get
        {
            if (instance == null) instance = Build();
            return instance;
        }
    }

    private GameObject panel;
    private TMP_Text label;
    private Coroutine routine;
    public bool IsShowing => routine != null;

    private static HintBar Build()
    {
        var root = new GameObject("HintBar");
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var bar = root.AddComponent<HintBar>();

        var panelGo = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(root.transform, false);
        var panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.08f, 0.04f);
        panelRect.anchorMax = new Vector2(0.92f, 0.17f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        var image = panelGo.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.7f);
        image.raycastTarget = false;

        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(panelGo.transform, false);
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(24, 12);
        textRect.offsetMax = new Vector2(-24, -12);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 34;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        bar.panel = panelGo;
        bar.label = tmp;
        panelGo.SetActive(false);
        return bar;
    }

    // Mesajı harf harf yazar. holdSeconds > 0 ise yazı bittikten sonra o kadar bekleyip kaybolur.
    public void Show(string message, float holdSeconds = 6f, float charsPerSecond = 30f)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Run(message, holdSeconds, charsPerSecond));
    }

    public void Hide()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        panel.SetActive(false);
    }

    private IEnumerator Run(string message, float holdSeconds, float charsPerSecond)
    {
        panel.SetActive(true);
        label.text = message;
        label.maxVisibleCharacters = 0;
        label.ForceMeshUpdate();
        int total = label.textInfo.characterCount;

        float typed = 0f;
        while (label.maxVisibleCharacters < total)
        {
            typed += Time.unscaledDeltaTime * charsPerSecond;
            label.maxVisibleCharacters = Mathf.Min(total, (int)typed);
            yield return null;
        }

        if (holdSeconds > 0f)
        {
            yield return new WaitForSecondsRealtime(holdSeconds);
            Hide();
        }
    }
}