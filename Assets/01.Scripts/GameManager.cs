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
    public GameObject damageTextObj;

    private void Awake()
    {
        totalGold = (int)PlayerPrefs.GetFloat($"USER_GOLD", 0);
        goldText.SetText($"Gold : {totalGold.ToString()}");
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnUpdateGold(int gold)
    {
        totalGold += gold;
        goldText.SetText($"Gold : {totalGold.ToString()}");
    }
}
