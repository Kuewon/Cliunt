using UnityEngine;

public class HitEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float flashTime = 0.1f;
    private float flashTimer = 0f;
    private Color originalColor;

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
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            flashTimer = flashTime;
            StartCoroutine(FlashRoutine());
        }
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        while (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            float lerp = flashTimer / flashTime;
            spriteRenderer.color = Color.Lerp(originalColor, Color.white, lerp);
            yield return null;
        }
        spriteRenderer.color = originalColor;
    }
}