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

        // ��� �̹����� �ʺ� ���ϱ�
        backgroundWidth = Mathf.Abs(backgroundImage1.offsetMin.x) + Mathf.Abs(backgroundImage1.offsetMax.x);

        // �� ��° ����� ù ��° ��� �����ʿ� ��Ȯ�� ��ġ
        SetupSecondBackground();

        Debug.Log($"Background Width: {backgroundWidth}");
        Debug.Log($"Background 1 Position: {backgroundImage1.anchoredPosition}");
        Debug.Log($"Background 2 Position: {backgroundImage2.anchoredPosition}");
    }

    private void SetupSecondBackground()
    {
        // ù ��° ����� Left, Right ������ ����
        backgroundImage2.offsetMin = new Vector2(backgroundImage1.offsetMin.x, backgroundImage1.offsetMin.y);
        backgroundImage2.offsetMax = new Vector2(backgroundImage1.offsetMax.x, backgroundImage1.offsetMax.y);

        // �� ��° ����� ù ��° ��� �ٷ� �����ʿ� ��ġ
        Vector2 firstBgPosition = backgroundImage1.anchoredPosition;
        backgroundImage2.anchoredPosition = new Vector2(firstBgPosition.x + backgroundWidth, firstBgPosition.y);

        // anchorMin�� anchorMax�� �����ϰ� ����
        backgroundImage2.anchorMin = backgroundImage1.anchorMin;
        backgroundImage2.anchorMax = backgroundImage1.anchorMax;
    }

    private void Update()
    {
        // ��� ��ũ��
        ScrollBackground(backgroundImage1);
        ScrollBackground(backgroundImage2);
    }

    private void ScrollBackground(RectTransform background)
    {
        // ���� ��ġ
        Vector2 position = background.anchoredPosition;

        // �������� �̵�
        position.x -= scrollSpeed * Time.deltaTime;
        background.anchoredPosition = position;

        // ȭ�� ������ ������ �������� üũ
        if (position.x <= backgroundImage1.anchoredPosition.x - backgroundWidth)
        {
            // ���� ���̴� ����� ���������� �̵�
            position.x += backgroundWidth * 2;
            background.anchoredPosition = position;
        }
    }

    // ����׿� - ��� ��ġ Ȯ��
    private void OnGUI()
    {
        if (backgroundImage1 != null && backgroundImage2 != null)
        {
            GUILayout.Label($"BG1 Pos: {backgroundImage1.anchoredPosition}");
            GUILayout.Label($"BG2 Pos: {backgroundImage2.anchoredPosition}");
        }
    }
}