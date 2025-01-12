using System.Collections;
using UnityEngine;

public class LaygerHit : MonoBehaviour
{
    public int LaygerDamage = 10;
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
        if (collision.CompareTag("Player") && !player.NoHitDamage )
        {
            Debug.Log("플레이어와 충돌");
            player.SetHp(LaygerDamage);
            Destroy(gameObject);
        }
    }
}
