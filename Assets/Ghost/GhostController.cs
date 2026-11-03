using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// Hayaletin ana sınıfı. Sahneler arası kalıcıdır, durumları ve sensörü yönetir.
[RequireComponent(typeof(NavMeshAgent))]
public class GhostController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followDistance = 2f;
    [SerializeField] private Vector2 spawnOffset = new Vector2(-1f, 0f);
    [SerializeField] private float arriveDistance = 0.4f;
    [SerializeField] private float spellDuration = 1.5f;
    [SerializeField] private float fetchPauseSeconds = 0.6f;   // ürünün yanında durma süresi
    [SerializeField] private RaycastSensor sensorComponent;

    public static GhostController Instance { get; private set; }

    public NavMeshAgent Agent { get; private set; }
    public Transform Player => player;
    public float FollowDistance => followDistance;
    public IObstacleSensor Sensor => sensorComponent;
    public bool LastRouteWasDetour { get; set; }

    private readonly GhostStateMachine stateMachine = new GhostStateMachine();
    private FollowState followState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Agent = GetComponent<NavMeshAgent>();
        Agent.updateRotation = false;
        Agent.updateUpAxis = false;
        followState = new FollowState();

        // Inspector'da atanmadıysa aynı objedeki sensörü kendisi bulsun.
        if (sensorComponent == null)
            sensorComponent = GetComponent<RaycastSensor>();
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void Start()
    {
        if (Instance != this) return;
        ReturnToFollow();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this) return;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        player = playerObj.transform;
        Agent.Warp(player.position + (Vector3)spawnOffset);
        ReturnToFollow();
    }

    private void Update()
    {
        if (player == null) return;

        // Her karede çevreyi gözlemle.
        if (sensorComponent != null)
            sensorComponent.Scan();

        stateMachine.Tick(this);
    }

    // Oyuncunun emri: hedefe yürü, varınca sihir yap (etrafında dön), sonra takibe dön.
    public void CommandMoveTo(Transform approachPoint, Transform spellCenter, Action onSpellCast)
    {
        var spell = new CastSpellState(spellDuration, spellCenter, onSpellCast, ReturnToFollow);
        var move = new MoveToTargetState(approachPoint, arriveDistance,
            () => stateMachine.ChangeState(spell, this));
        stateMachine.ChangeState(move, this);
    }

    // Oyuncunun emri: hedefin yanına yürü, varınca kısa dur (onReached çağrılır), sonra takibe dön.
    public void CommandFetch(Transform target, Action onReached)
    {
        var pause = new FetchPauseState(fetchPauseSeconds, onReached, ReturnToFollow);
        var move = new MoveToTargetState(target, arriveDistance,
            () => stateMachine.ChangeState(pause, this));
        stateMachine.ChangeState(move, this);
    }

    public void ReturnToFollow()
    {
        stateMachine.ChangeState(followState, this);
    }
}