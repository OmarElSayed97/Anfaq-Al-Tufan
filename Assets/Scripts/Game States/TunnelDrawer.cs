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
    [SerializeField] private bool isDrawing = false;

    [Header("UI")]
    [SerializeField] private TunnelStatsUI tunnelStatsUI;

    private float maxDepth = 0f;
    private float horizontalDistance = 0f;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    public void OnEnter()
    {
        InputManager.Instance.swipePressStarted += OnTunnelDrawStart;
        InputManager.Instance.swipePressEnded += OnTunnelDrawEnd;
        lineRenderer.positionCount = 0;
        maxDepth = 0f;
        horizontalDistance = 0f;
        
    }

    public void OnExit()
    {
        InputManager.Instance.swipePressStarted -= OnTunnelDrawStart;
        InputManager.Instance.swipePressEnded -= OnTunnelDrawEnd;
    }

    void OnTunnelDrawStart(Vector2 pos)
    {
        tunnelStatsUI.Show(true);
        Debug.Log("Started drawing");
        if (GamePhaseManager.Instance.CurrentPhase != GamePhase.TunnelDrawing ||
            playerContext.CurrentLocation != PlayerLocation.AboveGround ||
            playerContext.IsAnimating || GamePhaseManager.Instance.IsInputLocked)
        {
            Debug.LogError("Cannot start drawing tunnel in current state.");
            return;
        }
        float playerX = playerTransform.position.x;
        float startX = Mathf.Clamp(pos.x, playerX - startZoneWidth / 2f, playerX + startZoneWidth / 2f);
        startPoint = new Vector3(startX, 0f, 0f);
        isDrawing = true;
    }

    void OnTunnelDrawEnd(Vector2 pos)
    {
        if (!isDrawing) return;
        tunnelStatsUI.Show(false);
        GamePhaseManager.Instance.SetInputLock(true);
        tunnelNavigator.StartNavigation(ParabolaUtility.GetParabolaPoints(startPoint, controlPoint, endPoint, curveResolution));
        GamePhaseManager.Instance.SetPhase(GamePhase.TunnelNavigation);
        isDrawing = false;
        playerContext.StopBurstingAnimation();
        Debug.Log("Tunnel finalized.");
    }

    public void OnUpdate()
    {
        if (!isDrawing) return;
        if (GamePhaseManager.Instance.CurrentPhase != GamePhase.TunnelDrawing ||
            playerContext.CurrentLocation != PlayerLocation.AboveGround ||
            playerContext.IsAnimating || GamePhaseManager.Instance.IsInputLocked)
        {
            Debug.LogError("Cannot draw tunnel in current state.");
            return;
        }
        // Determine if input is still pressed
        bool isPressed = false;
        Vector2 dragWorld = Vector2.zero;
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
            if (isDrawing)
            {
                OnTunnelDrawEnd(dragWorld);
            }
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

        Debug.Log($"Current Depth: {currentDepth} | Horizontal Distance: {horizontalDistance}");

        // Update UI
        tunnelStatsUI.UpdateStats(horizontalDistance, maxDepth);
    }


    void DrawPreview(Vector3[] points)
    {

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }
}
