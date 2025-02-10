using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class BottomUIButtonsManager : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private List<Button> buttons = new List<Button>(); // 버튼 리스트
    [SerializeField] private Sprite xButtonSprite; // X 버튼 이미지

    [Header("Popup Settings")]
    [SerializeField] private GameObject popupPanel; // 팝업 창
    [SerializeField] private float popupSpeed = 0.5f; // 팝업이 올라오는 속도
    [SerializeField] private float bounceStrength = 1.2f; // 튕기는 정도
    [SerializeField] private float popupStartOffset = -600f; // 시작 위치 (Y 좌표)
    
    private Button activeButton = null; // 현재 활성화된 버튼
    private Sprite previousSprite = null; // 원래 버튼 이미지 저장
    private Dictionary<Button, TMP_Text> buttonTextMap = new Dictionary<Button, TMP_Text>(); // 버튼-텍스트 매핑

    private void Start()
    {
        InitializeButtons();

        if (popupPanel != null)
        {
            popupPanel.SetActive(false); // 시작 시 팝업 숨김
        }
    }

    private void InitializeButtons()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            Button button = buttons[i];
            button.onClick.AddListener(() => ToggleButton(button));

            TMP_Text tmpText = button.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                buttonTextMap[button] = tmpText;
            }
        }
    }

    public void ToggleButton(Button clickedButton)
    {
        if (!buttons.Contains(clickedButton)) return;

        Image clickedImage = clickedButton.GetComponent<Image>();
        TMP_Text clickedText = buttonTextMap.ContainsKey(clickedButton) ? buttonTextMap[clickedButton] : null;

        if (clickedImage == null) return;

        // 기존 버튼 복구
        RestorePreviousButton();

        // 같은 버튼을 다시 눌렀다면 비활성화 (팝업 즉시 숨기기)
        if (activeButton == clickedButton)
        {
            ResetButton(clickedImage, clickedText);
            HidePopupInstantly();
            return;
        }

        // 새 버튼 활성화
        ActivateNewButton(clickedButton, clickedImage, clickedText);

        // 첫 번째 버튼이면 팝업 표시
        if (clickedButton == buttons[0])
        {
            ShowPopup();
        }
        else
        {
            HidePopupInstantly(); // 다른 버튼을 누르면 팝업 즉시 사라짐
        }
    }

    private void RestorePreviousButton()
    {
        if (activeButton == null) return;

        Image activeImage = activeButton.GetComponent<Image>();
        TMP_Text activeText = buttonTextMap.ContainsKey(activeButton) ? buttonTextMap[activeButton] : null;

        if (activeImage != null)
        {
            activeImage.sprite = previousSprite;
            activeImage.SetNativeSize();
        }

        if (activeText != null)
        {
            activeText.gameObject.SetActive(true);
        }

        HidePopupInstantly(); // 기존 버튼이 비활성화되면 팝업 즉시 숨김
    }

    private void ResetButton(Image clickedImage, TMP_Text clickedText)
    {
        if (previousSprite != null)
        {
            clickedImage.sprite = previousSprite;
            clickedImage.SetNativeSize();
        }

        if (clickedText != null)
        {
            clickedText.gameObject.SetActive(true);
        }

        activeButton = null;
    }

    private void ActivateNewButton(Button clickedButton, Image clickedImage, TMP_Text clickedText)
    {
        previousSprite = clickedImage.sprite;
        clickedImage.sprite = xButtonSprite;
        clickedImage.SetNativeSize();

        if (clickedText != null)
        {
            clickedText.gameObject.SetActive(false);
        }

        activeButton = clickedButton;
    }

    private void ShowPopup()
    {
        if (popupPanel == null) return;

        popupPanel.SetActive(true);

        RectTransform popupTransform = popupPanel.GetComponent<RectTransform>();
        popupTransform.anchoredPosition = new Vector2(0, popupStartOffset); // 시작 위치 설정

        // 부드럽게 위로 이동 + 튕기는 애니메이션
        popupTransform.DOAnchorPosY(0, popupSpeed)
            .SetEase(Ease.OutBack, bounceStrength);
    }

    private void HidePopupInstantly()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false); // 즉시 비활성화
        }
    }
}