using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class BottomUIButtonsManager : MonoBehaviour
{
    [Header("UI Buttons")] [SerializeField]
    private List<Button> buttons = new List<Button>();

    [SerializeField] private Sprite xButtonSprite;

    [Header("Popup Settings")] [SerializeField]
    private GameObject popupPanel;

    [SerializeField] private float popupSpeed = 0.5f;
    [SerializeField] private float popupStartOffset = -600f;

    [Header("Second Button Animation")] [SerializeField]
    private GameObject brownImage;

    [SerializeField] private GameObject redImage;
    [SerializeField] private GameObject spinnerObject;
    [SerializeField] private GameObject secondPopupPanel;
    [SerializeField] private float secondButtonAnimSpeed = 0.5f;
    [SerializeField] private float brownStartOffset = -300f;
    [SerializeField] private float redStartOffset = 300f;
    [SerializeField] private float collisionStopOffset = 30f;
    [SerializeField] private float collisionBounceStrength = 50f;
    [SerializeField] private float collisionBounceDuration = 0.2f;

    [Header("Spinner Settings")] [SerializeField]
    private float spinnerStartScale = 5f;

    [SerializeField] private float spinnerScaleDuration = 0.3f;
    [SerializeField] private float spinnerStartDelay = 0.15f;

    [Header("Second Button Toggle")] [SerializeField]
    private Button toggleButton;

    [SerializeField] private GameObject redToggleImage;
    [SerializeField] private GameObject greenToggleImage;

    private Button activeButton = null;
    private Sprite previousSprite = null;
    private Dictionary<Button, TMP_Text> buttonTextMap = new Dictionary<Button, TMP_Text>();
    private bool isRedActive = true;
    private bool isAnimating = false; // 🔹 애니메이션 진행 여부를 저장하는 변수

    private void Start()
    {
        InitializeButtons();
        HideAllPopups();

        isAnimating = false; // 🔹 버튼 입력 가능 상태로 초기화

        // 🔹 초기 상태 설정 (빨간색이 기본적으로 보이고, 초록색은 오른쪽에서 대기)
        isRedActive = true;
        redToggleImage.SetActive(true);
        greenToggleImage.SetActive(false);

        RectTransform greenTransform = greenToggleImage.GetComponent<RectTransform>();
        RectTransform redTransform = redToggleImage.GetComponent<RectTransform>();

        // 🔹 초록색을 오른쪽 바깥에 배치하여 첫 번째 클릭 시 자연스럽게 이동하도록 설정
        greenTransform.anchoredPosition = new Vector2(toggleMoveDistance, greenTransform.anchoredPosition.y);
        redTransform.anchoredPosition = new Vector2(0, redTransform.anchoredPosition.y);

        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleRedGreenImages);
    }

    private void InitializeButtons()
    {
        foreach (var button in buttons)
        {
            button.onClick.AddListener(() => ToggleButton(button));
            TMP_Text tmpText = button.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
                buttonTextMap[button] = tmpText;
        }
    }

    public void ToggleButton(Button clickedButton)
    {
        if (!buttons.Contains(clickedButton)) return;

        if (activeButton == clickedButton)
        {
            ResetButton(clickedButton);
            HideAllPopups();
            return;
        }

        HideAllPopups();
        ActivateNewButton(clickedButton);

        if (clickedButton == buttons[0]) ShowPopup();
        else if (clickedButton == buttons[1]) PlayAnimation();
    }

    private void HideAllPopups()
    {
        popupPanel?.SetActive(false);
        secondPopupPanel?.SetActive(false);
        brownImage?.SetActive(false);
        redImage?.SetActive(false);
        spinnerObject?.SetActive(false);

        if (activeButton != null)
        {
            ResetButton(activeButton);
            activeButton = null;
        }
    }

    private void ResetButton(Button clickedButton)
    {
        if (clickedButton.TryGetComponent(out Image clickedImage) && previousSprite != null)
        {
            clickedImage.sprite = previousSprite;
            clickedImage.SetNativeSize();
        }

        if (buttonTextMap.TryGetValue(clickedButton, out TMP_Text clickedText))
            clickedText.gameObject.SetActive(true);
    }

    private void ActivateNewButton(Button clickedButton)
    {
        if (clickedButton.TryGetComponent(out Image clickedImage))
        {
            previousSprite = clickedImage.sprite;
            clickedImage.sprite = xButtonSprite;
            clickedImage.SetNativeSize();
        }

        if (buttonTextMap.TryGetValue(clickedButton, out TMP_Text clickedText))
            clickedText.gameObject.SetActive(false);

        activeButton = clickedButton;
    }

    private void ShowPopup()
    {
        if (popupPanel == null) return;

        popupPanel.SetActive(true);
        RectTransform popupTransform = popupPanel.GetComponent<RectTransform>();
        popupTransform.anchoredPosition = new Vector2(0, popupStartOffset);
        popupTransform.DOAnchorPosY(0, popupSpeed).SetEase(Ease.OutBack);
    }

    private void PlayAnimation()
    {
        if (brownImage == null || redImage == null || spinnerObject == null || secondPopupPanel == null) return;

        brownImage.SetActive(true);
        redImage.SetActive(true);
        secondPopupPanel.SetActive(true);

        RectTransform brownTransform = brownImage.GetComponent<RectTransform>();
        RectTransform redTransform = redImage.GetComponent<RectTransform>();

        brownTransform.anchoredPosition = new Vector2(0, brownStartOffset);
        redTransform.anchoredPosition = new Vector2(0, redStartOffset);

        brownTransform.DOAnchorPosY(collisionStopOffset, secondButtonAnimSpeed).SetEase(Ease.OutQuad);
        redTransform.DOAnchorPosY(-collisionStopOffset, secondButtonAnimSpeed)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => PlayBounceEffect(brownTransform, redTransform));

        // 🚀 버튼 클릭 후 `spinnerStartDelay` 후 스피너 실행
        DOVirtual.DelayedCall(spinnerStartDelay, () => StartSpinnerAnimation());
    }

    private void PlayBounceEffect(RectTransform brownTransform, RectTransform redTransform)
    {
        Sequence collisionSequence = DOTween.Sequence();

        float[] bounceStrengths =
            { collisionBounceStrength, collisionBounceStrength * 0.6f, collisionBounceStrength * 0.3f };
        float[] bounceDurations =
            { collisionBounceDuration, collisionBounceDuration * 0.7f, collisionBounceDuration * 0.5f };

        for (int i = 0; i < bounceStrengths.Length; i++)
        {
            float strength = bounceStrengths[i];
            float duration = bounceDurations[i];

            collisionSequence.Append(brownTransform.DOAnchorPosY(collisionStopOffset - strength, duration)
                .SetEase(Ease.OutBack));
            collisionSequence.Join(redTransform.DOAnchorPosY(-collisionStopOffset + strength, duration)
                .SetEase(Ease.OutBack));

            collisionSequence.Append(brownTransform.DOAnchorPosY(collisionStopOffset, duration * 0.8f)
                .SetEase(Ease.InOutQuad));
            collisionSequence.Join(redTransform.DOAnchorPosY(-collisionStopOffset, duration * 0.8f)
                .SetEase(Ease.InOutQuad));
        }
    }

    private void StartSpinnerAnimation()
    {
        if (spinnerObject == null) return;

        spinnerObject.SetActive(true);
        spinnerObject.transform.localScale = Vector3.one * spinnerStartScale;

        spinnerObject.transform.DOScale(Vector3.one, spinnerScaleDuration)
            .SetEase(Ease.OutQuad);
    }

    [Header("Second Button Toggle Animation")] [SerializeField]
    private float toggleAnimationDuration = 0.3f; // 토글 애니메이션 지속 시간

    [SerializeField] private float toggleMoveDistance = 100f; // 토글 이동 거리 (픽셀 단위)

    private void ToggleRedGreenImages()
    {
        // 🔹 현재 실행 중인 애니메이션이 있다면 중복 실행 방지
        if (isAnimating) return;

        isAnimating = true; // 🔹 애니메이션 시작 → 입력 차단

        float animationDuration = toggleAnimationDuration; // 애니메이션 지속 시간
        float moveDistance = toggleMoveDistance; // 이동 거리 (픽셀 단위)

        RectTransform redTransform = redToggleImage.GetComponent<RectTransform>();
        RectTransform greenTransform = greenToggleImage.GetComponent<RectTransform>();

        if (isRedActive)
        {
            // 🔹 초록색을 빨간색 오른쪽에 배치하여 붙여 놓음 (왼쪽으로 이동할 준비)
            greenToggleImage.SetActive(true);
            greenTransform.anchoredPosition = new Vector2(moveDistance, greenTransform.anchoredPosition.y);
            redTransform.anchoredPosition = new Vector2(0, redTransform.anchoredPosition.y);

            // 🔹 두 개의 이미지를 함께 왼쪽으로 이동
            Sequence transitionSequence = DOTween.Sequence();
            transitionSequence.Append(redTransform.DOAnchorPosX(-moveDistance, animationDuration)
                .SetEase(Ease.OutQuad));
            transitionSequence.Join(greenTransform.DOAnchorPosX(0, animationDuration).SetEase(Ease.OutQuad));
            transitionSequence.OnComplete(() =>
            {
                redToggleImage.SetActive(false);
                redTransform.anchoredPosition = new Vector2(0, redTransform.anchoredPosition.y);
                greenTransform.anchoredPosition = new Vector2(0, greenTransform.anchoredPosition.y);
                isAnimating = false; // 🔹 애니메이션 종료 → 입력 허용
            });

            isRedActive = !isRedActive;
        }
        else
        {
            // 🔹 빨간색을 초록색 왼쪽에 배치하여 붙여 놓음 (오른쪽으로 이동할 준비)
            redToggleImage.SetActive(true);
            redTransform.anchoredPosition = new Vector2(-moveDistance, redTransform.anchoredPosition.y);
            greenTransform.anchoredPosition = new Vector2(0, greenTransform.anchoredPosition.y);

            // 🔹 두 개의 이미지를 함께 오른쪽으로 이동
            Sequence transitionSequence = DOTween.Sequence();
            transitionSequence.Append(
                greenTransform.DOAnchorPosX(moveDistance, animationDuration).SetEase(Ease.OutQuad));
            transitionSequence.Join(redTransform.DOAnchorPosX(0, animationDuration).SetEase(Ease.OutQuad));
            transitionSequence.OnComplete(() =>
            {
                greenToggleImage.SetActive(false);
                greenTransform.anchoredPosition = new Vector2(0, greenTransform.anchoredPosition.y);
                redTransform.anchoredPosition = new Vector2(0, redTransform.anchoredPosition.y);
                isAnimating = false; // 🔹 애니메이션 종료 → 입력 허용
            });

            isRedActive = !isRedActive;
        }
    }
}