using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Fever : MonoBehaviour
{
    public Image cylinderImage;
    public GaugeBar gaugeBar;

    [Header("색상 설정 (점진적 변화)")]
    public Color normalColor = new Color(1f, 1f, 1f, 1f); // 기본 색 (흰색)
    public Color midColor1 = new Color(1f, 0.7f, 0.5f, 1f); // 주황빛 빨강
    public Color midColor2 = new Color(1f, 0.4f, 0.4f, 1f); // 연한 빨강
    public Color feverColor = new Color(0.8f, 0.2f, 0.2f, 1f); // 강한 빨강

    [Header("설정값")]
    public float colorChangeSpeed = 1.5f; // 색상 변화 속도
    public float cooldownTime = 3f; // 원래 색으로 돌아오는 시간

    private bool _isFeverActive;
    private Coroutine _colorChangeCoroutine;
    private float feverThreshold = 0.5f; // 🔹 게이지 50%부터 색 변화 시작

    private void Awake()
    {
        if (cylinderImage == null)
        {
            cylinderImage = GetComponent<Image>(); 
        }
        if (gaugeBar == null)
        {
            gaugeBar = FindObjectOfType<GaugeBar>(); 
        }
    }

    private void Update()
    {
        if (gaugeBar == null || cylinderImage == null)
        {
            return;
        }

        float gaugePercent = gaugeBar.CurrentGauge / gaugeBar.GetMaxGauge(); // 🔹 현재 게이지 % 계산

        // 🔹 게이지에 따라 색상을 점진적으로 변경
        Color targetColor = normalColor;
        if (gaugePercent >= 0.9f) targetColor = feverColor; // 최대로 차면 진한 빨강
        else if (gaugePercent >= 0.75f) targetColor = midColor2; // 연한 빨강
        else if (gaugePercent >= feverThreshold) targetColor = midColor1; // 주황빛 빨강

        // 🔥 색상이 자연스럽게 변하도록 처리 (한 번만 실행)
        if (_colorChangeCoroutine != null)
            StopCoroutine(_colorChangeCoroutine);
        _colorChangeCoroutine = StartCoroutine(ChangeColor(targetColor));
    }

    IEnumerator ChangeColor(Color targetColor)
    {
        float t = 0f;
        Color startColor = cylinderImage.color;

        while (t < 1f)
        {
            t += Time.deltaTime * colorChangeSpeed;
            cylinderImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
    }
}
