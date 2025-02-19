using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CartoonManager : MonoBehaviour
{
    [SerializeField] private Button goToInGameButton; // 인게임으로 가는 버튼

    private void Start()
    {
        if (goToInGameButton != null)
            goToInGameButton.onClick.AddListener(GoToInGame);
        else
            Debug.LogError("❌ `goToInGameButton` 버튼이 할당되지 않았습니다! Unity에서 연결하세요.");
    }

    private void GoToInGame()
    {
        SceneManager.LoadScene("Ingame"); // 버튼 클릭 시 Ingame 씬으로 이동
    }
}