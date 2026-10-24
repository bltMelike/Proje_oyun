using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ChatMessage
{
    public string role;     // "system", "user", "assistant"
    public string content;

    public ChatMessage(string role, string content)
    {
        this.role = role;
        this.content = content;
    }
}

// İstek sonucu: başarı ya da anlamlı hata mesajı.
public readonly struct LlmResult
{
    public readonly bool Success;
    public readonly string Content;
    public readonly string Error;

    private LlmResult(bool success, string content, string error)
    {
        Success = success; Content = content; Error = error;
    }

    public static LlmResult Ok(string content) => new LlmResult(true, content, null);
    public static LlmResult Fail(string error) => new LlmResult(false, null, error);
}

// LLM sağlayıcısının sözleşmesi (başka sağlayıcı gelirse oyun kodu değişmez).
public interface ILlmClient
{
    Awaitable<LlmResult> ChatAsync(List<ChatMessage> messages);
}