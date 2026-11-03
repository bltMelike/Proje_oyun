using System.Collections;
using UnityEngine;

// Araba korkutulunca titrer, sonra geldiği noktaya geri geri gider ve sahneden çıkar.
public class CarController : MonoBehaviour, IFearable
{
    [SerializeField] private Transform exitPoint;        // boşsa arabanın başlangıç konumu kullanılır
    [SerializeField] private float retreatSpeed = 4f;
    [SerializeField] private float shakeDuration = 1.4f; // hayalet dönerken titreme süresi
    [SerializeField] private float shakeAmount = 0.06f;  // titreme şiddeti
    [SerializeField] private HintTrigger afterFleeHint;  // araba kaçınca gösterilecek yazı

    private Vector3 startPosition;
    private bool scared;

    private void Awake()
    {
        startPosition = transform.position; // arabanın geldiği yer
    }

    public void OnScared()
    {
        if (scared) return;
        scared = true;
        StartCoroutine(ScaredRoutine());
    }

    private IEnumerator ScaredRoutine()
    {
        // Animasyon konumu eziyorsa kapat.
        var anim = GetComponent<Animator>();
        if (anim != null) anim.enabled = false;

        // 1) Hayalet dönerken araba titrer.
        Vector3 basePos = transform.position;
        float t = 0f;
        while (t < shakeDuration)
        {
            t += Time.deltaTime;
            Vector2 offset = Random.insideUnitCircle * shakeAmount;
            transform.position = basePos + (Vector3)offset;
            yield return null;
        }
        transform.position = basePos;

        // 2) Korkup geri geri kaçar.
        Vector3 target = exitPoint != null ? exitPoint.position : startPosition;
        while (Vector2.Distance(transform.position, target) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, target, retreatSpeed * Time.deltaTime);
            yield return null;
        }

        // 3) Yol açıldı: "markete gir" yazısını göster.
        if (afterFleeHint != null) afterFleeHint.Fire();
        gameObject.SetActive(false);
    }
}