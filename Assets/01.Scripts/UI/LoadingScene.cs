using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScene : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Slider loadingBarSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Image companyLogo;

    [Header("Scene Transition")]
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private string animationTriggerName = "Play";
    private CanvasGroup transitionCanvasGroup;
    private bool transitionCompleted = false; // ✅ 애니메이션 완료 여부 체크

    [Header("Loading Settings")]
    [SerializeField] private float minimumLoadingTime = 2f;
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color endColor = Color.green;

    [Header("Logo Animation")]
    [SerializeField] private float logoFadeInTime = 1f;
    [SerializeField] private float logoShowTime = 2f;

    private float currentProgress = 0f;
    private float targetProgress = 0f;
    private float smoothSpeed = 5f;
    private bool canTransition = false;
    private bool isDataLoaded = false;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog = true;

    private void LogDebug(string message)
    {
        if (showDebugLog)
            Debug.Log(message);
    }

    private void Awake()
    {
        if (transitionAnimator)
        {
            transitionCanvasGroup = transitionAnimator.GetComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        InitializeUI();
        StartCoroutine(LoadSequence());
    }

    private void InitializeUI()
    {
        if (loadingBarSlider) loadingBarSlider.value = 0f;
        if (fillImage) fillImage.color = startColor;
        if (companyLogo) companyLogo.color = new Color(1, 1, 1, 0);
        if (progressText) progressText.text = "샌디직원 연봉 협상 초기화 중...";
    }

    private void Update()
    {
        currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * smoothSpeed);

        if (loadingBarSlider)
            loadingBarSlider.value = currentProgress;

        if (fillImage)
            fillImage.color = Color.Lerp(startColor, endColor, currentProgress);
    }

    private IEnumerator LoadSequence()
    {
        // float startTime = Time.time; // 디버그 로그용

        // ✅ 로비 씬 비동기 로드 시작
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Lobby");
        asyncLoad.allowSceneActivation = false;
    
        // ✅ 로고 페이드인 및 표시
        yield return StartCoroutine(LogoFadeIn());
        yield return new WaitForSeconds(logoShowTime);

        // ✅ 진행률 30% 업데이트
        UpdateProgress(0.3f);
        GoogleSheetsManager.OnDataLoadComplete += OnDataLoaded;

        // ✅ 데이터 로드 및 씬 로드 진행
        float progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime / minimumLoadingTime;
            UpdateProgress(Mathf.Lerp(0.3f, 0.9f, progress));

            if (progressText)
                progressText.text = progress < 0.5f
                    ? $"게임 데이터 로딩 중... ({Mathf.Min(progress * 100, 100):F0}%)"
                    : $"로비 불러오는 중... ({Mathf.Min(progress * 100, 100):F0}%)";

            yield return null;
        }

        // float elapsedTime = Time.time - startTime; // 디버그 로그용
        // if (elapsedTime < minimumLoadingTime)
        // {
        //     yield return new WaitForSeconds(minimumLoadingTime - elapsedTime);
        // }

        // ✅ 최종적으로 진행률 100%로 고정
        UpdateProgress(1f);
        if (progressText) progressText.text = "로딩 완료! (100%)";
        yield return new WaitForSeconds(0.5f);

        // ✅ 트랜지션 애니메이션 시작
        canTransition = true;
        if (transitionAnimator && transitionCanvasGroup)
        {
            transitionCanvasGroup.alpha = 1f;
            transitionAnimator.gameObject.SetActive(true);
            transitionAnimator.SetTrigger(animationTriggerName);
        }

        // ✅ 애니메이션이 끝날 때까지 대기
        while (!transitionCompleted)
        {
            yield return null;
        }

        // ✅ 로비 씬으로 이동
        asyncLoad.allowSceneActivation = true;
    }

    // ✅ 애니메이션이 끝날 때 Unity 애니메이션 이벤트를 통해 호출됨
    public void OnTransitionAnimationComplete()
    {
        LogDebug("✅ 트랜지션 애니메이션 완료");
        transitionCompleted = true;
    }

    private void OnDataLoaded()
    {
        Debug.Log("OnDataLoaded 이벤트 발생");
        isDataLoaded = true;
        GoogleSheetsManager.OnDataLoadComplete -= OnDataLoaded;
    }

    private IEnumerator LogoFadeIn()
    {
        if (!companyLogo) yield break;

        float elapsed = 0f;
        while (elapsed < logoFadeInTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / logoFadeInTime);
            companyLogo.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }

    private void UpdateProgress(float progress)
    {
        targetProgress = Mathf.Clamp01(progress);
    }

    private void OnDestroy()
    {
        GoogleSheetsManager.OnDataLoadComplete -= OnDataLoaded;
    }
}