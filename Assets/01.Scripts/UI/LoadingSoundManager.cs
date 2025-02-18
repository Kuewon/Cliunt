using UnityEngine;
using System.Collections;

public class LoadingSoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource; // 오디오 소스 (Inspector에서 설정 가능)
    [SerializeField] private AudioClip soundClip; // 재생할 오디오 파일

    void Start()
    {
        // AudioSource가 없으면 자동 추가
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // 오디오 설정
        audioSource.clip = soundClip;
        audioSource.playOnAwake = false; // 자동 실행 방지
        audioSource.loop = false; // 한 번만 실행
        audioSource.volume = 0.8f; // 소리 크기 조절 (0.0 ~ 1.0)

        // 2초 뒤에 사운드 실행
        StartCoroutine(PlaySoundAfterDelay(1.0f));
    }

    // 일정 시간 후 사운드 재생
    private IEnumerator PlaySoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.Play();

        // 사운드 길이만큼 대기한 후 자동 삭제
        yield return new WaitForSeconds(audioSource.clip.length);
        Destroy(gameObject); // 사운드가 끝나면 오브젝트 삭제
    }
}