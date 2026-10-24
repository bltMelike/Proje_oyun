using System;

// LLM'den beklenen JSON şeması:
// {"attitude":"kibar|kaba|alakasiz","reply":"...","action":"step_aside|stay"}
[Serializable]
public class NpcDecision
{
    public string attitude;
    public string reply;
    public string action;
}