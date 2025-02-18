using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    private static UISoundManager instance; // 싱글톤 인스턴스
    

    [SerializeField] private AudioSource audioSource; // UI 사운드용 오디오 소스

    [Header("UI 사운드 클립 & 볼륨 설정")]
    [SerializeField] private AudioClip clickSound; // 버튼 클릭 소리
    [SerializeField] [Range(0f, 1f)] 
    private float clickVolume = 0.3f; // 버튼 클릭 볼륨

    [SerializeField] private AudioClip closeSound; // 창 닫기 소리
    [SerializeField] [Range(0f, 1f)] 
    private float closeVolume = 0.3f; // 창 닫기 볼륨



    
    [Header("로비 사운드")]
    [SerializeField] private AudioClip gameStartSound; // 버튼 클릭 소리
    [SerializeField] [Range(0f, 1f)] 
    private float gameStartVolume = 0.3f; // 버튼 클릭 볼륨

    void Awake()
    {
        // 싱글톤 패턴 적용 (UI 사운드 전용)
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

        // 기본 설정
        audioSource.playOnAwake = false; // 자동 실행 X
        audioSource.loop = false; // UI 사운드는 반복 X
    }

    // 🔊 버튼 클릭 사운드 실행
    public void PlayClickSound()
    {
        PlaySound(clickSound, clickVolume);
    }

    // 🔊 창 닫기 사운드 실행
    public void PlayCloseSound()
    {
        PlaySound(closeSound, closeVolume);
    }

    // 로비 스타트 사운드
    public void PlayLobbyGamseStartSound()
    {
        PlaySound(gameStartSound, gameStartVolume);
    }

    // 🔊 사운드 실행 함수 (개별 볼륨 적용)
    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, volume); // 개별 볼륨 적용
        }
    }

    // 🔊 UI 사운드 볼륨 조절 (전체 적용)
    public void SetGlobalVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp(volume, 0f, 1f);
    }
}