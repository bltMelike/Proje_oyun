using System.Collections;
using UnityEngine;

// Hayalet "puf" olup kaybolur. Hayaletin objesini kapatmaz, sadece görüntüsünü gizler.
public class PufEfekti : MonoBehaviour
{
    [Header("Opsiyonel: sihir/puf sprite kareleri (boşsa beyaz halka kullanılır)")]
    [SerializeField] private Sprite[] pufKareleri;
    [SerializeField] private float kareSuresi = 0.1f;

    [Header("Ayarlar")]
    [SerializeField] private float pufOlcegi = 2f;
    [SerializeField] private float yedekSure = 0.6f;
    [SerializeField] private int siralama = 40;

    private SpriteRenderer[] hayaletGorselleri;

    public IEnumerator Oynat()
    {
        var hayalet = GhostController.Instance;
        if (hayalet == null) yield break;

        hayaletGorselleri = hayalet.GetComponentsInChildren<SpriteRenderer>();

        var go = new GameObject("Puf");
        go.transform.position = hayalet.transform.position;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = siralama;

        if (pufKareleri != null && pufKareleri.Length > 0)
        {
            go.transform.localScale = Vector3.one * pufOlcegi;
            for (int i = 0; i < pufKareleri.Length; i++)
            {
                sr.sprite = pufKareleri[i];
                if (i == 1) HayaletiGizle(true);   // efektin ortasında kaybolur
                yield return new WaitForSeconds(kareSuresi);
            }
            if (pufKareleri.Length < 2) HayaletiGizle(true);
        }
        else
        {
            sr.sprite = YuvarlakSprite();
            HayaletiGizle(true);
            float t = 0f;
            while (t < yedekSure)
            {
                t += Time.deltaTime;
                float oran = t / yedekSure;
                go.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, pufOlcegi, oran);
                sr.color = new Color(1f, 1f, 1f, 1f - oran);
                yield return null;
            }
        }

        Destroy(go);
    }

    // Oyun yeniden başlarken hayalet geri gelsin diye.
    public void HayaletiGoster()
    {
        HayaletiGizle(false);
    }

    private void HayaletiGizle(bool gizli)
    {
        if (hayaletGorselleri == null) return;
        foreach (var r in hayaletGorselleri)
            if (r != null) r.enabled = !gizli;
    }

    private static Sprite YuvarlakSprite()
    {
        const int boyut = 64;
        var tex = new Texture2D(boyut, boyut) { filterMode = FilterMode.Bilinear };
        Vector2 merkez = new Vector2(boyut / 2f, boyut / 2f);
        for (int y = 0; y < boyut; y++)
            for (int x = 0; x < boyut; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), merkez) / (boyut / 2f);
                float a = Mathf.Clamp01(1f - d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, boyut, boyut), new Vector2(0.5f, 0.5f), 64f);
    }
}