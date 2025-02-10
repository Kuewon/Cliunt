using UnityEngine;
using UnityEngine.UI;

public class SpinnerControllerUI : MonoBehaviour
{
    public RectTransform spinnerImage; // 회전할 UI 이미지
    public Transform spinnerObject;

    private Vector2 lastMousePosition;
    private float rotationSpeed = 5f; // 회전 속도
    private bool isDragging = false; // 드래그 여부 체크

    void Update()
    {
        transform.rotation = spinnerObject.rotation;
    }

    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastMousePosition;
            float angle = delta.x * rotationSpeed; // 마우스 이동을 회전 값으로 변환
            spinnerImage.Rotate(0, 0, -angle);
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }
}
