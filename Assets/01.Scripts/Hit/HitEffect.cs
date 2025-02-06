using UnityEngine;

public class HitEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float flashTime = 0.1f;
    private float criticalFlashTime = 0.2f; // 크리티컬은 더 오래 지속
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
        if (spriteRenderer != null && !isCriticalFlashing) // 크리티컬 진행 중에는 일반 히트 무시
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
            
            // 화면 흔들기 효과 추가
            StartCoroutine(ShakeCamera(0.1f, 0.2f));
        }
    }

    private System.Collections.IEnumerator FlashRoutine(Color flashColor, float duration)
    {
        while (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            float lerp = flashTimer / duration;
            
            // 크리티컬일 경우 색상을 더 강렬하게
            if (isCriticalFlashing)
            {
                float intensity = 1f + Mathf.Sin(Time.time * 30f) * 0.2f; // 깜빡이는 효과
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

    private System.Collections.IEnumerator ShakeCamera(float duration, float magnitude)
    {
        if (Camera.main == null) yield break;

        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Camera.main.transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalPos;
    }
}