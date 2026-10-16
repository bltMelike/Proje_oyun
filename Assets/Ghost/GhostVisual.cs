using UnityEngine;
using UnityEngine.AI;

// Hayaletin GÖRÜNÜMÜNDEN sorumlu sınıf:
// 1) Sağa/sola giderken sprite'ı çevirir (flipX).
// 2) Yöne göre uygun kare dizisini oynatır (yukarı = arkadan, diğer = önden).
// 3) Yürürken süzülme + hafif sallanma efekti verir.
[RequireComponent(typeof(NavMeshAgent))]
public class GhostVisual : MonoBehaviour
{
    [Header("Kare dizileri (Inspector'dan sürükle)")]
    [SerializeField] private Sprite[] idleFrames;   // dururken: hayalet_0
    [SerializeField] private Sprite[] sideFrames;   // sağ/sol ve aşağı: yüzü görünenler
    [SerializeField] private Sprite[] upFrames;     // yukarı: hayalet_1, 5, 9
    [SerializeField] private float frameRate = 8f;

    [Header("Yön")]
    [SerializeField] private bool spriteFacesLeft = false;
    [SerializeField] private float minSpeedToFlip = 0.1f;

    [Header("Yürüme efekti")]
    [SerializeField] private float bobHeight = 0.08f;
    [SerializeField] private float bobSpeed = 10f;
    [SerializeField] private float tiltAngle = 6f;

    private NavMeshAgent agent;
    private SpriteRenderer spriteRenderer;
    private Transform visual;
    private Vector3 visualStartPos;
    private float bobTimer;
    private float frameTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        visual = spriteRenderer.transform;
        visualStartPos = visual.localPosition;
    }

    private void Update()
    {
        Vector3 vel = agent.velocity;
        bool isMoving = vel.sqrMagnitude > 0.01f;

        // Yön çevirme (sadece yatay harekette)
        if (Mathf.Abs(vel.x) > minSpeedToFlip)
        {
            bool movingLeft = vel.x < 0f;
            spriteRenderer.flipX = spriteFacesLeft ? !movingLeft : movingLeft;
        }

        // Hangi kare dizisi oynayacak?
        Sprite[] frames = idleFrames;
        if (isMoving)
        {
            bool goingUp = Mathf.Abs(vel.y) > Mathf.Abs(vel.x) && vel.y > 0f;
            frames = goingUp ? upFrames : sideFrames;
        }
        PlayFrames(frames);

        // Süzülme efekti
        if (isMoving)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            visual.localPosition = visualStartPos + new Vector3(0f, Mathf.Sin(bobTimer) * bobHeight, 0f);
            visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(bobTimer * 0.5f) * tiltAngle);
        }
        else
        {
            visual.localPosition = Vector3.Lerp(visual.localPosition, visualStartPos, Time.deltaTime * 10f);
            visual.localRotation = Quaternion.Lerp(visual.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            bobTimer = 0f;
        }
    }

    private void PlayFrames(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0) return;
        frameTimer += Time.deltaTime * frameRate;
        spriteRenderer.sprite = frames[(int)frameTimer % frames.Length];
    }
}