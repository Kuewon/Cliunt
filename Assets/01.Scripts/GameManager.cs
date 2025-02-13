using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update

    public int totalGold = 0;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI stageText;
    public GameObject damageTextObj;

    private void Awake()
    {
        totalGold = (int)PlayerPrefs.GetFloat($"USER_GOLD", 0);
        goldText.SetText($"{totalGold.ToString()}");
    }

    void Start()
    {
        WaveManager.Instance.OnStageChanged += OnUpdateStage;
        OnUpdateStage(WaveManager.Instance.GetCurrentStage());
    }

    // Update is called once per frame
    void Update()
    {
       // if(Input.GetMouseButtonDown(0))
       // {
        //    Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //    RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        //    if (hit.collider != null)
        //    {
        //        Debug.Log($"TEST :: Name :: {hit.collider.gameObject.name}");
       //     }
       // }
        
    }

    public void OnUpdateGold(int gold)
    {
        totalGold += gold;
        goldText.SetText($"{totalGold.ToString()}");
        PlayerPrefs.SetFloat("USER_GOLD", totalGold);
    }
    
    public void OnUpdateStage(int stage)
    {
        stageText.SetText($"Stage : {stage.ToString()}");
        PlayerPrefs.SetFloat("USER_STAGE", stage);
    }
    
    void OnDestroy()
    {
        // Unsubscribe from the event when the GameManager is destroyed
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnStageChanged -= OnUpdateStage;
        }
    }
}
