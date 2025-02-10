using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class BottomUIButtonsManager : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private List<Button> buttons = new List<Button>();
    [SerializeField] private Sprite xButtonSprite;

    [Header("Popup Settings")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private float popupSpeed = 0.5f;
    [SerializeField] private float popupStartOffset = -600f;

    [Header("Second Button Animation")]
    [SerializeField] private GameObject brownImage;
    [SerializeField] private GameObject redImage;
    [SerializeField] private GameObject spinnerObject;
    [SerializeField] private GameObject secondPopupPanel;
    [SerializeField] private float secondButtonAnimSpeed = 0.5f;
    [SerializeField] private float brownStartOffset = -300f;
    [SerializeField] private float redStartOffset = 300f;
    [SerializeField] private float collisionStopOffset = 30f;
    [SerializeField] private float collisionBounceStrength = 50f;
    [SerializeField] private float collisionBounceDuration = 0.2f;

    [Header("Spinner Settings")]
    [SerializeField] private float spinnerStartScale = 5f;
    [SerializeField] private float spinnerScaleDuration = 0.3f;
    [SerializeField] private float spinnerStartDelay = 0.15f;

    [Header("Second Button Toggle")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private GameObject redToggleImage;
    [SerializeField] private GameObject greenToggleImage;

    private Button activeButton = null;
    private Sprite previousSprite = null;
    private Dictionary<Button, TMP_Text> buttonTextMap = new Dictionary<Button, TMP_Text>();
    private bool isRedActive = true;

    private void Start()
    {
        InitializeButtons();
        HideAllPopups();

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

        // 첫 번째 충돌 애니메이션
        brownTransform.DOAnchorPosY(collisionStopOffset, secondButtonAnimSpeed).SetEase(Ease.OutQuad);
        redTransform.DOAnchorPosY(-collisionStopOffset, secondButtonAnimSpeed)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => PlayBounceEffect(brownTransform, redTransform));

        // 스피너 애니메이션 실행
        spinnerObject.SetActive(false);
        spinnerObject.transform.localScale = Vector3.one * spinnerStartScale;
        spinnerObject.transform.DOScale(Vector3.one, spinnerScaleDuration)
            .SetEase(Ease.OutQuad)
            .SetDelay(spinnerStartDelay)
            .OnStart(() => spinnerObject.SetActive(true));
    }

    private void PlayBounceEffect(RectTransform brownTransform, RectTransform redTransform)
    {
        Sequence collisionSequence = DOTween.Sequence();

        float[] bounceStrengths = { collisionBounceStrength, collisionBounceStrength * 0.6f, collisionBounceStrength * 0.3f };
        float[] bounceDurations = { collisionBounceDuration, collisionBounceDuration * 0.7f, collisionBounceDuration * 0.5f };

        for (int i = 0; i < bounceStrengths.Length; i++)
        {
            float strength = bounceStrengths[i];
            float duration = bounceDurations[i];

            collisionSequence.Append(brownTransform.DOAnchorPosY(collisionStopOffset - strength, duration).SetEase(Ease.OutBack));
            collisionSequence.Join(redTransform.DOAnchorPosY(-collisionStopOffset + strength, duration).SetEase(Ease.OutBack));

            collisionSequence.Append(brownTransform.DOAnchorPosY(collisionStopOffset, duration * 0.8f).SetEase(Ease.InOutQuad));
            collisionSequence.Join(redTransform.DOAnchorPosY(-collisionStopOffset, duration * 0.8f).SetEase(Ease.InOutQuad));
        }
    }

    private void ToggleRedGreenImages()
    {
        isRedActive = !isRedActive;
        redToggleImage.SetActive(isRedActive);
        greenToggleImage.SetActive(!isRedActive);
    }
}