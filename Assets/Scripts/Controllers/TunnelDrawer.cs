using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TunnelDrawer : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public PlayerContextManager playerContext;
    public TunnelNavigator tunnelNavigator;

    [Header("Tunnel Settings")]
    public float startZoneWidth = 2f;
    public float maxCurveWidth = 5f;
    public float maxCurveDepth = 5f;
    public float minCurveDepth = 0.5f;
    public float edgePadding = 0.5f;
    public int curveResolution = 20;

    private LineRenderer lineRenderer;
    [SerializeField] private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 controlPoint;

    [Header("UI")]
    [SerializeField] private TunnelStatsUI tunnelStatsUI;

    private float maxDepth = 0f;
    private float horizontalDistance = 0f;

    [Header("Draw Timer")]
    [SerializeField] private float startDrawTimeout = 5f; // seconds to start drawing before fail
    private float drawTimeoutTimer;
    private bool timerActive = false;

    // Timer events for UI
    public event System.Action<float> OnDrawTimeoutChanged;
    public event System.Action OnDrawTimeoutEnded;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    public void OnEnter()
    {
        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None).ToList();
        if (enemies == null || enemies.Count == 0)
        {
            GamePhaseManager.Instance.SetPhase(GamePhase.GameWinState);
            return;
        }
        
        InputManager.Instance.swipePressStarted += OnTunnelDrawStart;
        InputManager.Instance.swipePressEnded += OnTunnelDrawEnd;
        GamePhaseManager.Instance.SetInputLock(false);
        lineRenderer.positionCount = 0;
        maxDepth = 0f;
        horizontalDistance = 0f;
        drawTimeoutTimer = startDrawTimeout;
        timerActive = true;
        OnDrawTimeoutChanged?.Invoke(drawTimeoutTimer);
    }

    public void OnExit()
    {
        InputManager.Instance.swipePressStarted -= OnTunnelDrawStart;
        InputManager.Instance.swipePressEnded -= OnTunnelDrawEnd;
        timerActive = false;
    }

    void OnTunnelDrawStart(Vector2 pos)
    {
        tunnelStatsUI.Show(true);
        Debug.Log("Started drawing");
        if (playerContext.CurrentLocation != PlayerLocation.AboveGround)
        {
            Debug.LogWarning("Player not above ground. Moving player above ground.");
            Vector3 playerPos = playerTransform.position;
            playerTransform.position = new Vector3(playerPos.x, 0.2f, playerPos.z);
        }
        if (GamePhaseManager.Instance.CurrentPhase != GamePhase.TunnelDrawing ||
            playerContext.IsAnimating || GamePhaseManager.Instance.IsInputLocked)
        {
            Debug.LogError("Cannot start drawing tunnel in current state.");
            return;
        }
        float playerX = playerTransform.position.x;
        startPoint = new Vector3(playerX, 0f, 0f);
    }

    void OnTunnelDrawEnd(Vector2 pos)
    {
        tunnelStatsUI.Show(false);
        GamePhaseManager.Instance.SetInputLock(true);
        tunnelNavigator.StartNavigation(ParabolaUtility.GetParabolaPoints(startPoint, controlPoint, endPoint, curveResolution));
        GamePhaseManager.Instance.SetPhase(GamePhase.TunnelNavigation);
        timerActive = false;
        playerContext.StopBurstingAnimation();
        Debug.Log("Tunnel finalized.");
    }

    public void OnUpdate()
    {
        if (!timerActive) {
            timerActive = true;
            return;
        }
        // Draw timeout countdown
        if (drawTimeoutTimer > 0f)
        {
            drawTimeoutTimer -= Time.deltaTime;
            if (drawTimeoutTimer < 0f) drawTimeoutTimer = 0f;
            OnDrawTimeoutChanged?.Invoke(drawTimeoutTimer);
            if (drawTimeoutTimer == 0f)
            {
                timerActive = false;
                OnDrawTimeoutEnded?.Invoke();
                Debug.Log("Player failed to start drawing in time. Returning to Idle state.");
                GamePhaseManager.Instance.SetPhase(GamePhase.Idle);
                return;
            }
        }
        if (GamePhaseManager.Instance.CurrentPhase != GamePhase.TunnelDrawing ||
            playerContext.CurrentLocation != PlayerLocation.AboveGround)
        {
            Debug.Log("GamePhaseManager.CurrentPhase: " + GamePhaseManager.Instance.CurrentPhase);
            Debug.Log("PlayerContext.CurrentLocation: " + playerContext.CurrentLocation);
            Debug.Log("PlayerContext.IsAnimating: " + playerContext.IsAnimating);
            Debug.Log("GamePhaseManager.IsInputLocked: " + GamePhaseManager.Instance.IsInputLocked);
            Debug.LogError("Cannot draw tunnel in current state.");
            return;
        }
        // Determine if input is still pressed
        bool isPressed = false;
        Vector2 dragWorld;
        if (UnityEngine.InputSystem.Touchscreen.current != null && UnityEngine.InputSystem.Touchscreen.current.primaryTouch.press.isPressed)
        {
            isPressed = true;
            dragWorld = UnityEngine.InputSystem.Touchscreen.current.primaryTouch.position.ReadValue();
            dragWorld = Camera.main.ScreenToWorldPoint(new Vector3(dragWorld.x, dragWorld.y, Mathf.Abs(Camera.main.transform.position.z)));
        }
        else if (UnityEngine.InputSystem.Pointer.current != null && UnityEngine.InputSystem.Pointer.current.press.isPressed)
        {
            isPressed = true;
            dragWorld = UnityEngine.InputSystem.Pointer.current.position.ReadValue();
            dragWorld = Camera.main.ScreenToWorldPoint(new Vector3(dragWorld.x, dragWorld.y, Mathf.Abs(Camera.main.transform.position.z)));
        }
        else
        {
            dragWorld = InputManager.Instance.lastSwipePos;
        }

        if (!isPressed)
        {
            return;
        }
        // Only update preview while input is pressed
        float dragX = Mathf.Clamp(dragWorld.x - startPoint.x, -maxCurveWidth, maxCurveWidth);
        Vector3 rawEnd = startPoint + new Vector3(dragX, 0f, 0f);
        float z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 leftWorld = Camera.main.ViewportToWorldPoint(new Vector3(0f, 0.5f, z));
        Vector3 rightWorld = Camera.main.ViewportToWorldPoint(new Vector3(1f, 0.5f, z));
        float safeMinX = leftWorld.x + edgePadding;
        float safeMaxX = rightWorld.x - edgePadding;
        float clampedX = Mathf.Clamp(rawEnd.x, safeMinX, safeMaxX);
        endPoint = new Vector3(clampedX, 0f, 0f);

        float dragY = Mathf.Clamp(dragWorld.y, -maxCurveDepth, 0f);
        if (Mathf.Abs(dragY) < minCurveDepth)
            dragY = -minCurveDepth;

        Vector3 mid = (startPoint + endPoint) / 2f;
        controlPoint = new Vector3(mid.x, dragY, 0f);
        Vector3[] points = ParabolaUtility.GetParabolaPoints(startPoint, controlPoint, endPoint, curveResolution);

        // Update stats based on lowest Y point
        Vector3 deepestPoint = points[0];
        foreach (var point in points)
        {
            if (point.y < deepestPoint.y)
                deepestPoint = point;
        }

        UpdateTunnelStats(deepestPoint);
        DrawPreview(points);
    }

    void UpdateTunnelStats(Vector3 currentPoint)
    {
        // Update horizontal distance
        horizontalDistance = Mathf.Abs(currentPoint.x - startPoint.x);

        float currentDepth = Mathf.Abs(currentPoint.y);
        maxDepth = currentDepth;


        // Update UI
        tunnelStatsUI.UpdateStats(horizontalDistance, maxDepth);
    }


    void DrawPreview(Vector3[] points)
    {

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }
}
