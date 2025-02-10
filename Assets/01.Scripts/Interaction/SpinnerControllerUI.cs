using UnityEngine;
using UnityEngine.UI;

public class SpinnerControllerUI : MonoBehaviour
{
    public RectTransform spinnerImage; // ȸ���� UI �̹���
    public Transform spinnerObject;

    private Vector2 lastMousePosition;
    private float rotationSpeed = 5f; // ȸ�� �ӵ�
    private bool isDragging = false; // �巡�� ���� üũ

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
            float angle = delta.x * rotationSpeed; // ���콺 �̵��� ȸ�� ������ ��ȯ
            spinnerImage.Rotate(0, 0, -angle);
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }
}
