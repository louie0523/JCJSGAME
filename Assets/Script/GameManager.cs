using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Boss boss;

    public float BossMaxHp = 1000f;
    public float BossHP;

    public AudioClip BossSound; // Assign the sound clip in the Inspector

    [SerializeField] private Slider bossHPSlider;
    private AudioSource audioSource; // Add a reference to the AudioSource

    void Update()
    {

        if (BossHP <= 0)
        {
            boss.HandleBossDeath();
            PlayBossSound(); // Play sound when boss dies
            SceneManager.LoadScene("Clear");
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

        audioSource = GetComponent<AudioSource>(); // Get the AudioSource component attached to the GameManager
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

    private void PlayBossSound()
    {
        if (audioSource != null && BossSound != null)
        {
            audioSource.PlayOneShot(BossSound); // Play the sound
        }
    }
}
