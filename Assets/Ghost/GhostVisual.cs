using UnityEngine;
using UnityEngine.AI;

// Hayaletin sadece GÖRÜNÜMÜNDEN sorumlu sınıf:
// 1) Yürüdüğü yöne göre sprite'ı çevirir.
// 2) Yürürken aşağı yukarı süzülme + hafif sallanma efekti verir.
// Hareket mantığı GhostController'da kalır (tek sorumluluk prensibi).
[RequireComponent(typeof(NavMeshAgent))]
public class GhostVisual : MonoBehaviour
{
    [Header("Yön")]
    [SerializeField] private bool spriteFacesLeft = false;
    [SerializeField] private float minSpeedToFlip = 0.1f;

    [Header("Yürüme efekti")]
    [SerializeField] private float bobHeight = 0.08f;   // aşağı yukarı mesafe
    [SerializeField] private float bobSpeed = 10f;      // süzülme hızı
    [SerializeField] private float tiltAngle = 6f;      // sağa sola sallanma açısı

    private NavMeshAgent agent;
    private SpriteRenderer spriteRenderer;
    private Transform visual;      // sadece görüntüyü oynatacağımız çocuk obje
    private Vector3 visualStartPos;
    private float timer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Sprite'ı çocuk objede ararız; yoksa kendi üstünde kullanırız.
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        visual = spriteRenderer.transform;
        visualStartPos = visual.localPosition;
    }

    private void Update()
    {
        float vx = agent.velocity.x;
        bool isMoving = agent.velocity.sqrMagnitude > 0.01f;

        // 1) Yön çevirme
        if (Mathf.Abs(vx) > minSpeedToFlip)
        {
            bool movingLeft = vx < 0f;
            spriteRenderer.flipX = spriteFacesLeft ? !movingLeft : movingLeft;
        }

        // 2) Yürüme efekti: sadece hareket ederken çalışır
        if (isMoving)
        {
            timer += Time.deltaTime * bobSpeed;
            float offsetY = Mathf.Sin(timer) * bobHeight;
            float tilt = Mathf.Sin(timer * 0.5f) * tiltAngle;

            visual.localPosition = visualStartPos + new Vector3(0f, offsetY, 0f);
            visual.localRotation = Quaternion.Euler(0f, 0f, tilt);
        }
        else
        {
            // Dururken yumuşakça başlangıç konumuna dön.
            visual.localPosition = Vector3.Lerp(visual.localPosition, visualStartPos, Time.deltaTime * 10f);
            visual.localRotation = Quaternion.Lerp(visual.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            timer = 0f;
        }
    }
}