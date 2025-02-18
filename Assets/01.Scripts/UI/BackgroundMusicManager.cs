using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    private static BackgroundMusicManager instance; // 싱글톤 인스턴스

    [SerializeField] private AudioSource audioSource; // 오디오 소스

    [Header("🎵 로비 BGM 설정")]
    [SerializeField] private AudioClip lobbyBGM; // 로비 BGM
    [SerializeField] [Range(0f, 1f)] private float lobbyVolume = 0.5f; // 로비 BGM 볼륨

    [Header("🎮 인게임 BGM 설정")]
    [SerializeField] private AudioClip inGameBGM; // 인게임 BGM
    [SerializeField] [Range(0f, 1f)] private float inGameVolume = 0.5f; // 인게임 BGM 볼륨

    void Awake()
    {
        // 싱글톤 패턴 적용 (BGM 유지)
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 변경 시에도 유지
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // AudioSource가 없으면 자동 추가
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // 오디오 설정
        audioSource.loop = true; // 반복 재생
        audioSource.playOnAwake = false; // 자동 실행 X

        // 초기에 로비 BGM 실행
        PlayLobbyBGM();
    }

    // 🎵 로비 BGM 실행 (볼륨 반영)
    public void PlayLobbyBGM()
    {
        ChangeBGM(lobbyBGM, lobbyVolume);
    }

    // 🎮 인게임 BGM 실행 (볼륨 반영)
    public void PlayInGameBGM()
    {
        ChangeBGM(inGameBGM, inGameVolume);
    }

    // 🎼 BGM 변경 (볼륨도 함께 적용)
    private void ChangeBGM(AudioClip newClip, float volume)
    {
        if (audioSource.clip == newClip) return; // 같은 음악이면 변경 안 함

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.volume = volume; // 볼륨 설정 반영
        audioSource.Play();
    }

    // ⏸ BGM 중지
    public void StopBGM()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    // 🔊 수동으로 볼륨 조절 (전체 BGM에 적용)
    public void SetGlobalVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp(volume, 0f, 1f);
    }
}