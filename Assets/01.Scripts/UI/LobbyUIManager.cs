using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;


public class LobbyUIManager : MonoBehaviour
{
    public Button newGameButton; // 신규 유저 버튼 (카툰 씬으로 이동)
    public Button continueButton; // 기존 유저 버튼 (인게임 씬으로 이동)
    
    
    [System.Serializable]
    public class FloatingObject
    {
        public Transform target; // 움직일 대상
        [Range(0f, 1f)] public float moveFactor = 0.2f; // 이동 비율 (기본값: 20%)
        public float duration = 1.5f; // 이동 시간
        public float delay = 0f; // 시작 딜레이
    }

    [Header("개별 움직임 설정")]
    [SerializeField] private FloatingObject player;
    [SerializeField] private FloatingObject cloud1;
    [SerializeField] private FloatingObject cloud2;
    [SerializeField] private FloatingObject cloud3;
    
    [System.Serializable]
    public class PuddingObject
    {
        public Transform target; // 푸딩 오브젝트
        [Range(0f, 1f)] public float scaleFactor = 0.2f; // 원래 크기 대비 크기 변화율 (예: 0.2 → 20% 확대)
        public float bounceDuration = 0.3f; // 뽀잉 지속 시간
        public float delay = 0f; // 시작 딜레이
    }

    [Header("푸딩 개별 설정")]
    [SerializeField] private PuddingObject pudding1;
    [SerializeField] private PuddingObject pudding2;
    [SerializeField] private PuddingObject pudding3;
    [SerializeField] private PuddingObject pudding4;
    [SerializeField] private PuddingObject pudding5;



    private void Start()
    {
        AnimateObject(player);
        AnimateObject(cloud1);
        AnimateObject(cloud2);
        AnimateObject(cloud3);
        
        AnimatePudding(pudding1);
        AnimatePudding(pudding2);
        AnimatePudding(pudding3);
        AnimatePudding(pudding4);
        AnimatePudding(pudding5);
        
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

    private void AnimateObject(FloatingObject obj)
    {
        if (obj.target == null) return; // 오브젝트가 없으면 실행하지 않음

        float startY = obj.target.position.y; // 현재 위치 저장
        float moveDistance = startY * obj.moveFactor; // 현재 위치의 비율만큼 이동

        obj.target.DOMoveY(startY + moveDistance, obj.duration)
            .SetEase(Ease.InOutSine) // 부드러운 움직임
            .SetLoops(-1, LoopType.Yoyo) // 무한 반복
            .SetDelay(obj.delay); // 딜레이 추가
    }
    
    private void AnimatePudding(PuddingObject obj)
    {
        if (obj.target == null) return;

        Vector3 originalScale = obj.target.localScale; // 원래 크기 저장
        Vector3 bounceScale = originalScale * (1f + obj.scaleFactor); // 확대된 크기

        Sequence bounceSequence = DOTween.Sequence()
            .Append(obj.target.DOScale(bounceScale, obj.bounceDuration * 0.5f)
                .SetEase(Ease.OutBack)) // 튀어오를 때 부드러운 확장
            .Append(obj.target.DOScale(originalScale, obj.bounceDuration * 0.5f)
                .SetEase(Ease.InQuad)) // 원래 크기로 복귀 (살짝 빠르게)
            .SetLoops(-1) // 무한 반복
            .SetDelay(obj.delay); // 시작 딜레이 추가
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