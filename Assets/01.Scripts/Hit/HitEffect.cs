using UnityEngine;

public class HitEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float flashTime = 0.1f;
    private float criticalFlashTime = 0.2f;
    private float flashTimer = 0f;
    private Color originalColor;
    private bool isCriticalFlashing = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void PlayHitEffect()
    {
        if (spriteRenderer != null && !isCriticalFlashing)
        {
            spriteRenderer.color = Color.white;
            flashTimer = flashTime;
            StartCoroutine(FlashRoutine(Color.white, flashTime));
        }
    }

    public void PlayCriticalHitEffect()
    {
        if (spriteRenderer != null)
        {
            isCriticalFlashing = true;
            spriteRenderer.color = Color.red;
            flashTimer = criticalFlashTime;
            StartCoroutine(FlashRoutine(Color.red, criticalFlashTime));

            // 태그로 Top_Ingame 찾기
            GameObject topIngame = GameObject.FindGameObjectWithTag("TopIngame");
            if (topIngame != null)
            {
                StartCoroutine(ShakeCanvas(topIngame.GetComponent<RectTransform>(), 0.2f, 0.2f));
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
                spriteRenderer.color = Color.Lerp(originalColor, intensifiedColor, lerp);
            }
            else
            {
                spriteRenderer.color = Color.Lerp(originalColor, flashColor, lerp);
            }

            yield return null;
        }

        spriteRenderer.color = originalColor;
        isCriticalFlashing = false;
    }

    private System.Collections.IEnumerator ShakeCanvas(RectTransform canvasRect, float duration, float magnitude)
    {
        if (canvasRect == null) yield break;

        Vector2 originalPos = canvasRect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude * 30f;
            float y = Random.Range(-1f, 1f) * magnitude * 30f;

            canvasRect.anchoredPosition = new Vector2(originalPos.x + x, originalPos.y + y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasRect.anchoredPosition = originalPos;
    }
}