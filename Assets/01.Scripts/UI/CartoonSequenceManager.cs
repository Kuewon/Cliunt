using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CartoonSequenceManager : MonoBehaviour
{
    [System.Serializable]
    public class CartoonScene
    {
        public Sprite sceneImage;
        [TextArea(3, 10)]
        public string sceneText;
        public GameObject transitionAnimation; // 트랜지션 애니메이션 프리팹
    }

    [Header("UI References")]
    [SerializeField] private Image sceneImage;
    [SerializeField] private TextMeshProUGUI sceneText;
    [SerializeField] private GameObject touchBlocker; // 터치 입력을 막는 패널

    [Header("Scene Data")]
    [SerializeField] private CartoonScene[] scenes;
    
    private int currentSceneIndex = 0;
    private bool isTransitioning = false;
    private Animator transitionAnimator;
    private GameObject currentTransition;

    private void Start()
    {
        // 첫 번째 씬 표시
        ShowCurrentScene();
        touchBlocker.SetActive(false);
    }

    private void Update()
    {
        // 터치/클릭 감지
        if (Input.GetMouseButtonDown(0) && !isTransitioning)
        {
            StartCoroutine(TransitionToNextScene());
        }
    }

    private void ShowCurrentScene()
    {
        if (currentSceneIndex < scenes.Length)
        {
            // 현재 씬의 이미지와 텍스트 표시
            sceneImage.sprite = scenes[currentSceneIndex].sceneImage;
            sceneText.text = scenes[currentSceneIndex].sceneText;
        }
    }

    private IEnumerator TransitionToNextScene()
    {
        if (currentSceneIndex >= scenes.Length - 1) yield break;

        isTransitioning = true;
        touchBlocker.SetActive(true);

        // 현재 씬의 트랜지션 애니메이션 실행
        if (scenes[currentSceneIndex].transitionAnimation != null)
        {
            // 이전 트랜지션 제거
            if (currentTransition != null)
            {
                Destroy(currentTransition);
            }

            // 새 트랜지션 생성
            currentTransition = Instantiate(scenes[currentSceneIndex].transitionAnimation);
            transitionAnimator = currentTransition.GetComponent<Animator>();
            
            // 애니메이션 완료 대기
            if (transitionAnimator != null)
            {
                yield return new WaitForSeconds(transitionAnimator.GetCurrentAnimatorStateInfo(0).length);
            }
            else
            {
                // 애니메이터가 없는 경우 기본 대기 시간
                yield return new WaitForSeconds(1f);
            }
        }

        // 다음 씬으로 이동
        currentSceneIndex++;
        ShowCurrentScene();

        isTransitioning = false;
        touchBlocker.SetActive(false);
    }

    // 씬 인덱스 초기화 (필요한 경우)
    public void ResetToFirstScene()
    {
        currentSceneIndex = 0;
        ShowCurrentScene();
    }
}