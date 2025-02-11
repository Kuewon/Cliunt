using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class BackgroundScroller : MonoBehaviour
{
    public static BackgroundScroller Instance { get; private set; }

    [SerializeField] private RectTransform backgroundImage1;
    [SerializeField] private RectTransform backgroundImage2;
    private float scrollDuration = 2.5f;

    private float backgroundWidth;
    private bool isScrolling = false;
    private Animator characterAnimator;
    private bool isImage1Active = true;  // 현재 화면에 보이는 배경이 어떤 것인지 추적

    public event Action OnScrollComplete;
    public event Action<float> OnScrollUpdate;

    public bool IsScrolling => isScrolling;
    
    public float GetScrollAmount() => backgroundWidth;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            characterAnimator = player.GetComponent<Animator>();
        }
    }

    private void Start()
    {
        if (backgroundImage1 == null || backgroundImage2 == null)
        {
            Debug.LogError("Background images not assigned!");
            return;
        }
    
        backgroundWidth = backgroundImage1.rect.width;
        SetupBackgrounds();

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveCompleted += OnWaveCompleted;
        }

        StartCoroutine(ScrollBackgrounds());
    }

    private void SetupBackgrounds()
    {
        // 첫 번째 배경은 화면 시작점에
        backgroundImage1.anchoredPosition = Vector2.zero;
        // 두 번째 배경은 첫 번째 배경 바로 오른쪽에
        backgroundImage2.anchoredPosition = new Vector2(backgroundWidth, 0);
        isImage1Active = true;  // 초기 상태 설정
    }

    public void OnWaveCompleted()
    {
        if (!isScrolling)
        {
            PrepareNextBackground();
            
            if (WaveMovementController.Instance != null)
            {
                WaveMovementController.Instance.ResetWaveMovement();
            }
            StartCoroutine(ScrollBackgrounds());
        }
    }

    private void PrepareNextBackground()
    {
        // 현재 활성화된 배경에 따라 다음 배경 준비
        if (isImage1Active)
        {
            // Image1이 활성화되어 있을 때 (화면에 보일 때)
            backgroundImage2.anchoredPosition = new Vector2(backgroundWidth, 0);
            backgroundImage1.anchoredPosition = Vector2.zero;
        }
        else
        {
            // Image2가 활성화되어 있을 때
            backgroundImage1.anchoredPosition = new Vector2(backgroundWidth, 0);
            backgroundImage2.anchoredPosition = Vector2.zero;
        }
    }

    private IEnumerator ScrollBackgrounds()
    {
        isScrolling = true;
    
        if (characterAnimator != null)
        {
            characterAnimator.SetBool("IsWalking", true);
        }

        RectTransform currentBg = isImage1Active ? backgroundImage1 : backgroundImage2;
        RectTransform nextBg = isImage1Active ? backgroundImage2 : backgroundImage1;

        Vector2 currentStartPos = currentBg.anchoredPosition;
        Vector2 nextStartPos = nextBg.anchoredPosition;
    
        Vector2 currentTargetPos = currentStartPos + Vector2.left * backgroundWidth;
        Vector2 nextTargetPos = nextStartPos + Vector2.left * backgroundWidth;

        float elapsedTime = 0f;
        while (elapsedTime < scrollDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / scrollDuration;
        
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
        
            currentBg.anchoredPosition = Vector2.Lerp(currentStartPos, currentTargetPos, smoothT);
            nextBg.anchoredPosition = Vector2.Lerp(nextStartPos, nextTargetPos, smoothT);

            OnScrollUpdate?.Invoke(smoothT);

            yield return null;
        }

        // 정확한 위치로 설정
        currentBg.anchoredPosition = currentTargetPos;
        nextBg.anchoredPosition = nextTargetPos;

        // 활성 배경 전환
        isImage1Active = !isImage1Active;
    
        if (characterAnimator != null)
        {
            characterAnimator.SetBool("IsWalking", false);
        }

        isScrolling = false;
        OnScrollComplete?.Invoke();
    }
    
    private void OnDestroy()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveCompleted -= OnWaveCompleted;
        }
    }
}