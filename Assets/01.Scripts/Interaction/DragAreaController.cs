using UnityEngine;
using UnityEngine.EventSystems;

public class DragAreaController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private SpinnerController spinnerController;

    private void Awake()
    {
        if (spinnerController == null)
        {
            spinnerController = FindObjectOfType<SpinnerController>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (spinnerController == null) return;
        spinnerController.CheckInputClick(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (spinnerController == null) return;
        if (spinnerController.isDragging)
        {
            spinnerController.HandleDrag(eventData.position);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (spinnerController == null) return;
        spinnerController.OnDragEnd();
    }
}