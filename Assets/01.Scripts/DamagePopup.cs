// DamagePopup.cs
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    private TextMesh textMesh;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;

    private void Awake()
    {
        // TextMesh 컴포넌트 추가
        textMesh = gameObject.AddComponent<TextMesh>();
        textMesh.fontSize = 40;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.characterSize = 0.1f;

        // 정렬을 위한 MeshRenderer 설정
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.sortingLayerName = "UI"; // 적절한 Sorting Layer를 설정하세요
        meshRenderer.sortingOrder = 100;
    }

    public static DamagePopup Create(Vector3 position, float damageAmount, bool isCritical = false)
    {
        GameObject damagePopupObj = new GameObject("DamagePopup");
        DamagePopup damagePopup = damagePopupObj.AddComponent<DamagePopup>();
        damagePopup.Setup(position, damageAmount, isCritical);
        return damagePopup;
    }

    private void Setup(Vector3 position, float damageAmount, bool isCritical)
    {
        transform.position = position + new Vector3(0, 0.5f, 0);
        transform.rotation = Quaternion.Euler(0, 0, 0); // 카메라를 향하도록

        // 데미지 텍스트 설정
        textMesh.text = damageAmount.ToString("F1");
        textMesh.color = isCritical ? Color.red : Color.yellow;

        textColor = textMesh.color;
        disappearTimer = 1f;
        moveVector = new Vector3(Random.Range(-1f, 1f), 1) * 3f;
    }

    private void Update()
    {
        // 카메라를 향하도록 회전
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }

        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 8f * Time.deltaTime;

        if (disappearTimer > 0.5f)
        {
            // 첫 0.5초는 원래 크기
            transform.localScale += Vector3.one * Time.deltaTime * 0.3f;
        }
        else
        {
            // 그 다음부터 점점 작아짐
            transform.localScale -= Vector3.one * Time.deltaTime * 1f;
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            textColor.a -= Time.deltaTime * 2f;
            textMesh.color = textColor;
            if (textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}