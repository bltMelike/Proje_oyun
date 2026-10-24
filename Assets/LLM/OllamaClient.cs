using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// Yerel Ollama sunucusuna asenkron istek atar. Ana oyun akışını bloklamaz.
public class OllamaClient : ILlmClient
{
    [Serializable] private class Options { public float temperature = 0.3f; public int num_predict = 120; }
    [Serializable]
    private class Request
    {
        public string model;
        public List<ChatMessage> messages;
        public bool stream = false;
        public string format = "json";      // modeli JSON çıktıya zorlar
        public string keep_alive = "10m";   // model bellekte kalsın
        public Options options = new Options();
    }
    [Serializable] private class Response { public ChatMessage message; }

    private readonly string endpoint;
    private readonly string model;
    private readonly int timeoutSeconds;

    public OllamaClient(string endpoint, string model, int timeoutSeconds)
    {
        this.endpoint = endpoint;
        this.model = model;
        this.timeoutSeconds = timeoutSeconds;
    }

    public async Awaitable<LlmResult> ChatAsync(List<ChatMessage> messages)
    {
        try
        {
            string json = JsonUtility.ToJson(new Request { model = model, messages = messages });

            using (var req = new UnityWebRequest(endpoint, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = timeoutSeconds;

                var op = req.SendWebRequest();
                while (!op.isDone)
                    await Awaitable.NextFrameAsync();   // kare kare bekler, oyun donmaz

                if (req.result == UnityWebRequest.Result.ConnectionError)
                    return LlmResult.Fail("Yapay zekâ sunucusuna ulaşılamadı. Ollama açık mı?");
                if (req.result != UnityWebRequest.Result.Success)
                    return LlmResult.Fail($"Sunucu hatası ({req.responseCode}). Model yüklü mü?");

                var parsed = JsonUtility.FromJson<Response>(req.downloadHandler.text);
                if (parsed == null || parsed.message == null || string.IsNullOrWhiteSpace(parsed.message.content))
                    return LlmResult.Fail("Sunucudan boş yanıt geldi.");

                return LlmResult.Ok(parsed.message.content);
            }
        }
        catch (Exception e)
        {
            return LlmResult.Fail("Beklenmeyen hata: " + e.Message);
        }
    }
}