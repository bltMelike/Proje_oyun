using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

// Pizza malzemeleri listesini tutar ve ekrandaki yazıyı günceller.
public class ShoppingList : MonoBehaviour
{
    [SerializeField] private string title = "Pizza Malzemeleri";
    [SerializeField] private List<string> items = new List<string> { "Un", "Peynir", "Domates", "Yumurta" };
    [SerializeField] private TMP_Text listText;

    public event Action<string> ItemCollected;   // bir malzeme toplanınca
    public event Action AllCollected;            // liste bitince

    private readonly List<string> remaining = new List<string>();

    private void Awake()
    {
        remaining.AddRange(items);
        Refresh();
    }

    public bool Contains(string itemName)
    {
        return FindIndex(itemName) >= 0;
    }

    // Malzemeyi listeden siler. Listede yoksa false döner.
    public bool Collect(string itemName)
    {
        int index = FindIndex(itemName);
        if (index < 0) return false;

        string collected = remaining[index];
        remaining.RemoveAt(index);
        Refresh();

        ItemCollected?.Invoke(collected);
        if (remaining.Count == 0)
            AllCollected?.Invoke();
        return true;
    }

    private int FindIndex(string itemName)
    {
        for (int i = 0; i < remaining.Count; i++)
            if (string.Equals(remaining[i], itemName, StringComparison.OrdinalIgnoreCase))
                return i;
        return -1;
    }

    private void Refresh()
    {
        if (listText == null) return;

        var sb = new StringBuilder();
        sb.AppendLine(title);
        if (remaining.Count == 0)
        {
            sb.Append("Hepsi tamam!");
        }
        else
        {
            foreach (string item in remaining)
                sb.AppendLine("- " + item);
        }
        listText.text = sb.ToString();
    }
}