using UnityEngine;

public class SpinnerController : MonoBehaviour
{
    public float dragThreshold = 12f;
    public float smoothSpeed = 15f;
    public float dampingRate = 0.95f;
    public float spinMinVelocity = 15f;
    public float spinStopThreshold = 1f;
    public float dragRotationSpeed = 0.3f; // 드래그 중 회전 속도 계수 추가
    private RectTransform rectTransform;
    private float targetAngularVelocity;
    public bool isDragging;
    private Vector2 lastMousePosition;
    private Vector2 dragStartPosition;
    private Vector2 previousDragPosition;
    private float previousVelocity;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        ApplyRotation();
    }

    public void CheckInputClick(Vector2 inputPosition)
    {
        isDragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, inputPosition, null, out lastMousePosition);
        dragStartPosition = lastMousePosition;
        previousDragPosition = lastMousePosition;
        previousVelocity = targetAngularVelocity;
    }

    public void HandleDrag(Vector2 currentPosition)
    {
        if (Time.deltaTime <= 0) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, currentPosition, null, out Vector2 localMousePos);

        // 드래그 중일 때 천천히 회전
        Vector2 delta = localMousePos - previousDragPosition;
        float dragSpeed = Mathf.Abs(delta.x) * dragRotationSpeed;
        rectTransform.Rotate(0, 0, -dragSpeed);

        previousDragPosition = localMousePos;
        lastMousePosition = localMousePos;
    }

    public void OnDragEnd()
    {
        if (!isDragging) return;

        isDragging = false;

        float dragDistance = (lastMousePosition - dragStartPosition).magnitude;
        float newSpeed = Mathf.Clamp(dragDistance * dragThreshold, 100f, 1000f);

        targetAngularVelocity = Mathf.Abs(previousVelocity) + newSpeed;
        targetAngularVelocity = Mathf.Min(targetAngularVelocity, 2000f);
    }

    private void ApplyRotation()
    {
        if (!isDragging)
        {
            if (Mathf.Abs(targetAngularVelocity) > spinMinVelocity)
            {
                targetAngularVelocity *= dampingRate;
            }
            else
            {
                targetAngularVelocity *= dampingRate * 0.95f;
            }

            if (Mathf.Abs(targetAngularVelocity) < spinStopThreshold)
            {
                targetAngularVelocity = Mathf.Lerp(targetAngularVelocity, 0f, Time.deltaTime * 10f);
            }

            rectTransform.Rotate(0, 0, -targetAngularVelocity * Time.deltaTime);
        }
    }
}