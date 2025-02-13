using UnityEngine;
using UnityEngine.EventSystems;

public class DragAreaController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private SpinnerController spinnerController; // 피젯 스피너를 제어하는 컨트롤러
    private RectTransform rectTransform; // 현재 UI 오브젝트(드래그 영역)의 RectTransform
    private Vector2 lastMousePosition; // 마지막 마우스 위치 저장
    private float minDragThreshold = 10f; // 최소 드래그 거리 기준 (이 값보다 짧은 이동은 무시)
    private bool hasMoved; // 사용자가 일정 거리 이상 이동했는지 여부 (속도 적용 여부 결정)

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>(); // RectTransform 컴포넌트 가져오기

        // SpinnerController가 할당되지 않았다면 자동으로 찾음
        if (spinnerController == null)
        {
            spinnerController = FindObjectOfType<SpinnerController>();
        }
    }

    // 사용자가 터치 또는 마우스 클릭을 했을 때 실행
    public void OnPointerDown(PointerEventData eventData)
    {
        if (spinnerController == null) return; // SpinnerController가 없으면 리턴

        // 스피너에 클릭 입력을 전달하여 클릭 시작 처리
        spinnerController.CheckInputClick(eventData.position);

        // 클릭 위치를 RectTransform 내부의 로컬 좌표로 변환하여 저장
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out lastMousePosition);

        // 🛑 클릭한 순간에는 이동하지 않았으므로 초기화
        hasMoved = false;
    }

    // 사용자가 드래그하는 동안 실행
    public void OnDrag(PointerEventData eventData)
    {
        if (spinnerController == null) return; // SpinnerController가 없으면 리턴

        // 현재 마우스 위치를 RectTransform 내부의 로컬 좌표로 변환
        Vector2 currentMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out currentMousePosition);

        // 마우스 이동 거리 계산
        float dragDistance = (currentMousePosition - lastMousePosition).magnitude;

        // ✅ 일정 거리 이상 이동했을 경우에만 속도 증가
        if (dragDistance > minDragThreshold)
        {
            spinnerController.HandleDrag(eventData.position);
            lastMousePosition = currentMousePosition; // 마지막 위치 업데이트
            hasMoved = true; // ✅ 이동했음을 기록
        }

        // 🛑 일정 거리 이상 이동하지 않았다면 속도를 0으로 유지 (갑작스러운 가속 방지)
        if (!hasMoved)
        {
            spinnerController.OnDragEnd(); // 속도를 감소시키도록 강제 적용
        }

        // ✅ 드래그 영역을 벗어났는지 확인하는 로직
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out localPoint
        );

        // 만약 드래그 위치가 영역을 벗어났다면 자동으로 터치 해제 처리
        if (!rectTransform.rect.Contains(localPoint))
        {
            OnPointerUp(eventData);
        }
    }

    // 사용자가 터치 또는 클릭을 해제했을 때 실행
    public void OnPointerUp(PointerEventData eventData)
    {
        if (spinnerController == null) return; // SpinnerController가 없으면 리턴

        // ✅ 드래그가 종료되었음을 SpinnerController에 알림
        spinnerController.OnDragEnd();
    }
}
