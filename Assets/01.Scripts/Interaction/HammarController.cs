using UnityEngine;
using System.Collections.Generic;

namespace _01.Scripts.Interaction
{
    public class HammerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform[] spinnerTriggers;
        [SerializeField] private GaugeBar gaugeBar;
        [SerializeField] private FireHitEffect fireHitPrefab; // 🔥 Fire Hit 프리팹
        [SerializeField] private Transform fireHitPoint; // 🔥 Fire Hit이 나올 위치

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

                // ✅ Hit 발생 시 GaugeBar에 알림
                gaugeBar?.IncreaseGauge();

                // ✅ Hit 발생 시 플레이어 공격 트리거
                if (playerController != null)
                {
                    playerController.TriggerManualAttack();
                }

                // 🔥 Fire Hit 이펙트 실행 (고정된 위치에서)
                TriggerFireHitEffect();
            }

            for (int i = 0; i < spinnerTriggers.Length; i++)
            {
                Vector3 currentPosition = spinnerTriggers[i].position;
                Vector3 previousPosition = _previousPositions[i];

                float speed = Vector3.Distance(currentPosition, previousPosition) / Time.deltaTime;

                if (_canHit[i] && _hasExitedHammer[i] &&
                    (IsTouchingHammer(currentPosition) || MultiSampleCheck(previousPosition, currentPosition)))
                {
                    if (!_hitQueue.Contains(i))
                    {
                        _hitQueue.Enqueue(i);
                        _canHit[i] = false;
                        _hasExitedHammer[i] = false;
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

        // 🔥 Fire Hit 이펙트 실행 함수 (고정된 위치에서 실행됨!)
        private void TriggerFireHitEffect()
        {
            if (fireHitPrefab != null && fireHitPoint != null) // 🔹 Fire Hit 프리팹과 위치가 설정되었는지 확인
            {
                FireHitEffect fireHitInstance = Instantiate(fireHitPrefab, fireHitPoint.position, Quaternion.identity);
                fireHitInstance.PlayEffect(fireHitPoint.position);
            }
        }
    }
}
