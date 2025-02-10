using UnityEngine;
using UnityEngine.UI;

public class HitEffect : MonoBehaviour
{
    private Image image;
    private float flashTime = 0.1f;
    private float criticalFlashTime = 0.2f;
    private float flashTimer = 0f;
    private Color originalColor;
    private bool isCriticalFlashing = false;
    private static Vector2 originalCanvasPos;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (image != null)
        {
            originalColor = image.color;
        }
        else
        {
            Debug.LogWarning("Image 컴포넌트를 찾을 수 없습니다!");
        }
    }

    public void PlayHitEffect()
    {
        if (image != null && !isCriticalFlashing)
        {
            image.color = Color.white;
            flashTimer = flashTime;
            StartCoroutine(FlashRoutine(Color.white, flashTime));
        }
    }

    public void PlayCriticalHitEffect()
    {
        if (image != null)
        {
            isCriticalFlashing = true;
            image.color = Color.red;
            flashTimer = criticalFlashTime;
            StartCoroutine(FlashRoutine(Color.red, criticalFlashTime));

            // 캔버스 찾기 및 흔들기 효과 적용
            GameObject topIngame = GameObject.FindGameObjectWithTag("TopIngame");
            if (topIngame != null)
            {
                RectTransform canvasRect = topIngame.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    // 최초 실행 시 원본 위치 저장
                    if (originalCanvasPos == Vector2.zero)
                    {
                        originalCanvasPos = canvasRect.anchoredPosition;
                    }
                    StartCoroutine(ShakeCanvas(canvasRect, 0.2f, 5f));
                }
            }
        }
    }

    private System.Collections.IEnumerator FlashRoutine(Color flashColor, float duration)
    {
        while (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            float lerp = flashTimer / duration;

            if (isCriticalFlashing)
            {
                float intensity = 1f + Mathf.Sin(Time.time * 30f) * 0.2f;
                Color intensifiedColor = flashColor * intensity;
                image.color = Color.Lerp(originalColor, intensifiedColor, lerp);
            }
            else
            {
                image.color = Color.Lerp(originalColor, flashColor, lerp);
            }

            yield return null;
        }

        image.color = originalColor;
        isCriticalFlashing = false;
    }

    private System.Collections.IEnumerator ShakeCanvas(RectTransform canvasRect, float duration, float magnitude)
    {
        if (canvasRect == null) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            canvasRect.anchoredPosition = originalCanvasPos + new Vector2(x, y);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 흔들림 효과 후 원래 위치로 복귀
        canvasRect.anchoredPosition = originalCanvasPos;
    }
}