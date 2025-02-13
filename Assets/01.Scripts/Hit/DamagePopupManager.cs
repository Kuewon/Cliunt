using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    [SerializeField] private DamagePopup popupPrefab;  // 위에서 만든 프리팹
    private Transform canvasTransform;

    private void Start()
    {
        canvasTransform = GameObject.FindWithTag("TopIngame").transform;
    }

    public void ShowDamage(Vector2 position, float amount, bool isCritical = false)
    {
        if (canvasTransform == null) return;

        DamagePopup popup = Instantiate(popupPrefab, canvasTransform);
        popup.Setup(position, amount, isCritical);
    }
}