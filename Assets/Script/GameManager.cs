using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int PlayerHP = 100;
    public float BossMaxHp = 1000f;
    public float BossHP;

    [SerializeField] private Slider bossHPSlider;

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F))
        {
            BossHP -= 10f;
            UpdateBossHP(BossHP);
            Debug.Log("보스에게 10 데미지!");
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    private void Start()
    {
        if (bossHPSlider != null)
        {
            bossHPSlider.maxValue = BossHP;
            bossHPSlider.value = BossHP;
        }
        BossHP = BossMaxHp;
        UpdateBossHP(BossHP);  
    }

    public void UpdateBossHP(float newHP)
    {
        BossHP = Mathf.Clamp(newHP, 0f, bossHPSlider.maxValue);
        if (bossHPSlider != null)
        {
            bossHPSlider.value = BossHP;
        }
    }
}
