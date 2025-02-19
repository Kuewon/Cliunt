using UnityEngine;
using System.Collections.Generic;

namespace _01.Scripts.Interaction
{
    public class HammerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform[] spinnerTriggers;
        [SerializeField] private GaugeBar gaugeBar;
        [SerializeField] private FireHitEffect fireHitPrefab;
        [SerializeField] private Transform fireHitPoint;
        [SerializeField] private SpinnerController spinnerController; // 추가된 SpinnerController 참조

        [Header("🔔 진동 설정")]
        [SerializeField] private long vibrationDuration = 30;
        [SerializeField] private int vibrationStrength = 30;

        private RectTransform _myRect;
        private Camera _uiCamera;
        private Dictionary<int, Vector3> _previousPositions = new Dictionary<int, Vector3>();

        private Queue<int> _hitQueue = new Queue<int>();
        private Dictionary<int, bool> _canHit = new Dictionary<int, bool>();
        private Dictionary<int, bool> _hasExitedHammer = new Dictionary<int, bool>();

        private int _lastHitFrame = 0;
        private const int MIN_FRAME_GAP = 5;
        private const int EXIT_FRAME_THRESHOLD = 30;
        private const int SAMPLE_POINTS = 15;
        private const float SPEED_THRESHOLD = 300f;

        private bool _isFirstFrame = true;
        private PlayerController playerController;

        private void Awake()
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("CharacterController not found in the scene!");
            }

            if (spinnerController == null)
            {
                spinnerController = FindObjectOfType<SpinnerController>();
                if (spinnerController == null)
                {
                    Debug.LogError("SpinnerController not found in the scene!");
                }
            }
        }

        void Start()
        {
            _myRect = GetComponent<RectTransform>();
            _uiCamera = Camera.main;

            for (int i = 0; i < spinnerTriggers.Length; i++)
            {
                _previousPositions[i] = spinnerTriggers[i].position;
                _canHit[i] = false;
                _hasExitedHammer[i] = false;
            }
        }

        void Update()
        {
            if (_isFirstFrame)
            {
                _isFirstFrame = false;
                return;
            }

            if (_hitQueue.Count > 0 && Time.frameCount - _lastHitFrame >= MIN_FRAME_GAP)
            {
                int triggerIndex = _hitQueue.Dequeue();
                _lastHitFrame = Time.frameCount;

                gaugeBar?.IncreaseGauge();

                if (playerController != null)
                {
                    playerController.TriggerManualAttack();
                }

                TriggerFireHitEffect();
                TriggerVibration();
            }

            for (int i = 0; i < spinnerTriggers.Length; i++)
            {
                Vector3 currentPosition = spinnerTriggers[i].position;
                Vector3 previousPosition = _previousPositions[i];

                float speed = Vector3.Distance(currentPosition, previousPosition) / Time.deltaTime;

                if (_canHit[i] && _hasExitedHammer[i] &&
                    (IsTouchingHammer(currentPosition) || MultiSampleCheck(previousPosition, currentPosition)))
                {
                    // 총알 활성화 상태 확인
                    if (spinnerController != null && spinnerController.IsBullettActive(i))
                    {
                        if (!_hitQueue.Contains(i))
                        {
                            _hitQueue.Enqueue(i);
                            _canHit[i] = false;
                            _hasExitedHammer[i] = false;
                        }
                    }
                }

                if (!IsTouchingHammer(currentPosition))
                {
                    if (speed > SPEED_THRESHOLD)
                    {
                        _canHit[i] = true;
                        _hasExitedHammer[i] = true;
                    }
                    else if (!_hasExitedHammer[i])
                    {
                        _hasExitedHammer[i] = true;
                        _canHit[i] = true;
                    }
                }

                _previousPositions[i] = currentPosition;
            }
        }

        private bool IsTouchingHammer(Vector3 position)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(_uiCamera, position);
            return RectTransformUtility.RectangleContainsScreenPoint(_myRect, screenPoint, _uiCamera);
        }

        private bool MultiSampleCheck(Vector3 start, Vector3 end)
        {
            for (int j = 1; j <= SAMPLE_POINTS; j++)
            {
                float t = (float)j / (SAMPLE_POINTS + 1);
                Vector3 interpolatedPosition = Vector3.Lerp(start, end, t);

                if (IsTouchingHammer(interpolatedPosition))
                {
                    return true;
                }
            }
            return false;
        }

        private void TriggerFireHitEffect()
        {
            if (fireHitPrefab != null && fireHitPoint != null) 
            {
                FireHitEffect fireHitInstance = Instantiate(fireHitPrefab, fireHitPoint.position, Quaternion.identity);
                fireHitInstance.PlayEffect(fireHitPoint.position);
            }
        }

        private void TriggerVibration()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");

                if (vibrator != null)
                {
                    AndroidJavaClass vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
                    AndroidJavaObject effect = vibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", vibrationDuration, vibrationStrength);
                    vibrator.Call("vibrate", effect);
                }
            }
            else
            {
                Handheld.Vibrate();
            }
        }
    }
}