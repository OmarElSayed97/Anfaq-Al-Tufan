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
    } 

    public void OnExit()
    {
        InputManager.Instance.swipePressStarted -= OnTunnelDrawStart;
        InputManager.Instance.swipePressEnded -= OnTunnelDrawEnd;
    }

    void OnTunnelDrawStart(Vector2 pos)
    {
        Debug.Log("Started drawing");
        if (GamePhaseManager.Instance.CurrentPhase != GamePhase.TunnelDrawing ||
            playerContext.CurrentLocation != PlayerLocation.AboveGround ||
            playerContext.IsAnimating || GamePhaseManager.Instance.IsInputLocked)
        {
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
        GamePhaseManager.Instance.SetInputLock(true);
        tunnelNavigator.StartNavigation(ParabolaUtility.GetParabolaPoints(startPoint, controlPoint, endPoint, curveResolution));
        GamePhaseManager.Instance.SetPhase(GamePhase.TunnelNavigation);
        isDrawing = false;
        Debug.Log("Tunnel finalized.");
    }

    public void OnUpdate()
    {
        if (!isDrawing) return;
        if (GamePhaseManager.Instance.CurrentPhase != GamePhase.TunnelDrawing ||
            playerContext.CurrentLocation != PlayerLocation.AboveGround ||
            playerContext.IsAnimating || GamePhaseManager.Instance.IsInputLocked)
        {
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
            // Input released, finalize tunnel if drawing
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

        DrawPreview(points);
    }

    void DrawPreview(Vector3[] points)
    {
        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }
}
