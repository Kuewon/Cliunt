using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using DG.Tweening;

public class BottomUIButtonsManager : MonoBehaviour
{
    [Header("Common UI Elements")] [SerializeField]
    private List<Button> bottomButtons = new List<Button>();

    [SerializeField] private Sprite closeButtonSprite;

    [Header("Upgrade Panel Settings")] [SerializeField]
    private GameObject upgradePanel;

    [SerializeField] private float upgradePanelSpeed = 0.5f;
    [SerializeField] private float upgradePanelStartOffset = -600f;

    [Header("Weapon Upgrade Animation")] [SerializeField]
    private GameObject weaponBottomImage;

    [SerializeField] private GameObject weaponTopImage;
    [SerializeField] private GameObject loadingSpinner;
    [SerializeField] private GameObject weaponUpgradePanel;
    [SerializeField] private float weaponAnimationSpeed = 0.5f;
    [SerializeField] private float bottomImageStartOffset = -300f;
    [SerializeField] private float topImageStartOffset = 300f;
    [SerializeField] private float weaponCollisionOffset = 30f;
    [SerializeField] private float weaponBounceStrength = 50f;
    [SerializeField] private float weaponBounceDuration = 0.2f;

    [Header("Loading Spinner Settings")] [SerializeField]
    private float spinnerInitialScale = 5f;

    [SerializeField] private float spinnerScaleTime = 0.3f;
    [SerializeField] private float spinnerDelay = 0.15f;
    [SerializeField] private GameObject revolverSpinnerImage;
    [SerializeField] private GameObject cylinderSpinnerImage;

    [Header("Weapon Tab Toggle")] [SerializeField]
    private List<Button> tabButtons = new List<Button>();

    [SerializeField] private GameObject revolverTabImage;
    [SerializeField] private GameObject cylinderTabImage;
    [SerializeField] private GameObject bulletTabImage;
    [SerializeField] private float tabAnimationDuration = 0.3f;
    [SerializeField] private float tabSlideDistance = 100f;

    private Button currentActiveButton = null;
    private Sprite previousButtonSprite = null;
    private Dictionary<Button, TMP_Text> buttonToTextMap = new Dictionary<Button, TMP_Text>();
    private bool isAnimationPlaying = false;
    private int currentTabIndex = 0;
    
    [Header("Tab Info Box Settings")]
    [SerializeField] private Transform[] tabInfoBoxParents;  // 각 탭별 InfoBox 부모 오브젝트
    [SerializeField] private float infoBoxRotateAmount = 15f;
    [SerializeField] private float infoBoxRotateDuration = 0.4f;
    [SerializeField] private float infoBoxReturnDelay = 0.1f;
    private GameObject[] tabImages;

    private class TabSequence
    {
        public int previousTabIndex { get; private set; }
        public int currentTabIndex { get; private set; }

        public void UpdateSequence(int newIndex)
        {
            previousTabIndex = currentTabIndex;
            currentTabIndex = newIndex;
        }

        public void Reset()
        {
            previousTabIndex = 0;
            currentTabIndex = 0;
        }
    }

    private TabSequence tabSequence;

    private void Start()
    {
        InitializeUIButtons();
        ResetAllPanels();
        InitializeWeaponTab();
        isAnimationPlaying = false;
        tabSequence = new TabSequence();
    }

    private void InitializeUIButtons()
    {
        foreach (var button in bottomButtons)
        {
            button.onClick.AddListener(() => OnBottomButtonClick(button));
            var buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonToTextMap[button] = buttonText;
        }
    }

    private void InitializeWeaponTab()
    {
        tabImages = new GameObject[] { revolverTabImage, cylinderTabImage, bulletTabImage };

        currentTabIndex = 0;
        for (int i = 0; i < tabImages.Length; i++)
        {
            if (i == 0)
            {
                tabImages[i].SetActive(true);
                tabImages[i].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            else
            {
                tabImages[i].SetActive(false);
                tabImages[i].GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(tabSlideDistance, tabImages[i].GetComponent<RectTransform>().anchoredPosition.y);
            }
        }

        for (int i = 0; i < tabButtons.Count; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => OnTabButtonClick(index));
        }
    }

