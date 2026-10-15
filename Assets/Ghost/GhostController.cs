using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// Hayaletin ana sınıfı. Sahneler arası kalıcıdır, yeni sahnede oyuncuyu bulup yanına gelir.
[RequireComponent(typeof(NavMeshAgent))]
public class GhostController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followDistance = 2f;
    // Yeni sahnede oyuncunun ne kadar yanında belireceği.
    [SerializeField] private Vector2 spawnOffset = new Vector2(-1f, 0f);

    // Aynı anda tek hayalet olsun (sahne değişince kopya oluşmasın).
    public static GhostController Instance { get; private set; }

    public NavMeshAgent Agent { get; private set; }
    public Transform Player => player;
    public float FollowDistance => followDistance;

    private readonly GhostStateMachine stateMachine = new GhostStateMachine();
    private FollowState followState;

    private void Awake()
    {
        // Zaten bir hayalet varsa (yeni sahnedeki kopya) kendini yok et.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Sahne değişince yok olmasın.
        DontDestroyOnLoad(gameObject);

        Agent = GetComponent<NavMeshAgent>();
        Agent.updateRotation = false;
        Agent.updateUpAxis = false;
        followState = new FollowState();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (Instance != this) return;
        stateMachine.ChangeState(followState, this);
    }

    // Yeni sahne yüklenince oyuncuyu bul ve hayaleti yanına ışınla.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        player = playerObj.transform;
        Vector3 target = player.position + (Vector3)spawnOffset;
        Agent.Warp(target);
    }

    private void Update()
    {
        // Oyuncu yoksa (sahne geçişi anı) bir şey yapma, hata vermesin.
        if (player == null) return;
        stateMachine.Tick(this);
    }
}