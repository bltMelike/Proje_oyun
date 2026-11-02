using System;
using System.Collections.Generic;

// LLM'in verdiği kararı doğrulayan güvenlik katmanı.
// Karar modelden gelir; bu sınıf "step_aside" kararını mantıksız durumlarda engeller.
public class NpcDecisionGuard
{
    private const int RudeWindow = 4;   // son kaç cevaba bakılır
    private const int MinWords = 3;     // bundan kısa mesaj kapıyı açamaz

    private static readonly string[] ApologyWords =
        { "özür", "pardon", "affedersin", "kusura bakma", "hata yaptım" };

    // Rica işaretleri: emir değil, rica ya da soru cümlesi olduğunu gösterir.
    private static readonly string[] RequestMarkers =
        { "lütfen", "rica", "izin", "müsaade", "miyim", "mıyım", "muyum", "müyüm",
          "misiniz", "mısınız", "musunuz", "müsünüz" };

    private readonly Queue<string> _recentAttitudes = new Queue<string>();

    // true dönerse kapı açılmamalı. reason, engelleme sebebini verir.
    public bool ShouldBlock(NpcDecision decision, string playerText, out string reason)
    {
        reason = null;
        if (decision == null || decision.action != NpcActions.StepAside)
            return false;

        if (HasRecentRudeness() && !ContainsAny(playerText, ApologyWords))
        {
            reason = "Oyuncu daha önce kaba davrandı ve özür dilemedi.";
            return true;
        }

        if (CountWords(playerText) < MinWords)
        {
            reason = "Oyuncu henüz markete girme sebebini söylemedi.";
            return true;
        }

        if (!ContainsAny(playerText, RequestMarkers))
        {
            reason = "Oyuncu rica etmedi, emir verir ya da yönlendirir gibi konuştu.";
            return true;
        }

        return false;
    }

    // Her cevaptan sonra tavrı kaydet.
    public void Register(NpcDecision decision)
    {
        if (decision == null) return;
        _recentAttitudes.Enqueue(decision.attitude ?? "");
        while (_recentAttitudes.Count > RudeWindow)
            _recentAttitudes.Dequeue();
    }

    public void Reset() => _recentAttitudes.Clear();

    private bool HasRecentRudeness()
    {
        foreach (var a in _recentAttitudes)
            if (a == "kaba") return true;
        return false;
    }

    private static bool ContainsAny(string text, string[] words)
    {
        if (string.IsNullOrEmpty(text)) return false;
        foreach (var w in words)
            if (text.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        return false;
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        return text.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}