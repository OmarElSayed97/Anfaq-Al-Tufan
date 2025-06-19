// TunnelDrawer.cs
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
    private Vector3 startPoint;
    private Vector3 endPoint;
    private Vector3 controlPoint;
    private bool isDrawing = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        if (GamePhaseManager.Instance.CurrentPhase != GamePhase.TunnelDrawing ||
            playerContext.CurrentLocation != PlayerLocation.AboveGround ||
            playerContext.IsAnimating || GamePhaseManager.Instance.IsInputLocked)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = GetMouseWorldPos();
            float playerX = playerTransform.position.x;
            float startX = Mathf.Clamp(mouseWorld.x, playerX - startZoneWidth / 2f, playerX + startZoneWidth / 2f);

            startPoint = new Vector3(startX, 0f, 0f);
            isDrawing = true;
        }

        if (Input.GetMouseButton(0) && isDrawing)
        {
            Vector3 dragWorld = GetMouseWorldPos();
            float dragX = Mathf.Clamp(dragWorld.x - startPoint.x, -maxCurveWidth, maxCurveWidth);

            // Compute unclamped end point
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
            {
                dragY = -minCurveDepth;
            }

            Vector3 mid = (startPoint + endPoint) / 2f;
            controlPoint = new Vector3(mid.x, dragY, 0f);

            Vector3[] points = ParabolaUtility.GetParabolaPoints(startPoint, controlPoint, endPoint, curveResolution);
            DrawPreview(points);
        }

        if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            isDrawing = false;
            Debug.Log("Tunnel finalized.");
            tunnelNavigator.StartNavigation(ParabolaUtility.GetParabolaPoints(startPoint, controlPoint, endPoint, curveResolution));
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    void DrawPreview(Vector3[] points)
    {
        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }
}
