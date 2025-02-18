using UnityEngine;

public class FireHitEffect : MonoBehaviour
{
    private ParticleSystem fireEffect;

    private void Awake()
    {
        fireEffect = GetComponent<ParticleSystem>();
    }

    public void PlayEffect(Vector3 position)
    {
        transform.position = position; // 위치 설정
        fireEffect.Play(); // 파티클 실행
        Destroy(gameObject, 0.5f); // 0.5초 후 자동 삭제
    }
}