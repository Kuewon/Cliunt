using UnityEngine;

public class SpinnerUIController : MonoBehaviour
{
    [SerializeField] private Transform worldSpinner;  // 원래 월드에서 동작하는 Spinner
    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private Camera mainCamera;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        mainCamera = Camera.main;

        if (worldSpinner != null)
        {
            MatchSizeAndPosition();
        }
    }

    private void Update()
    {
        if (worldSpinner != null)
        {
            MatchSizeAndPosition(); // 계속 위치, 크기 갱신
        }
    }

    private void MatchSizeAndPosition()
    {
        // 💡 1. 월드 좌표 -> UI 좌표 변환 (월드 좌표를 Canvas의 localPosition으로 변환)
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldSpinner.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPosition, mainCamera, out Vector2 localPoint);
        rectTransform.localPosition = localPoint;

        // 💡 2. 월드 오브젝트 크기 가져오기
        SpriteRenderer spriteRenderer = worldSpinner.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Vector2 spriteSize = spriteRenderer.bounds.size;

            // 💡 3. UI 크기 조정 (Canvas에 맞게 크기 변환)
            float canvasScaleFactor = parentCanvas.scaleFactor;  // Canvas 크기 보정
            rectTransform.sizeDelta = spriteSize * canvasScaleFactor * 100f; // 100은 보정값, 필요하면 수정
        }
    }
}
