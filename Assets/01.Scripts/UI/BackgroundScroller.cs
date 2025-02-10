using UnityEngine;
using UnityEngine.UI;

public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] private RectTransform backgroundImage1;
    [SerializeField] private RectTransform backgroundImage2;
    [SerializeField] private float scrollSpeed = 2f;

    private float backgroundWidth;

    private void Start()
    {
        if (backgroundImage1 == null || backgroundImage2 == null)
        {
            Debug.LogError("Background images not assigned!");
            return;
        }

        // 배경 이미지의 너비 구하기
        backgroundWidth = Mathf.Abs(backgroundImage1.offsetMin.x) + Mathf.Abs(backgroundImage1.offsetMax.x);

        // 두 번째 배경을 첫 번째 배경 오른쪽에 정확히 배치
        SetupSecondBackground();

        Debug.Log($"Background Width: {backgroundWidth}");
        Debug.Log($"Background 1 Position: {backgroundImage1.anchoredPosition}");
        Debug.Log($"Background 2 Position: {backgroundImage2.anchoredPosition}");
    }

    private void SetupSecondBackground()
    {
        // 첫 번째 배경의 Left, Right 오프셋 복사
        backgroundImage2.offsetMin = new Vector2(backgroundImage1.offsetMin.x, backgroundImage1.offsetMin.y);
        backgroundImage2.offsetMax = new Vector2(backgroundImage1.offsetMax.x, backgroundImage1.offsetMax.y);

        // 두 번째 배경을 첫 번째 배경 바로 오른쪽에 배치
        Vector2 firstBgPosition = backgroundImage1.anchoredPosition;
        backgroundImage2.anchoredPosition = new Vector2(firstBgPosition.x + backgroundWidth, firstBgPosition.y);

        // anchorMin과 anchorMax도 동일하게 설정
        backgroundImage2.anchorMin = backgroundImage1.anchorMin;
        backgroundImage2.anchorMax = backgroundImage1.anchorMax;
    }

    private void Update()
    {
        // 배경 스크롤
        ScrollBackground(backgroundImage1);
        ScrollBackground(backgroundImage2);
    }

    private void ScrollBackground(RectTransform background)
    {
        // 현재 위치
        Vector2 position = background.anchoredPosition;

        // 왼쪽으로 이동
        position.x -= scrollSpeed * Time.deltaTime;
        background.anchoredPosition = position;

        // 화면 밖으로 완전히 나갔는지 체크
        if (position.x <= backgroundImage1.anchoredPosition.x - backgroundWidth)
        {
            // 현재 보이는 배경의 오른쪽으로 이동
            position.x += backgroundWidth * 2;
            background.anchoredPosition = position;
        }
    }

    // 디버그용 - 배경 위치 확인
    private void OnGUI()
    {
        if (backgroundImage1 != null && backgroundImage2 != null)
        {
            GUILayout.Label($"BG1 Pos: {backgroundImage1.anchoredPosition}");
            GUILayout.Label($"BG2 Pos: {backgroundImage2.anchoredPosition}");
        }
    }
}