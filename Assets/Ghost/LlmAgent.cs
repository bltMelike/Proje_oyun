using System;
using System.Collections.Generic;
using UnityEngine;

// Bir LLM karakterinin tek bir cevabının sonucu.
public class AgentResult
{
    public bool Success;
    public string Error;
    public string Reply;
    public string Pizza;   // "Karışık" ya da "Special" (sadece müşteri siparişinde dolu)

    public static AgentResult Fail(string error) => new AgentResult { Error = error };
}

// Kişiliği (system prompt) olan, her seferinde verilen duruma cevap veren yapay zekâ karakteri.
// Çıktı JSON olarak istenir, bu sınıf ayrıştırır ve doğrular.
public class LlmAgent
{
    [Serializable]
    private class Dto
    {
        public string reply;
        public string pizza;
    }

    private readonly ILlmClient client;
    private readonly string persona;
    private readonly int maxAttempts;

    public LlmAgent(ILlmClient client, string persona, int maxAttempts = 2)
    {
        this.client = client;
        this.persona = persona;
        this.maxAttempts = maxAttempts;
    }

    public async Awaitable<AgentResult> SpeakAsync(string situation, bool requirePizza)
    {
        var messages = new List<ChatMessage>
        {
            new ChatMessage("system", persona),
            new ChatMessage("user", situation)
        };

        AgentResult last = AgentResult.Fail("Karakter ne diyeceğini toparlayamadı.");

        for (int i = 0; i < maxAttempts; i++)
        {
            LlmResult result = await client.ChatAsync(messages);
            if (!result.Success)
                return AgentResult.Fail(result.Error);   // sunucu hatası: tekrar deneme

            AgentResult parsed = Parse(result.Content, requirePizza, out string problem);
            if (parsed.Success) return parsed;

            Debug.LogWarning($"LLM çıktısı geçersiz (deneme {i + 1}/{maxAttempts}): {problem} | {result.Content}");
        }

        return last;
    }

    private static AgentResult Parse(string raw, bool requirePizza, out string problem)
    {
        problem = null;
        string json = ExtractJson(raw);
        if (json == null) { problem = "JSON bulunamadı"; return AgentResult.Fail(problem); }

        Dto dto;
        try { dto = JsonUtility.FromJson<Dto>(json); }
        catch (Exception e) { problem = "JSON okunamadı: " + e.Message; return AgentResult.Fail(problem); }

        if (dto == null || string.IsNullOrWhiteSpace(dto.reply))
        {
            problem = "Boş cevap";
            return AgentResult.Fail(problem);
        }

        string pizza = null;
        if (requirePizza)
        {
            pizza = NormalizePizza(dto.pizza);
            if (pizza == null)
            {
                problem = "Geçersiz pizza: " + dto.pizza;
                return AgentResult.Fail(problem);
            }
        }

        return new AgentResult { Success = true, Reply = dto.reply.Trim(), Pizza = pizza };
    }

    private static string NormalizePizza(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        string v = value.Trim().ToLowerInvariant();
        if (v.Contains("kar")) return "Karışık";
        if (v.Contains("spe")) return "Special";
        return null;
    }

    private static string ExtractJson(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return null;
        int start = raw.IndexOf('{');
        int end = raw.LastIndexOf('}');
        if (start < 0 || end <= start) return null;
        return raw.Substring(start, end - start + 1);
    }
}