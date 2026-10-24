using System.Collections;
using UnityEngine;

// Görevlinin oyun içindeki davranışı: LLM "step_aside" derse kapıdan çekilir.
public class MarketGuard : MonoBehaviour, INpcActionHandler
{
    [SerializeField] private Transform asidePoint;   // çekileceği yer
    [SerializeField] private float speed = 2f;

    private Coroutine moving;

    public void Handle(string action)
    {
        if (action != NpcActions.StepAside) return;
        if (asidePoint == null)
        {
            Debug.LogError("MarketGuard: Aside Point atanmamış! Inspector'da MarketAsidePoint'i sürükle.");
            return;
        }
        if (moving == null)
            moving = StartCoroutine(MoveAside());
    }

    private IEnumerator MoveAside()
    {
        while (Vector2.Distance(transform.position, asidePoint.position) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(transform.position, asidePoint.position, speed * Time.deltaTime);
            yield return null;
        }
    }
}