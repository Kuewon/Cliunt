using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RangedHitEffectsManager : MonoBehaviour
{
    public static RangedHitEffectsManager Instance { get; private set; }

    [Header("Effect Settings")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float effectDuration = 0.5f;
    [SerializeField] private float randomRadius = 30f;  // 반경을 30으로 줄임

    private RectTransform topIngameRect;
    private RectTransform playerRect;
    private RectTransform effectContainerRect;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // TopIngame 캔버스 찾기
        GameObject topIngame = GameObject.FindWithTag("TopIngame");
        if (topIngame != null)
        {
            topIngameRect = topIngame.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError("❌ TopIngame 캔버스를 찾을 수 없습니다!");
            return;
        }

        // 플레이어 찾기
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerRect = player.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError("❌ Player를 찾을 수 없습니다!");
            return;
        }

        // 이펙트 컨테이너 생성
        GameObject container = new GameObject("RangedHitEffects");
        effectContainerRect = container.AddComponent<RectTransform>();
        effectContainerRect.SetParent(topIngameRect, false);  // worldPositionStays를 false로 설정
        
        // 컨테이너의 RectTransform 설정
        effectContainerRect.anchorMin = Vector2.zero;
        effectContainerRect.anchorMax = Vector2.one;
        effectContainerRect.offsetMin = Vector2.zero;
        effectContainerRect.offsetMax = Vector2.zero;
        effectContainerRect.localScale = Vector3.one;  // 로컬 스케일을 1로 명시적 설정
    }

    public void SpawnHitEffect()
    {
        if (!ValidateComponents()) return;

        // 플레이어 위치에서 오른쪽으로 50유닛 이동한 기준점 설정
        Vector2 basePosition = playerRect.anchoredPosition + new Vector2(50f, 0f);

        // 기준점 주변의 랜덤한 위치 계산
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomDistance = Random.Range(0f, randomRadius);
        Vector2 randomOffset = new Vector2(
            Mathf.Cos(randomAngle) * randomDistance,
            Mathf.Sin(randomAngle) * randomDistance
        );

        // 최종 이펙트 위치 계산
        Vector2 hitPosition = basePosition + randomOffset;

        // 이펙트 생성 및 설정
        GameObject effectObj = Instantiate(hitEffectPrefab, effectContainerRect);
        effectObj.SetActive(true);

        RectTransform effectRect = effectObj.GetComponent<RectTransform>();
        if (effectRect != null)
        {
            // 이펙트의 anchoredPosition 설정
            effectRect.anchoredPosition = hitPosition;
            
            // RectTransform 설정
            effectRect.anchorMin = new Vector2(0.5f, 0.5f);
            effectRect.anchorMax = new Vector2(0.5f, 0.5f);
            effectRect.pivot = new Vector2(0.5f, 0.5f);
        }

        StartCoroutine(DestroyEffectAfterDelay(effectObj));
    }

    private bool ValidateComponents()
    {
        if (playerRect == null)
        {
            Debug.LogError("❌ PlayerRect가 없습니다!");
            return false;
        }

        if (hitEffectPrefab == null)
        {
            Debug.LogError("❌ HitEffectPrefab이 할당되지 않았습니다!");
            return false;
        }

        if (effectContainerRect == null)
        {
            Debug.LogError("❌ EffectContainer가 없습니다!");
            return false;
        }

        return true;
    }

    private IEnumerator DestroyEffectAfterDelay(GameObject effect)
    {
        yield return new WaitForSeconds(effectDuration);
        if (effect != null)
        {
            Destroy(effect);
        }
    }

    // 범위 시각화를 위한 기즈모
    private void OnDrawGizmos()
    {
        if (playerRect != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = playerRect.position;
            Gizmos.DrawWireSphere(center, randomRadius);
        }
    }
}