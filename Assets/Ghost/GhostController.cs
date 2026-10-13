using UnityEngine;
using UnityEngine.AI;

// Hayaletin ana sınıfı. Sahnede hayalet objesine takılacak.
// RequireComponent: objede NavMeshAgent yoksa Unity otomatik ekler.
[RequireComponent(typeof(NavMeshAgent))]
public class GhostController : MonoBehaviour
{
    // Inspector'dan atanır: hayaletin takip edeceği oyuncu.
    [SerializeField] private Transform player;
    // Oyuncuya ne kadar yaklaşınca duracağı (birim cinsinden).
    [SerializeField] private float followDistance = 2f;

    // Durumların erişebilmesi için dışarıya açılan, sadece okunabilir özellikler.
    public NavMeshAgent Agent { get; private set; }
    public Transform Player => player;
    public float FollowDistance => followDistance;

    private readonly GhostStateMachine stateMachine = new GhostStateMachine();
    private FollowState followState;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        // 2D için şart: ajanın objeyi döndürmesini ve Z ekseninde oynatmasını engeller.
        Agent.updateRotation = false;
        Agent.updateUpAxis = false;

        followState = new FollowState();
    }

    // Oyun başlayınca ilk durum: oyuncuyu takip et.
    private void Start()
    {
        stateMachine.ChangeState(followState, this);
    }

    // Her karede aktif durumu çalıştır.
    private void Update()
    {
        stateMachine.Tick(this);
    }
}