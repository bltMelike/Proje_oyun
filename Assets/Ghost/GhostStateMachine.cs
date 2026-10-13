// Hayaletin hangi durumda olduğunu tutar ve durumlar arası geçişi yönetir.
// MonoBehaviour değil, düz bir C# sınıfı: sahnedeki bir objeye takılmaz.
public class GhostStateMachine
{
    // Şu an aktif olan durum. Dışarıdan okunabilir, sadece bu sınıf değiştirebilir.
    public IGhostState Current { get; private set; }

    // Eski durumdan çıkar (Exit), yeni duruma girer (Enter).
    public void ChangeState(IGhostState next, GhostController ghost)
    {
        // "?." : Current boşsa (ilk geçişte) hata vermeden atlar.
        Current?.Exit(ghost);
        Current = next;
        Current.Enter(ghost);
    }

    // Her karede aktif durumun Tick metodunu çalıştırır.
    public void Tick(GhostController ghost)
    {
        Current?.Tick(ghost);
    }
}