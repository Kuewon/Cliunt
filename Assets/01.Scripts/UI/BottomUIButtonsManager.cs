using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BottomUIButtonsManager : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private List<Button> buttons = new List<Button>(); // 버튼 리스트
    [SerializeField] private Sprite xButtonSprite; // X 버튼 이미지

    private Button activeButton = null; // 현재 활성화된 버튼
    private Sprite previousSprite = null; // 원래 버튼 이미지 저장
    private Dictionary<Button, TMP_Text> buttonTextMap = new Dictionary<Button, TMP_Text>(); // 버튼과 텍스트 매핑

    private void Start()
    {
        // 버튼 클릭 이벤트 추가 + 버튼과 TMP_Text 연결
        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i; // 람다 캡처 방지
            Button button = buttons[i];
            button.onClick.AddListener(() => ToggleButton(index));

            // 버튼 내부의 TMP_Text 찾기
            TMP_Text tmpText = button.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                buttonTextMap[button] = tmpText; // 버튼과 텍스트 매핑 저장
            }
        }
    }

    public void ToggleButton(int index)
    {
        if (index < 0 || index >= buttons.Count)
        {
            Debug.LogError($"❌ 인덱스 범위 초과: {index} (버튼 개수: {buttons.Count})");
            return;
        }

        Button clickedButton = buttons[index];
        Image clickedImage = clickedButton.GetComponent<Image>(); // 버튼의 Image 가져오기
        TMP_Text clickedText = buttonTextMap.ContainsKey(clickedButton) ? buttonTextMap[clickedButton] : null; // TMP_Text 가져오기

        if (clickedImage == null)
        {
            Debug.LogError($"❌ 버튼 {index}에 Image 컴포넌트가 없습니다!");
            return;
        }

        Debug.Log($"✅ 버튼 {index} 클릭! 현재 이미지: {clickedImage.sprite.name}");

        // **1. 기존 활성화된 버튼이 있으면 원래 상태로 복구**
        if (activeButton != null && activeButton != clickedButton)
        {
            Image activeImage = activeButton.GetComponent<Image>();
            TMP_Text oldText = buttonTextMap.ContainsKey(activeButton) ? buttonTextMap[activeButton] : null;

            if (activeImage != null)
            {
                activeImage.sprite = previousSprite; // 기존 버튼 원래 이미지로 변경
                activeImage.SetNativeSize();
                Debug.Log($"🔹 버튼 {buttons.IndexOf(activeButton)} 원래 이미지로 복구됨.");
            }

            if (oldText != null)
            {
                oldText.gameObject.SetActive(true); // 기존 버튼의 TMP 텍스트 다시 표시
                Debug.Log($"🔹 버튼 {buttons.IndexOf(activeButton)} 텍스트 다시 활성화됨.");
            }
        }

        // **2. 같은 버튼을 다시 눌렀다면 비활성화 (X → 원래 버튼 이미지, 텍스트 복구)**
        if (activeButton == clickedButton)
        {
            if (previousSprite != null)
            {
                clickedImage.sprite = previousSprite; // 원래 이미지로 변경
                clickedImage.SetNativeSize();
            }
            if (clickedText != null) clickedText.gameObject.SetActive(true); // TMP 텍스트 다시 활성화

            Debug.Log($"🔹 버튼 {index} 비활성화됨. 원래 이미지: {previousSprite.name}");
            activeButton = null;
        }
        else
        {
            // **3. 새로운 버튼 활성화 (X 버튼 & 텍스트 숨김)**
            previousSprite = clickedImage.sprite; // 기존 버튼 이미지 저장
            clickedImage.sprite = xButtonSprite; // X 버튼으로 변경
            clickedImage.SetNativeSize();
            activeButton = clickedButton;

            if (clickedText != null)
            {
                clickedText.gameObject.SetActive(false); // TMP 텍스트 숨김
            }

            Debug.Log($"✅ 버튼 {index} 활성화됨. 새로운 이미지: {xButtonSprite.name}");
        }
    }
}