using UnityEngine;
public class SpinnerController : MonoBehaviour
{
    private float smoothSpeed = 15f;
    private float dampingRate = 0.993f;
    private float spinMinVelocity = 15f;
    private float spinStopThreshold = 40f;
    private float dragRotationSpeed = 0.2f;

    private float minimumDragDistance = 10f;  // 최소 드래그 거리
    private float maxDragDistanceSpeed = 100f; // 드래그 거리에 따른 기본 속도의 최대값

    private float shortDistance = 100f;
    private float middleDistance = 200f;
    private float longDistance = 200f;
    private float shortAcceleration = 50f;  // 가속도 값도 전체적으로 낮춤
    private float middleAcceleration = 300f;
    private float longAcceleration = 1000f;

    private RectTransform rectTransform;
    private float targetAngularVelocity;
    public bool isDragging;
    private Vector2 lastMousePosition;
    private Vector2 dragStartPosition;
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
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, inputPosition, null, out Vector2 localPos);
        lastMousePosition = localPos / 1000f;
        dragStartPosition = lastMousePosition;
        previousVelocity = targetAngularVelocity;
    }

    public void HandleDrag(Vector2 currentPosition)
    {
        if (Time.deltaTime <= 0) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, currentPosition, null, out Vector2 localPos);
        lastMousePosition = localPos / 1000f;
    }

    public void OnDragEnd()
    {
        if (!isDragging) return;
        isDragging = false;

        float dragDistance = (lastMousePosition - dragStartPosition).magnitude;
        Debug.Log($"Drag Distance: {dragDistance}");

        // 최소 드래그 거리 체크
        if (dragDistance < minimumDragDistance) return;

        // 1. 드래그 거리에 따른 기본 속도 계산
        float dragDistanceSpeed = Mathf.Min((dragDistance / longDistance) * maxDragDistanceSpeed, maxDragDistanceSpeed);

        // 2. 거리에 따른 가속도 계산
        float acceleration;
        if (dragDistance < shortDistance)
        {
            acceleration = shortAcceleration;
        }
        else if (dragDistance <= middleDistance)
        {
            acceleration = middleAcceleration;
        }
        else
        {
            acceleration = longAcceleration;
        }

        // 3. 최종 속도 계산 (기본 속도 + 가속도)
        float finalSpeed = dragDistanceSpeed + acceleration;
        Debug.Log($"DragDistanceSpeed: {dragDistanceSpeed}, Acceleration: {acceleration}, FinalSpeed: {finalSpeed}");

        targetAngularVelocity = Mathf.Abs(previousVelocity) + finalSpeed;
        targetAngularVelocity = Mathf.Min(targetAngularVelocity, 6000f);
    }

    private void ApplyRotation()
    {
        if (Mathf.Abs(targetAngularVelocity) > spinMinVelocity)
        {
            targetAngularVelocity *= dampingRate;
        }
        else
        {
            targetAngularVelocity *= dampingRate * 0.98f;
        }

        if (Mathf.Abs(targetAngularVelocity) < spinStopThreshold)
        {
            targetAngularVelocity = Mathf.Lerp(targetAngularVelocity, 0f, Time.deltaTime * 3f);
        }

        rectTransform.Rotate(0, 0, -Mathf.Abs(targetAngularVelocity) * Time.deltaTime);

        if (isDragging)
        {
            Vector2 delta = lastMousePosition - dragStartPosition;
            float targetRotation = -Mathf.Abs(delta.x * dragRotationSpeed);
            float smoothRotation = Mathf.Lerp(0, targetRotation, Time.deltaTime * smoothSpeed);
            rectTransform.Rotate(0, 0, smoothRotation);
        }
    }
}