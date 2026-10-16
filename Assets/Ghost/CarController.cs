using System.Collections;
using UnityEngine;

// Araba korkutulunca geldiği noktaya geri geri gider ve sahneden çıkar.
public class CarController : MonoBehaviour, IFearable
{
    [SerializeField] private Transform exitPoint;   // boşsa arabanın başlangıç konumu kullanılır
    [SerializeField] private float retreatSpeed = 4f;
    [SerializeField] private float waitBeforeRetreat = 0.3f;

    private Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.position; // arabanın geldiği yer
    }

    public void OnScared()
    {
        StartCoroutine(ScaredRoutine());
    }

    private IEnumerator ScaredRoutine()
    {
        // Animasyon konumu eziyorsa kapat.
        var anim = GetComponent<Animator>();
        if (anim != null) anim.enabled = false;

        yield return new WaitForSeconds(waitBeforeRetreat);

        Vector3 target = exitPoint != null ? exitPoint.position : startPosition;
        while (Vector2.Distance(transform.position, target) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position, target, retreatSpeed * Time.deltaTime);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}