using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody), typeof(LineRenderer))]
public class Checker : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Force Settings")]
    [SerializeField] private float minForce = 5f;
    [SerializeField] private float maxForce = 20f;
    [SerializeField] private float maxDragDistance = 2f;
   
    
    [Header("Line Renderer Settings")]
    [SerializeField] [Range(0.01f, 2f)] 
    private float lineLengthMultiplier = 0.3f;
    [SerializeField] private float maxLineLength = 3f;

    [Header("Board Settings")]
    [SerializeField] private float boardHeight = 0.5f;

    private Rigidbody rb;
    private LineRenderer lineRenderer;
    private Camera mainCamera;
    private Vector3 dragStartWorldPos;
    private bool isSelected;
    private float currentForce;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
        mainCamera = Camera.main;
        lineRenderer.enabled = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsPlayersChecker()) return;

        dragStartWorldPos = GetBoardIntersectionPoint(eventData.position);
        isSelected = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isSelected) return;
        lineRenderer.enabled = true;
        var currentWorldPos = GetBoardIntersectionPoint(eventData.position);
        var dragVector = currentWorldPos - dragStartWorldPos;
        
        currentForce = Mathf.Lerp(minForce, maxForce, 
            Mathf.Clamp01(dragVector.magnitude / maxDragDistance));

        UpdateLineRenderer(-dragVector.normalized);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isSelected) return;

        var currentWorldPos = GetBoardIntersectionPoint(eventData.position);
        var forceDirection = (currentWorldPos - dragStartWorldPos).normalized;
        
        ApplyForce(-forceDirection * currentForce);
        ResetVisuals();
    }

    private Vector3 GetBoardIntersectionPoint(Vector2 screenPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        Plane boardPlane = new Plane(Vector3.up, new Vector3(0, boardHeight, 0));
        
        if (boardPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }

    private void UpdateLineRenderer(Vector3 direction)
    {
        // Рассчитываем базовую длину
        var baseLength = currentForce * lineLengthMultiplier;
    
        // Ограничиваем максимальную длину
        var finalLength = Mathf.Min(baseLength, maxLineLength);
        
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + direction * finalLength);

        var t = currentForce / maxForce;
        lineRenderer.startColor = Color.Lerp(Color.green, Color.red, t);
        lineRenderer.endColor = Color.Lerp(Color.green, Color.red, t);
    }

    private void ApplyForce(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
       // GameManager.Instance.SwitchTurn();
    }

    private void ResetVisuals()
    {
        isSelected = false;
        lineRenderer.enabled = false;
        currentForce = 0f;
    }

    private bool IsPlayersChecker()
    {
        return true; // GameManager.Instance.IsCurrentPlayer(/* your logic here */);
    }
}