    private void OnTabButtonClick(int targetIndex)
    {
        if (isAnimationPlaying || targetIndex == currentTabIndex) return;
        isAnimationPlaying = true;

        tabSequence.UpdateSequence(targetIndex);
        UpdateTabButtonStates(targetIndex);

        GameObject currentTab = tabImages[currentTabIndex];
        GameObject targetTab = tabImages[targetIndex];
        RectTransform currentTransform = currentTab.GetComponent<RectTransform>();
        RectTransform targetTransform = targetTab.GetComponent<RectTransform>();

        float direction = Mathf.Sign(targetIndex - currentTabIndex);

        // InfoBox 애니메이션 실행 및 Sequence 저장
        Sequence infoBoxSequence = AnimateTabInfoBoxes(currentTabIndex, targetIndex);

        targetTab.SetActive(true);
        targetTransform.anchoredPosition = new Vector2(direction * tabSlideDistance, targetTransform.anchoredPosition.y);

        if (weaponUpgradePanel.activeSelf)
        {
            int tempIndex = currentTabIndex;
            currentTabIndex = targetIndex;
            UpdateSpinnerImage();
            currentTabIndex = tempIndex;
        }

        // 메인 탭 전환 애니메이션
        Sequence mainSequence = DOTween.Sequence()
            .Append(currentTransform.DOAnchorPosX(-direction * tabSlideDistance, tabAnimationDuration).SetEase(Ease.OutQuad))
            .Join(targetTransform.DOAnchorPosX(0, tabAnimationDuration).SetEase(Ease.OutQuad));

        // 두 애니메이션 시퀀스를 하나로 합치기
        DOTween.Sequence()
            .Append(mainSequence)
            .Join(infoBoxSequence)
            .OnComplete(() =>
            {
                currentTab.SetActive(false);
                currentTabIndex = targetIndex;
                isAnimationPlaying = false;
            });
    }

    private void OnBottomButtonClick(Button clickedButton)
    {
        if (!bottomButtons.Contains(clickedButton)) return;

        if (currentActiveButton == clickedButton)
        {
            ResetButton(clickedButton);
            ResetAllPanels();
            return;
        }

        ResetAllPanels();
        ActivateButton(clickedButton);

        if (clickedButton == bottomButtons[0]) ShowUpgradePanel();
        else if (clickedButton == bottomButtons[1]) PlayWeaponAnimation();
    }

    private void ResetAllPanels()
    {
        upgradePanel?.SetActive(false);
        weaponUpgradePanel?.SetActive(false);
        weaponBottomImage?.SetActive(false);
        weaponTopImage?.SetActive(false);
        loadingSpinner?.SetActive(false);
        revolverSpinnerImage?.SetActive(false);
        cylinderSpinnerImage?.SetActive(false);

        tabSequence?.Reset();
        ResetTabState();

        if (currentActiveButton != null)
        {
            ResetButton(currentActiveButton);
            currentActiveButton = null;
        }
    }

    private void ResetTabState()
    {
        if (tabImages == null || tabImages.Length == 0) return;

        for (int i = 0; i < tabImages.Length; i++)
        {
            tabImages[i].SetActive(false);
            RectTransform tabTransform = tabImages[i].GetComponent<RectTransform>();
            tabTransform.anchoredPosition = new Vector2(tabSlideDistance, tabTransform.anchoredPosition.y);
        }

        currentTabIndex = 0;
        tabImages[currentTabIndex].SetActive(true);
        tabImages[currentTabIndex].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        // ✅ 팝업이 닫혔다가 다시 열릴 때 버튼 상태도 초기화
        UpdateTabButtonStates(currentTabIndex);
    }

    private void ResetButton(Button button)
    {
        if (button.TryGetComponent(out Image buttonImage) && previousButtonSprite != null)
        {
            buttonImage.sprite = previousButtonSprite;
            buttonImage.SetNativeSize();
        }

        if (buttonToTextMap.TryGetValue(button, out TMP_Text buttonText))
            buttonText.gameObject.SetActive(true);
    }

    private void ActivateButton(Button button)
    {
        if (button.TryGetComponent(out Image buttonImage))
        {
            previousButtonSprite = buttonImage.sprite;
            buttonImage.sprite = closeButtonSprite;
            buttonImage.SetNativeSize();
        }

        if (buttonToTextMap.TryGetValue(button, out TMP_Text buttonText))
            buttonText.gameObject.SetActive(false);

        currentActiveButton = button;
    }

    private void ShowUpgradePanel()
    {
        if (upgradePanel == null) return;

        upgradePanel.SetActive(true);
        RectTransform panelTransform = upgradePanel.GetComponent<RectTransform>();
        panelTransform.anchoredPosition = new Vector2(0, upgradePanelStartOffset);
        panelTransform.DOAnchorPosY(0, upgradePanelSpeed).SetEase(Ease.OutBack);

        // ✅ 팝업이 열릴 때 항상 첫 번째 탭 버튼 강조
        UpdateTabButtonStates(currentTabIndex);
    }

    private void PlayWeaponAnimation()
    {
        if (weaponBottomImage == null || weaponTopImage == null || loadingSpinner == null ||
            weaponUpgradePanel == null) return;

        bool isInitialOpen = !weaponUpgradePanel.activeSelf;
        weaponUpgradePanel.SetActive(true);

        // 팝업 최초 오픈 시에만 로딩 스피너 활성화
        if (isInitialOpen)
        {
            loadingSpinner.SetActive(true);
            StartWeaponCollisionAnimation();
        }

        DOVirtual.DelayedCall(spinnerDelay, () => UpdateSpinnerImage(isInitialOpen));
    }

