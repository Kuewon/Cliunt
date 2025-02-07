using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    public Button newGameButton;   // 신규 유저 버튼 (카툰 씬으로 이동)
    public Button continueButton;  // 기존 유저 버튼 (인게임 씬으로 이동)

    private void Start()
    {
        // 버튼이 Inspector에서 설정되지 않았다면 오류 로그 출력 후 종료
        if (newGameButton == null || continueButton == null)
        {
            Debug.LogError("❌ `newGameButton` 또는 `continueButton`이 Unity Inspector에서 할당되지 않았습니다!");
            return;
        }

        // 시작 시 버튼 숨김
        newGameButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);

        // 데이터 로드 완료 후 버튼을 활성화
        GoogleSheetsManager.OnDataLoadComplete += OnDataLoadComplete;
        UserDataManager.OnUserDataProcessed += EnableButtons;
    }

    private void OnDataLoadComplete()
    {
        Debug.Log("✅ Google 스프레드시트 데이터 로드 완료. 유저 데이터 검증 시작...");
    }

    private void EnableButtons(bool isNewUser)
    {
        if (newGameButton == null || continueButton == null)
        {
            Debug.LogError("❌ `newGameButton` 또는 `continueButton`이 제거되었거나 할당되지 않았습니다!");
            return;
        }

        if (isNewUser)
        {
            Debug.Log("🚀 신규 유저입니다. New Game 버튼 활성화.");
            newGameButton.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("✅ 기존 유저 데이터가 존재합니다. Continue 버튼 활성화.");
            continueButton.gameObject.SetActive(true);
        }
    }

    public void OnNewGameButtonClick()
    {
        Debug.Log("🆕 신규 게임 시작! CartoonScene으로 이동.");
        SceneManager.LoadScene("Cartoon");
    }

    public void OnContinueButtonClick()
    {
        Debug.Log("▶ 기존 유저 계속 진행! InGameScene으로 이동.");
        SceneManager.LoadScene("Ingame");
    }

    private void OnDestroy()
    {
        GoogleSheetsManager.OnDataLoadComplete -= OnDataLoadComplete;
        UserDataManager.OnUserDataProcessed -= EnableButtons;
    }
}