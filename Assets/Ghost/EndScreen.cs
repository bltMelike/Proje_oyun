using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Oyun sonu ekranı. Sahneye elle koymak gerekmez, kendini oluşturur. Enter ile ana menüye döner.
public class EndScreen : MonoBehaviour
{
    private bool shown;

    public static void Show(string title, string subtitle)
    {
        var root = new GameObject("EndScreen");
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var bg = new GameObject("Bg", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(root.transform, false);
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var img = bg.GetComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.88f);
        img.raycastTarget = false;

        MakeText(bg.transform, title, 90, new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.8f));
        MakeText(bg.transform, subtitle, 40, new Vector2(0.15f, 0.3f), new Vector2(0.85f, 0.55f));
        MakeText(bg.transform, "Ana menüye dönmek için Enter'a bas", 32, new Vector2(0.1f, 0.1f), new Vector2(0.9f, 0.2f));

        root.AddComponent<EndScreen>().shown = true;
    }

    private static void MakeText(Transform parent, string text, float size, Vector2 min, Vector2 max)
    {
        var go = new GameObject("Text", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
    }

    private void Update()
    {
        if (!shown) return;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // Yeni oyun temiz başlasın: kalıcı hayaleti kaldır.
            if (GhostController.Instance != null)
                Destroy(GhostController.Instance.gameObject);
            SceneManager.LoadScene("AnaMenu");
        }
    }
}