    private void StartWeaponCollisionAnimation()
    {
        weaponBottomImage.SetActive(true);
        weaponTopImage.SetActive(true);

        RectTransform bottomTransform = weaponBottomImage.GetComponent<RectTransform>();
        RectTransform topTransform = weaponTopImage.GetComponent<RectTransform>();

        bottomTransform.anchoredPosition = new Vector2(0, bottomImageStartOffset);
        topTransform.anchoredPosition = new Vector2(0, topImageStartOffset);

        bottomTransform.DOAnchorPosY(weaponCollisionOffset, weaponAnimationSpeed).SetEase(Ease.OutQuad);
        topTransform.DOAnchorPosY(-weaponCollisionOffset, weaponAnimationSpeed)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => PlayWeaponBounceEffect(bottomTransform, topTransform));
    }

    private void PlayWeaponBounceEffect(RectTransform bottomTransform, RectTransform topTransform)
    {
        Sequence bounceSequence = DOTween.Sequence();

        float[] bounceStrengths = { weaponBounceStrength, weaponBounceStrength * 0.6f, weaponBounceStrength * 0.3f };
        float[] bounceDurations = { weaponBounceDuration, weaponBounceDuration * 0.7f, weaponBounceDuration * 0.5f };

        for (int i = 0; i < bounceStrengths.Length; i++)
        {
            float strength = bounceStrengths[i];
            float duration = bounceDurations[i];

            bounceSequence.Append(bottomTransform.DOAnchorPosY(weaponCollisionOffset - strength, duration)
                .SetEase(Ease.OutBack));
            bounceSequence.Join(topTransform.DOAnchorPosY(-weaponCollisionOffset + strength, duration)
                .SetEase(Ease.OutBack));

            bounceSequence.Append(bottomTransform.DOAnchorPosY(weaponCollisionOffset, duration * 0.8f)
                .SetEase(Ease.InOutQuad));
            bounceSequence.Join(topTransform.DOAnchorPosY(-weaponCollisionOffset, duration * 0.8f)
                .SetEase(Ease.InOutQuad));
        }
    }

    private void UpdateSpinnerImage(bool isInitialOpen = false)
    {
        if (loadingSpinner == null) return;

        // 리볼버 탭인 경우
        if (currentTabIndex == 0)
        {
            revolverSpinnerImage.SetActive(true);
            cylinderSpinnerImage.SetActive(false);

            if (tabSequence.previousTabIndex != 0 || isInitialOpen)
            {
                PlaySpinnerAnimation();
            }
        }
        // 실린더 또는 불릿 탭의 경우
        else
        {
            revolverSpinnerImage.SetActive(false);
            cylinderSpinnerImage.SetActive(true);

            // 리볼버에서 전환되는 경우나 최초 오픈인 경우에만 애니메이션 실행
            if (tabSequence.previousTabIndex == 0 || isInitialOpen)
            {
                PlaySpinnerAnimation();
            }
        }
    }

    private void PlaySpinnerAnimation()
    {
        loadingSpinner.transform.localScale = Vector3.one * spinnerInitialScale;
        loadingSpinner.transform.DOScale(Vector3.one, spinnerScaleTime)
            .SetEase(Ease.OutQuad);
    }

    private void UpdateTabButtonStates(int activeIndex)
    {
        for (int i = 0; i < tabButtons.Count; i++)
        {
            bool isActive = (i == activeIndex);

            tabButtons[i].GetComponent<Image>().DOColor(isActive ? Color.white : new Color(0.7f, 0.7f, 0.7f), 0.2f);
            tabButtons[i].transform.DOScale(isActive ? 1.2f : 1.0f, 0.2f).SetEase(Ease.OutBack);
        }
    }
    
    private Sequence AnimateTabInfoBoxes(int fromIndex, int toIndex)
    {
        Sequence masterSequence = DOTween.Sequence();
    
        if (tabInfoBoxParents == null) return masterSequence;

        float direction = Mathf.Sign(toIndex - fromIndex);

        // 현재 탭의 InfoBox들 애니메이션
        if (fromIndex < tabInfoBoxParents.Length && tabInfoBoxParents[fromIndex] != null)
        {
            foreach (Transform infoBox in tabInfoBoxParents[fromIndex])
            {
                masterSequence.Join(AnimateInfoBox(infoBox, direction));
            }
        }

        // 이동할 탭의 InfoBox들 애니메이션
        if (toIndex < tabInfoBoxParents.Length && tabInfoBoxParents[toIndex] != null)
        {
            foreach (Transform infoBox in tabInfoBoxParents[toIndex])
            {
                masterSequence.Join(AnimateInfoBox(infoBox, -direction));
            }
        }

        return masterSequence;
    }
    
    private Sequence AnimateInfoBox(Transform infoBox, float direction)
    {
        float targetRotation = -direction * infoBoxRotateAmount;
    
        Sequence infoBoxSequence = DOTween.Sequence();
    
        infoBoxSequence.Append(
            infoBox.DOLocalRotate(
                    new Vector3(0, 0, targetRotation), 
                    infoBoxRotateDuration * 0.5f)
                .SetEase(Ease.OutQuad)
        );
    
        infoBoxSequence.AppendInterval(infoBoxReturnDelay);
    
        infoBoxSequence.Append(
            infoBox.DOLocalRotate(
                    Vector3.zero, 
                    infoBoxRotateDuration)
                .SetEase(Ease.OutElastic, 0.5f)
        );

        return infoBoxSequence;
    }
}