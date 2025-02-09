using UnityEngine;

public class DragAreaController : MonoBehaviour
{
    [SerializeField] private SpinnerController spinnerController;

    private void OnMouseDown()
    {
        spinnerController.CheckInputClick(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        Debug.Log("마우스 클릭됨");
    }

    private void OnMouseDrag()
    {
     
        if (spinnerController.isDragging)
        {
            Vector2 currentPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            spinnerController.HandleDrag(currentPosition);
        }
    }

    private void OnMouseUp()
    {
        Debug.Log("마우스 뗌");
        spinnerController.isDragging = false;
    }
}