using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    [SerializeField] private DamagePopup popupPrefab;
    private Transform canvasTransform;

    private void Awake()
    {
        GameObject canvas = GameObject.FindWithTag("TopIngame");
        if (canvas != null)
        {
            canvasTransform = canvas.transform;
        }
        else
        {
            Debug.LogError("TopIngame 태그를 가진 캔버스를 찾을 수 없습니다!");
        }
    }

    public void ShowDamage(Vector3 worldPosition, float amount, bool isCritical = false)
    {
        // 기본적으로 Enemy 타입으로 처리 (이전 버전과의 호환성 유지)
        ShowDamage(worldPosition, amount, isCritical, DamagePopup.EntityType.Player);
    }

    public void ShowDamage(Vector3 worldPosition, float amount, bool isCritical, DamagePopup.EntityType entityType)
    {
        if (canvasTransform == null || popupPrefab == null) 
        {
            Debug.LogError("DamagePopupManager: 필수 컴포넌트가 없습니다!");
            return;
        }

        DamagePopup popup = Instantiate(popupPrefab, canvasTransform);
        popup.Setup(worldPosition, amount, isCritical, entityType);
    }
}