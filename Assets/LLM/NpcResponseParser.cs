using System.Collections.Generic;
using UnityEngine;

public interface INpcResponseParser
{
    bool TryParse(string raw, out NpcDecision decision, out string error);
}

// LLM çıktısını güvenle NpcDecision'a çevirir. Bozuk, izinsiz ya da tutarsız çıktıyı reddeder.
public class NpcResponseParser : INpcResponseParser
{
    private readonly HashSet<string> allowedActions;

    public NpcResponseParser(IEnumerable<string> allowedActions)
    {
        this.allowedActions = new HashSet<string>(allowedActions);
    }

    public bool TryParse(string raw, out NpcDecision decision, out string error)
    {
        decision = null;
        error = null;

        // Model bazen JSON'un etrafına metin ekler: ilk { ile son } arasını al.
        int start = raw.IndexOf('{');
        int end = raw.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            error = "JSON bulunamadı";
            return false;
        }

        try
        {
            decision = JsonUtility.FromJson<NpcDecision>(raw.Substring(start, end - start + 1));
        }
        catch (System.Exception e)
        {
            error = "Geçersiz JSON: " + e.Message;
            return false;
        }

        if (decision == null || string.IsNullOrWhiteSpace(decision.reply))
        {
            error = "reply alanı boş";
            return false;
        }
        if (string.IsNullOrWhiteSpace(decision.action) || !allowedActions.Contains(decision.action))
        {
            error = "İzinsiz ya da eksik action: " + decision.action;
            return false;
        }

        // Tutarlılık: kibar olmayan oyuncuya kapı açılamaz.
        if (decision.action == NpcActions.StepAside && decision.attitude != NpcAttitudes.Polite)
        {
            error = "Tutarsız çıktı: kibar olmayan oyuncuya kapı açılamaz";
            return false;
        }
        return true;
    }
}