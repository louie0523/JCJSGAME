using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int Bullet_Damage = 5;
    public float LifeTime = 2f;
    private float CurrentTime = 0f;

    public Player player;

    void Start()
    {
        player = FindObjectOfType<Player>();
    }
    void Update()
    {
        CurrentTime += Time.deltaTime;
        if (CurrentTime >= LifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && !player.NoHitDamage)
        {
            player.SetHp(Bullet_Damage);
            Destroy(gameObject);
        } else if(collision.CompareTag("Line"))
        {
            Destroy(gameObject);
        }
    }
}
