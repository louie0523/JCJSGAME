using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f; // �̵� �ӵ�
    public Collider2D attackBoxCollider; // AttackBox �ݶ��̴�

    private Rigidbody2D rb; // �÷��̾��� Rigidbody2D
    private Vector2 movement; // �̵� ����

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D ������Ʈ�� ������
    }

    void Update()
    {
        // WASD�� �÷��̾��� �̵� ���� ����
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D�� X�� �̵�
        movement.y = Input.GetAxisRaw("Vertical");   // W/S�� Y�� �̵�
    }

    void FixedUpdate()
    {
        // �̵� �� �÷��̾ AttackBox �ݶ��̴� ���� �ִ��� Ȯ��
        MovePlayer();
    }

    void MovePlayer()
    {
        // �̵� ���͸� ����
        Vector2 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;

        // AttackBox �ݶ��̴��� ���� ���� ���� ���� �̵��ϵ��� ����
        if (IsInsideAttackBox(newPosition))
        {
            rb.MovePosition(newPosition); // �÷��̾� �̵�
        }
    }

    bool IsInsideAttackBox(Vector2 position)
    {
        // AttackBox�� ��� ���� ��ġ�ϴ��� Ȯ��
        return attackBoxCollider.bounds.Contains(position);
    }
}
