using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    [SerializeField] private DamagePopup popupPrefab;
    private Transform canvasTransform;

    private void Awake()  // Start 대신 Awake 사용
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

    public void ShowDamage(Vector3 worldPosition, float amount, bool isCritical = false)  // Vector2 대신 Vector3 사용
    {
        if (canvasTransform == null || popupPrefab == null) 
        {
            Debug.LogError("DamagePopupManager: 필수 컴포넌트가 없습니다!");
            return;
        }

        DamagePopup popup = Instantiate(popupPrefab, canvasTransform);
        popup.Setup(worldPosition, amount, isCritical);
    }
}