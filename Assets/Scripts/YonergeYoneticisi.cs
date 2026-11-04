using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class YonergeYoneticisi : MonoBehaviour
{
    [Header("Bilgilendirme bandı (altta)")]
    [SerializeField] CanvasGroup bilgilendirmeBandi;
    [SerializeField] float gorunmeSuresi = 5f;
    [SerializeField] float solmaSuresi = 1f;

    [Header("Yönerge paneli")]
    [SerializeField] GameObject yonergePaneli;

    [Header("İsteğe bağlı: sağ üst simge")]
    [SerializeField] Button yonergeSimgesi;

    [Header("Ayarlar")]
    [SerializeField] bool oyunuDurdur = true;

    void Start()
    {
        Time.timeScale = 1f;
        yonergePaneli.SetActive(false);

        if (yonergeSimgesi != null)
            yonergeSimgesi.onClick.AddListener(PaneliDegistir);

        StartCoroutine(BilgilendirmeGoster());
    }

    void Update()
    {
        if (HTusuBasildi()) PaneliDegistir();
        if (yonergePaneli.activeSelf && EscBasildi()) PaneliKapat();
    }
    
    // --- Girdi: hem eski hem yeni Input System ile çalışır ---
    bool HTusuBasildi()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.H);
#endif
    }

    bool EscBasildi()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    // --- Bilgilendirme bandı ---
    IEnumerator BilgilendirmeGoster()
    {
        bilgilendirmeBandi.gameObject.SetActive(true);
        bilgilendirmeBandi.alpha = 1f;

        yield return new WaitForSecondsRealtime(gorunmeSuresi);

        float t = 0f;
        while (t < solmaSuresi)
        {
            t += Time.unscaledDeltaTime;
            bilgilendirmeBandi.alpha = 1f - (t / solmaSuresi);
            yield return null;
        }

        bilgilendirmeBandi.gameObject.SetActive(false);
    }

    // --- Panel ---
    void PaneliDegistir()
    {
        if (yonergePaneli.activeSelf) PaneliKapat();
        else PaneliAc();
    }

    void PaneliAc()
    {
        yonergePaneli.SetActive(true);
        if (oyunuDurdur) Time.timeScale = 0f;
    }

    void PaneliKapat()
    {
        yonergePaneli.SetActive(false);
        if (oyunuDurdur) Time.timeScale = 1f;
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;   // sahne değişirse oyun donuk kalmasın
    }
}