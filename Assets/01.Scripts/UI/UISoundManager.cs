using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    private static UISoundManager instance;

    public static UISoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UISoundManager>();
                if (instance == null)
                {
                    Debug.LogError("UISoundManager 인스턴스가 존재하지 않습니다.");
                }
            }
            return instance;
        }
    }

    [SerializeField] private AudioSource audioSource;

    [Header("UI 사운드 클립")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] [Range(0f, 1f)] private float clickVolume = 0.3f;

    [SerializeField] private AudioClip tabSwitchSound;
    [SerializeField] [Range(0f, 1f)] private float tabSwitchVolume = 0.3f;

    [SerializeField] private AudioClip closeSound;  // 🔹 UI 닫기 사운드 추가
    [SerializeField] [Range(0f, 1f)] private float closeVolume = 0.3f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    public void PlayClickSound() => PlaySound(clickSound, clickVolume);
    public void PlayTabSwitchSound() => PlaySound(tabSwitchSound, tabSwitchVolume);
    public void PlayCloseSound() => PlaySound(closeSound, closeVolume);  // 🔹 UI 닫기 사운드 메서드 추가

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null) audioSource.PlayOneShot(clip, volume);
    }
}