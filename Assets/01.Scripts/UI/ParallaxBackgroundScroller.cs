using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ParallaxBackgroundScroller : MonoBehaviour
{
    public static ParallaxBackgroundScroller Instance { get; private set; }

    [System.Serializable]
    public class BackgroundLayer
    {
        public RectTransform image1;
        public RectTransform image2;
        [Range(0f, 1f)]
        public float scrollSpeed = 1f; // 1이 가장 빠름, 0에 가까울수록 느림
    }

    [SerializeField] private BackgroundLayer[] backgroundLayers;
    private float scrollDuration = 2.5f;
    private float[] layerWidths;
    private bool[] isImage1Active;
    private bool isScrolling = false;

    public event Action OnScrollComplete;
    public event Action<float> OnScrollUpdate;

    public bool IsScrolling => isScrolling;

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

        // 배경 레이어 초기화
        layerWidths = new float[backgroundLayers.Length];
        isImage1Active = new bool[backgroundLayers.Length];
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            isImage1Active[i] = true;
        }
    }

    private void Start()
    {
        ValidateBackgrounds();
        InitializeBackgrounds();

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveCompleted += OnWaveCompleted;
        }
    }

    private void ValidateBackgrounds()
    {
        if (backgroundLayers == null || backgroundLayers.Length == 0)
        {
            Debug.LogError("No background layers assigned!");
            return;
        }

        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            if (backgroundLayers[i].image1 == null || backgroundLayers[i].image2 == null)
            {
                Debug.LogError($"Background images not assigned for layer {i}!");
                return;
            }
            layerWidths[i] = backgroundLayers[i].image1.rect.width;
        }
    }

    private void InitializeBackgrounds()
    {
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            // 첫 번째 이미지는 시작점에
            backgroundLayers[i].image1.anchoredPosition = Vector2.zero;
            // 두 번째 이미지는 첫 번째 이미지 바로 오른쪽에
            backgroundLayers[i].image2.anchoredPosition = new Vector2(layerWidths[i], 0);
        }
    }

    public void OnWaveCompleted()
    {
        if (!isScrolling)
        {
            PrepareNextBackgrounds();

            if (WaveMovementController.Instance != null)
            {
                WaveMovementController.Instance.ResetWaveMovement();
            }
            StartCoroutine(ScrollBackgrounds());
        }
    }

    private void PrepareNextBackgrounds()
    {
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            if (isImage1Active[i])
            {
                backgroundLayers[i].image2.anchoredPosition = new Vector2(layerWidths[i], 0);
                backgroundLayers[i].image1.anchoredPosition = Vector2.zero;
            }
            else
            {
                backgroundLayers[i].image1.anchoredPosition = new Vector2(layerWidths[i], 0);
                backgroundLayers[i].image2.anchoredPosition = Vector2.zero;
            }
        }
    }

    private IEnumerator ScrollBackgrounds()
    {
        isScrolling = true;
        float elapsedTime = 0f;

        // 각 레이어의 시작 위치와 목표 위치 저장
        Vector2[] currentStartPositions = new Vector2[backgroundLayers.Length];
        Vector2[] nextStartPositions = new Vector2[backgroundLayers.Length];
        Vector2[] currentTargetPositions = new Vector2[backgroundLayers.Length];
        Vector2[] nextTargetPositions = new Vector2[backgroundLayers.Length];

        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            RectTransform currentBg = isImage1Active[i] ? backgroundLayers[i].image1 : backgroundLayers[i].image2;
            RectTransform nextBg = isImage1Active[i] ? backgroundLayers[i].image2 : backgroundLayers[i].image1;

            currentStartPositions[i] = currentBg.anchoredPosition;
            nextStartPositions[i] = nextBg.anchoredPosition;
            currentTargetPositions[i] = currentStartPositions[i] + Vector2.left * layerWidths[i];
            nextTargetPositions[i] = nextStartPositions[i] + Vector2.left * layerWidths[i];
        }

        while (elapsedTime < scrollDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / scrollDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < backgroundLayers.Length; i++)
            {
                float speed = backgroundLayers[i].scrollSpeed;

                // 패럴랙스 효과를 위한 보간 계산
                // speed가 1일 때는 일반 보간과 동일
                // speed가 작을수록 중간 과정은 천천히 움직이지만 결국 목표지점에는 도달
                float layerT;
                if (smoothT < 0.5f)
                {
                    // 전반부: 각 레이어의 속도에 따라 다르게 움직임
                    layerT = smoothT * speed * 2f;
                }
                else
                {
                    // 후반부: 목표지점을 향해 가속
                    float remainingDistance = 1f - (speed * 0.5f); // 전반부에서 이동하지 않은 거리
                    float normalizedT = (smoothT - 0.5f) * 2f; // 0.5~1 범위를 0~1로 정규화
                    layerT = (speed * 0.5f) + (remainingDistance * normalizedT);
                }

                // 최종 위치 보간
                layerT = Mathf.Clamp01(layerT); // 0~1 범위 보장

                RectTransform currentBg = isImage1Active[i] ? backgroundLayers[i].image1 : backgroundLayers[i].image2;
                RectTransform nextBg = isImage1Active[i] ? backgroundLayers[i].image2 : backgroundLayers[i].image1;

                currentBg.anchoredPosition = Vector2.Lerp(currentStartPositions[i], currentTargetPositions[i], layerT);
                nextBg.anchoredPosition = Vector2.Lerp(nextStartPositions[i], nextTargetPositions[i], layerT);
            }

            OnScrollUpdate?.Invoke(smoothT);
            yield return null;
        }

        // 정확한 최종 위치 설정
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            RectTransform currentBg = isImage1Active[i] ? backgroundLayers[i].image1 : backgroundLayers[i].image2;
            RectTransform nextBg = isImage1Active[i] ? backgroundLayers[i].image2 : backgroundLayers[i].image1;

            currentBg.anchoredPosition = currentTargetPositions[i];
            nextBg.anchoredPosition = nextTargetPositions[i];

            isImage1Active[i] = !isImage1Active[i];
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