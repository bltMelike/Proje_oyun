// Hayaletin sahip olabileceği tüm durumların (takip, hedefe gitme, sihir vb.)
// uyması gereken ortak sözleşme. Her durum bu üç metodu yazmak zorundadır.
public interface IGhostState
{
    // Hayalet bu duruma GİRDİĞİ anda bir kez çalışır.
    // Örnek: takip durumuna girince ajanı hareket edebilir hale getirmek.
    void Enter(GhostController ghost);

    // Hayalet bu durumdayken HER KAREDE çalışır.
    // Örnek: oyuncuya olan mesafeyi ölçüp ona doğru yürümek.
    void Tick(GhostController ghost);

    // Hayalet bu durumdan ÇIKTIĞI anda bir kez çalışır.
    // Örnek: eski hedef yolunu temizlemek.
    void Exit(GhostController ghost);
}
