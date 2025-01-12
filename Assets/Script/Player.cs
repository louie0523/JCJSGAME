using System.Collections;
using TMPro;  // TextMeshProUGUI ����� ���� �߰�
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public int PlayerHp = 10;
    public float moveSpeed = 5f; // �̵� �ӵ�
    public Collider2D attackBoxCollider; // AttackBox �ݶ��̴�
    public bool NoHitDamage = false;
    public GameObject HpText;  // TMP UI ������Ʈ
    public AudioClip[] PlayerSound; // �Ҹ� ���� �迭
    private AudioSource audioSource; // AudioSource ������Ʈ

    private Rigidbody2D rb; // �÷��̾��� Rigidbody2D
    private Vector2 movement; // �̵� ����
    private SpriteRenderer[] spriteRenderers; // ��� �ڽ� ��ü�� SpriteRenderer �迭
    private Color[] originalColors; // ��� �ڽ� ��ü�� ���� ������ ������ �迭
    private TextMeshProUGUI hpTextMesh; // TMP UI �ؽ�Ʈ ������Ʈ

    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>(); // Rigidbody2D ������Ʈ�� ������
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(); // �ڽ� ��ü���� SpriteRenderer ������Ʈ�� ��� ������
        originalColors = new Color[spriteRenderers.Length]; // ���� ������ ������ �迭 �ʱ�ȭ

        // ���� ���� ����
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }

        // TextMeshProUGUI ������Ʈ ��������
        hpTextMesh = HpText.GetComponent<TextMeshProUGUI>();

        // AudioSource ������Ʈ �߰�
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // WASD�� �÷��̾��� �̵� ���� ����
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D�� X�� �̵�
        movement.y = Input.GetAxisRaw("Vertical");   // W/S�� Y�� �̵�

        if (PlayerHp < 0)
        {
            StartCoroutine("Wait");
            SceneManager.LoadScene("Lose");
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(1f);
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

    public void SetHp(int damage)
    {
        PlayerHp -= damage;
        // �Ҹ� ��� (PlayerSound �迭���� ù ��° �Ҹ�)
        PlaySound(0);

        if (PlayerHp > 0)
        {
            NoHitDamage = true;
            StartCoroutine(HandleDamageEffect()); // ���ظ� ������ ȿ�� ����
        }

        // ü�� ������Ʈ �� �ؽ�Ʈ ����
        UpdateHpText();
    }

    // ü�� �ؽ�Ʈ�� �����ϴ� �޼���
    void UpdateHpText()
    {
        if (hpTextMesh != null)
        {
            hpTextMesh.text = $"[ HP : {PlayerHp} ]";
        }
    }

    // ���� ȿ���� ó���ϴ� �ڷ�ƾ
    IEnumerator HandleDamageEffect()
    {
        // ��� �ڽ� ��ü�� SpriteRenderer�� ������ ���������� ����
        Color blackColor = Color.black; // ������

        // ��� �ڽ� ��ü�� ������ ���ÿ� ����
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            renderer.color = blackColor; // ���������� ����
        }

        // ���������� ������ �� 0.75�� ���� ����
        yield return new WaitForSeconds(0.75f);

        // ���� �������� ����
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].color = originalColors[i]; // ���� �������� ����
        }

        NoHitDamage = false; // ���� ó�� �� NoHitDamage ����
    }

    // �Ҹ��� ����ϴ� �޼���
    void PlaySound(int soundIndex)
    {
        if (soundIndex >= 0 && soundIndex < PlayerSound.Length)
        {
            audioSource.clip = PlayerSound[soundIndex]; // �ش� �Ҹ� ���� ����
            audioSource.Play(); // �Ҹ� ���
        }
    }
}
