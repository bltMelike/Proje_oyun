// LLM'in döndürebileceği izinli eylemler. Bunun dışındakiler geçersiz sayılır.
public static class NpcActions
{
    public const string StepAside = "step_aside"; // kapının önünden çekil
    public const string Stay = "stay";            // yerinde kal
}

// LLM'in oyuncuya yönelik değerlendirmesi.
public static class NpcAttitudes
{
    public const string Polite = "kibar";
}