using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ParallaxBackgroundScroller : MonoBehaviour
{
    public static ParallaxBackgroundScroller Instance { get; private set; }
    public bool IsScrolling => isScrolling;
    
    public event Action OnScrollComplete;
    public event Action<float> OnScrollUpdate;

    [System.Serializable]
    public class BackgroundLayer
    {
        public RectTransform image1;
        public RectTransform image2;
        [Range(0f, 1f)]
        public float scrollAmount = 1f;
    }

    [SerializeField] public BackgroundLayer[] backgroundLayers;
    [SerializeField] private float scrollSpeed = 300f;
    [SerializeField] private float scrollDuration = 2f;
    private float[] layerWidths;
    private bool isScrolling = false;

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

        InitializeBackgrounds();
    }

    private void InitializeBackgrounds()
    {
        layerWidths = new float[backgroundLayers.Length];
        
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            layerWidths[i] = backgroundLayers[i].image1.rect.width;
            backgroundLayers[i].image1.anchoredPosition = Vector2.zero;
            backgroundLayers[i].image2.anchoredPosition = new Vector2(layerWidths[i], 0);
        }
    }

    private void Update()
    {
        if (!isScrolling) return;

        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            MoveLayer(i);
            CheckAndResetPosition(i);
        }
    }

    private void MoveLayer(int index)
    {
        var layer = backgroundLayers[index];
        float moveAmount = scrollSpeed * layer.scrollAmount * Time.deltaTime;

        // 현재 위치에서 왼쪽으로 이동
        Vector2 pos1 = layer.image1.anchoredPosition;
        Vector2 pos2 = layer.image2.anchoredPosition;

        pos1 += Vector2.left * moveAmount;
        pos2 += Vector2.left * moveAmount;

        layer.image1.anchoredPosition = pos1;
        layer.image2.anchoredPosition = pos2;
    }

    private void CheckAndResetPosition(int index)
    {
        var layer = backgroundLayers[index];
        float width = layerWidths[index];

        // 첫 번째 이미지가 화면 밖으로 완전히 나갔는지 체크
        if (layer.image1.anchoredPosition.x < -width)
        {
            // 두 번째 이미지의 오른쪽에 재배치
            layer.image1.anchoredPosition = layer.image2.anchoredPosition + new Vector2(width, 0);
            
            // 참조 스왑
            var temp = layer.image1;
            layer.image1 = layer.image2;
            layer.image2 = temp;
        }
    }

    private IEnumerator ScrollCoroutine()
    {
        isScrolling = true;
        float elapsedTime = 0f;

        while (elapsedTime < scrollDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / scrollDuration;
            OnScrollUpdate?.Invoke(progress);
            yield return null;
        }

        isScrolling = false;
        OnScrollComplete?.Invoke();
    }

    public void StartScroll()
    {
        if (!isScrolling)
        {
            StartCoroutine(ScrollCoroutine());
        }
    }

    public void StopScroll()
    {
        isScrolling = false;
        OnScrollComplete?.Invoke();
    }
}