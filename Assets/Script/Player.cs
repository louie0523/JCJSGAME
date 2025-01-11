using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f; // 이동 속도
    public Collider2D attackBoxCollider; // AttackBox 콜라이더

    private Rigidbody2D rb; // 플레이어의 Rigidbody2D
    private Vector2 movement; // 이동 방향

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D 컴포넌트를 가져옴
    }

    void Update()
    {
        // WASD로 플레이어의 이동 방향 설정
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D로 X축 이동
        movement.y = Input.GetAxisRaw("Vertical");   // W/S로 Y축 이동
    }

    void FixedUpdate()
    {
        // 이동 후 플레이어가 AttackBox 콜라이더 내에 있는지 확인
        MovePlayer();
    }

    void MovePlayer()
    {
        // 이동 벡터를 구함
        Vector2 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;

        // AttackBox 콜라이더의 범위 내에 있을 때만 이동하도록 제한
        if (IsInsideAttackBox(newPosition))
        {
            rb.MovePosition(newPosition); // 플레이어 이동
        }
    }

    bool IsInsideAttackBox(Vector2 position)
    {
        // AttackBox의 경계 내에 위치하는지 확인
        return attackBoxCollider.bounds.Contains(position);
    }
}
