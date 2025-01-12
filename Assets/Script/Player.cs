using System.Collections;
using TMPro;  // TextMeshProUGUI 사용을 위해 추가
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public int PlayerHp = 10;
    public float moveSpeed = 5f; // 이동 속도
    public Collider2D attackBoxCollider; // AttackBox 콜라이더
    public bool NoHitDamage = false;
    public GameObject HpText;  // TMP UI 오브젝트
    public AudioClip[] PlayerSound; // 소리 파일 배열
    private AudioSource audioSource; // AudioSource 컴포넌트

    private Rigidbody2D rb; // 플레이어의 Rigidbody2D
    private Vector2 movement; // 이동 방향
    private SpriteRenderer[] spriteRenderers; // 모든 자식 객체의 SpriteRenderer 배열
    private Color[] originalColors; // 모든 자식 객체의 원래 색상을 저장할 배열
    private TextMeshProUGUI hpTextMesh; // TMP UI 텍스트 컴포넌트

    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>(); // Rigidbody2D 컴포넌트를 가져옴
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(); // 자식 객체들의 SpriteRenderer 컴포넌트를 모두 가져옴
        originalColors = new Color[spriteRenderers.Length]; // 원래 색상을 저장할 배열 초기화

        // 원래 색상 저장
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }

        // TextMeshProUGUI 컴포넌트 가져오기
        hpTextMesh = HpText.GetComponent<TextMeshProUGUI>();

        // AudioSource 컴포넌트 추가
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // WASD로 플레이어의 이동 방향 설정
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D로 X축 이동
        movement.y = Input.GetAxisRaw("Vertical");   // W/S로 Y축 이동

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

    public void SetHp(int damage)
    {
        PlayerHp -= damage;
        // 소리 재생 (PlayerSound 배열에서 첫 번째 소리)
        PlaySound(0);

        if (PlayerHp > 0)
        {
            NoHitDamage = true;
            StartCoroutine(HandleDamageEffect()); // 피해를 받으면 효과 시작
        }

        // 체력 업데이트 후 텍스트 갱신
        UpdateHpText();
    }

    // 체력 텍스트를 갱신하는 메서드
    void UpdateHpText()
    {
        if (hpTextMesh != null)
        {
            hpTextMesh.text = $"[ HP : {PlayerHp} ]";
        }
    }

    // 피해 효과를 처리하는 코루틴
    IEnumerator HandleDamageEffect()
    {
        // 모든 자식 객체의 SpriteRenderer의 색상을 검정색으로 설정
        Color blackColor = Color.black; // 검정색

        // 모든 자식 객체의 색상을 동시에 변경
        foreach (SpriteRenderer renderer in spriteRenderers)
        {
            renderer.color = blackColor; // 검정색으로 변경
        }

        // 검정색으로 변경한 후 0.75초 동안 유지
        yield return new WaitForSeconds(0.75f);

        // 원래 색상으로 복원
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].color = originalColors[i]; // 원래 색상으로 복원
        }

        NoHitDamage = false; // 피해 처리 후 NoHitDamage 해제
    }

    // 소리를 재생하는 메서드
    void PlaySound(int soundIndex)
    {
        if (soundIndex >= 0 && soundIndex < PlayerSound.Length)
        {
            audioSource.clip = PlayerSound[soundIndex]; // 해당 소리 파일 설정
            audioSource.Play(); // 소리 재생
        }
    }
}
