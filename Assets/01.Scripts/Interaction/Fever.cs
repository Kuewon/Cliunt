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
    public Color feverColor = new Color(0.8f, 0.2f, 0.2f, 1f); // 강한 빨강 (MAX 시)

    [Header("설정값")]
    public float colorChangeSpeed = 1.5f; // 색상 변화 속도
    private float cooldownThreshold = 0.1f; // 🔥 **10% 이하가 되면 점진적 색 복귀**

    private bool _isMaxState; // 🔥 MAX 상태 유지 여부
    private Coroutine _colorChangeCoroutine;

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

        // 🔥 MAX 상태일 때 강한 빨강 유지 (10% 이하까지 유지됨)
        if (gaugePercent >= 1f)
        {
            if (!_isMaxState)
            {
                _isMaxState = true; // MAX 상태 진입
                ChangeColorInstant(feverColor); // 즉시 강한 빨강 적용
            }
            return; // 여기서 종료 (색상 변경 중지)
        }

        // 🎯 MAX 이후 게이지가 10% 이상일 때는 강한 빨강 유지
        if (_isMaxState && gaugePercent > cooldownThreshold)
        {
            return; // 🔥 10% 이하로 떨어질 때까지 강한 빨강 유지
        }

        // 🔥 10% 이하가 되면 점진적으로 원래 색으로 복귀
        if (_isMaxState && gaugePercent <= cooldownThreshold)
        {
            _isMaxState = false; // MAX 상태 해제
        }

        // 🔹 MAX가 아니고, 10% 이하에서만 점진적으로 색상 변화
        Color targetColor = normalColor;
        if (gaugePercent >= 0.9f) targetColor = feverColor;
        else if (gaugePercent >= 0.75f) targetColor = midColor2;
        else if (gaugePercent >= 0.5f) targetColor = midColor1;

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

    // 🔥 즉시 색상 변경 (MAX 상태에서 바로 빨간색으로 만들기)
    private void ChangeColorInstant(Color targetColor)
    {
        if (_colorChangeCoroutine != null)
            StopCoroutine(_colorChangeCoroutine);

        cylinderImage.color = targetColor;
    }
}
