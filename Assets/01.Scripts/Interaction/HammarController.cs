using UnityEngine;

namespace _01.Scripts.Interaction
{
    public class HammerController : MonoBehaviour
    {
        private int _currentHitNumber = 1;
        [SerializeField] private RectTransform[] spinnerTriggers;
        private RectTransform _myRect;
        private bool _canTrigger = true;  // 연속 감지 방지용 플래그 추가
        
        void Start()
        {
            _myRect = GetComponent<RectTransform>();
        }
        
        void Update()
        {
            CheckTriggerProximity();
        }
        
        private void CheckTriggerProximity()
        {
            // 월드 스페이스 위치 사용
            Vector3 hammerPos = _myRect.position;
            
            foreach(RectTransform trigger in spinnerTriggers)
            {
                Vector3 triggerPos = trigger.position;
                float distance = Vector2.Distance(
                    new Vector2(hammerPos.x, hammerPos.y),
                    new Vector2(triggerPos.x, triggerPos.y)
                );

                // 거리 체크 값을 좀 더 크게 조정
                if(distance < 50f && _canTrigger)
                {
                    Debug.Log($"Hit {_currentHitNumber}");
                    _currentHitNumber = (_currentHitNumber % 6) + 1;
                    _canTrigger = false;  // 연속 감지 방지
                    break;
                }
                else if(distance >= 50f)
                {
                    _canTrigger = true;  // 다시 감지 가능하도록 설정
                }
            }
        }
    }
}