using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public GameObject Player;
    public GameObject HpBar;
    public float playerAttackTime = 15f;
    public float bossAttackTime = 30f;
    public bool isPlayerTurn = true;
    private int bossPhase = 1;
    public float bulletSpeed = 5f;
    public float LaygerSpeed = 20f;
    public float CirclebulletSpeed = 5f;
    public int BulletCountNum = 10;
    public int TotalCountNum = 10;
    public float DBB = 0.05f;
    public LineAttack lineAttack;
    [SerializeField] private GameObject attackBox;
    [SerializeField] private GameObject projectilePrefab; // 발사체 프리팹
    [SerializeField] private GameObject LaygerProjectilePrefab; // 레이저 프리팹
    [SerializeField] private GameObject CircleBulletPrefab;
    [SerializeField] private GameObject VirusShark;
    [SerializeField] private GameObject LeftHandPrefab;
    [SerializeField] private GameObject RightHandPrefab;
    [SerializeField] private Transform leftHand; // 보스의 왼쪽 손
    [SerializeField] private Transform rightHand; // 보스의 오른쪽 손
    [SerializeField] private Transform CircleBulletStart;
    public bool BossNoHitDamage = false;
    private bool isPhaseTransitioning = false; // 페이즈 전환 상태
    //public GameObject[] Eyes;
    public AudioClip[] LineAttackSound; // 소리 파일 배열
    private AudioSource audioSource; // AudioSource 컴포넌트
    public GameObject BGM;

    private int PhaseAni = 0;
    public GameObject Phase2;
    public GameObject RealPhase2;
    public GameObject CircleBg;

    public GameObject Layger;
    public bool LaygetAttackCheck = false;

    private Animator attackBoxAnimator;
    private Animator BossAnimator;
    private Animator PlayerAnimator;

    private Player player;

    void Start()
    {
        Layger.SetActive(false);
        attackBoxAnimator = attackBox.GetComponent<Animator>();
        BossAnimator = this.gameObject.GetComponent<Animator>();
        PlayerAnimator = Player.GetComponent<Animator>();
        player = Player.GetComponent<Player>();
        audioSource = gameObject.AddComponent<AudioSource>();

        StartCoroutine(BattleLoop());
    }
    void Update()
    {
        // Continuously check the boss's phase based on current HP
        UpdateBossPhase();
    }

    private void PlaySound(int soundIndex)
    {
        if (LineAttackSound != null && LineAttackSound.Length > 0 && soundIndex < LineAttackSound.Length)
        {
            audioSource.PlayOneShot(LineAttackSound[soundIndex]);
        }
    }

    void UpdateBossPhase()
    {
        float maxHP = GameManager.Instance.BossMaxHp;
        float currentHP = GameManager.Instance.BossHP;

        int previousPhase = bossPhase; // Save the previous phase to check if it changes

        if (currentHP <= maxHP * 0.6f)
        {
            bossPhase = 2;
            if(PhaseAni == 0)
            {
                BossAnimator.SetTrigger("Phase2");
                PhaseAni++;
            }
        }
        else
            bossPhase = 1;

        if (previousPhase != bossPhase)
        {
            // Trigger phase transition only if phase has changed
            StartCoroutine(HandlePhaseTransition());
        }
    }
    IEnumerator BattleLoop()
    {
        while (GameManager.Instance.BossHP > 0)
        {
            if (isPlayerTurn)
            {
                // 플레이어의 공격 턴 처리
                attackBoxAnimator.SetBool("PlayerTurn", true);
                BossAnimator.SetBool("BossTurn", false);
                HpBar.SetActive(true);
                lineAttack.lineDuration = 1f;

                PlayerAnimator.SetBool("Hide", true);
                yield return new WaitForSeconds(0.4f);
                Player.SetActive(false);
                player.NoHitDamage = false;

                yield return new WaitForSeconds(playerAttackTime - 0.4f);

                // 턴 교체
                isPlayerTurn = false;
                attackBoxAnimator.SetBool("PlayerTurn", false);
                BossAnimator.SetBool("BossTurn", true);
            }
            else
            {
                if (isPhaseTransitioning)
                {
                    Debug.Log("페이즈 전환 중, 보스 패턴을 일시 중단합니다.");
                    yield return new WaitUntil(() => !isPhaseTransitioning);
                }

                // 보스의 공격턴 처리
                Debug.Log($"보스의 공격턴 현재 페이즈: {bossPhase}");
                HpBar.SetActive(false);
                lineAttack.lineDuration = 0.25f;
                Player.transform.position = new Vector3(0, -4f, 0);
                Player.SetActive(true);
                player.NoHitDamage = true;
                yield return new WaitForSeconds(0.5f);
                player.NoHitDamage = false;
                PlayerAnimator.SetBool("Hide", false);

                // 보스 공격 패턴 실행
                yield return StartCoroutine(HandleBossTurn());

                yield return new WaitUntil(() => !isPhaseTransitioning); // 페이즈 전환 완료 대기

                isPlayerTurn = true;
            }

            if (GameManager.Instance.BossHP <= 0)
            {
                HandleBossDeath();
                yield break;
            }
        }

        Debug.Log("전투 종료!");
    }





    IEnumerator HandlePhaseTransition()
    {
        Debug.Log($"페이즈 전환 시작: 현재 페이즈 = {bossPhase}, PhaseAni = {PhaseAni}");

        if (bossPhase == 2 && PhaseAni == 1) // 2페이즈 전환
        {
            isPhaseTransitioning = true;
            BossNoHitDamage = true;
            Phase2.SetActive(true);
            CircleBg.SetActive(true);

            // 애니메이션을 실행
            yield return new WaitForSeconds(1.5f);
            bulletSpeed += 5f;
            CirclebulletSpeed += 4f;
            BulletCountNum += 2;
            TotalCountNum += 5;
            DBB = 0.025f;
            BossNoHitDamage = false;
            Debug.Log("2페이즈로 전환 완료");
            isPhaseTransitioning = false;

            CircleBg.SetActive(false);
            Phase2.SetActive(false);
            RealPhase2.SetActive(true);
        }
       
    }




    IEnumerator HandleBossTurn()
    {
        switch (bossPhase)
        {
            case 1:
                yield return BossPhase1Pattern();
                break;
            case 2:
                yield return BossPhase2Pattern();
                break;
        }
    }

    IEnumerator BossPhase1Pattern()
    {
        Debug.Log("보스가 1페이즈 패턴을 준비합니다.");

        // 페이즈 변환될동안 잠시 대기
        int randomPattern = Random.Range(1, 4); // 1, 2, 3 중 하나를 랜덤으로 선택

        switch (randomPattern)
        {
            case 1:
                Debug.Log("보스가 1페이즈 첫 번째 패턴을 실행합니다.");
                yield return StartCoroutine(Pattern1());
                break;

            case 2:
                Debug.Log("보스가 1페이즈 두 번째 패턴을 실행합니다.");
                yield return StartCoroutine(Pattern2());
                break;

            case 3:
                Debug.Log("보스가 1페이즈 세 번째 패턴을 실행합니다.");
                yield return StartCoroutine(Pattern3());
                break;
            case 4:
                Debug.Log("보스가 1페이즈 4 번째 패턴을 실행합니다.");
                yield return StartCoroutine(Pattern4());
                break;
        }

        // 10초 동안 발사체를 계속 보낸 후 대기
        yield return new WaitForSeconds(bossAttackTime);
    }

    IEnumerator Pattern1()
    {

        float attackDuration = 10f; // 발사체를 보내는 지속 시간
        float elapsedTime = 0f;

        while (elapsedTime < attackDuration)
        {
            // 양손에서 발사체를 발사
            ShootProjectile(leftHand.position);
            ShootProjectile(rightHand.position);

            elapsedTime += 0.3f; // 1초마다 발사
            yield return new WaitForSeconds(0.3f); // 1초 대기
        }
    }

    void ShootProjectile(Vector3 spawnPosition)
    {
        // 발사체 오브젝트 생성
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        // 플레이어 방향으로 발사체 이동
        Vector2 direction = (Player.transform.position - spawnPosition).normalized;
        rb.velocity = direction * bulletSpeed; // 발사체 속도 설정 (예: 5f)
    }

    IEnumerator Pattern2()
    {
        BossAnimator.SetInteger("Pattern", 2); // 보스 애니메이션 설정
        Layger.SetActive(true); // 레이저 활성화

        Vector3 initialPosition = transform.position; // 보스의 초기 위치 저장
        int count = 0; // 반복 횟수

        while (count < 5) // 5번 반복
        {
            // 1. 플레이어의 x좌표로 이동
            float targetX = Player.transform.position.x; // 목표 x좌표
            Vector3 targetPosition = new Vector3(targetX, transform.position.y, transform.position.z); // 목표 위치
            float moveDuration = 0.5f; // 이동 시간 (0.5초)
            float elapsedTime = 0f;

            // 보스가 목표 위치로 부드럽게 이동
            while (elapsedTime < moveDuration)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, elapsedTime / moveDuration);
                elapsedTime += Time.deltaTime;
                yield return null; // 다음 프레임 대기
            }

            // 정확히 목표 위치로 설정
            transform.position = targetPosition;

            Debug.Log($"보스가 플레이어의 x좌표 {targetX}로 이동했습니다.");

            // 2. 발사 전에 잠깐 멈춤
            yield return new WaitForSeconds(0.6f); // 1초 동안 멈춤

            // 3. 레이저에서 사각형 총알 발사
            Debug.Log($"보스가 x좌표 {targetX}에서 레이저에서 사각형 총알을 발사합니다.");
            ShootProjectileFromLaser(Layger.transform.position); // 레이저에서 총알 발사

            yield return new WaitForSeconds(0.5f);

            count++; // 반복 횟수 증가
        }

        Layger.SetActive(false); // 레이저 비활성화
        BossAnimator.SetInteger("Pattern", 0); // 패턴 애니메이션 초기화
        Debug.Log("패턴 2 종료");

        // 4. 패턴 종료 후 기존 위치로 복귀
        float returnDuration = 1f; // 복귀 시간
        float returnElapsedTime = 0f;

        while (returnElapsedTime < returnDuration)
        {
            transform.position = Vector3.Lerp(transform.position, initialPosition, returnElapsedTime / returnDuration);
            returnElapsedTime += Time.deltaTime;
            yield return null; // 다음 프레임 대기
        }

        transform.position = initialPosition; // 정확한 초기 위치로 설정
        Debug.Log("보스가 원래 위치로 돌아왔습니다.");
    }

    void ShootProjectileFromLaser(Vector3 spawnPosition)
    {
        GameObject projectile = Instantiate(LaygerProjectilePrefab, spawnPosition, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        Vector2 direction = Vector2.down; // 아래 방향
        rb.velocity = direction * LaygerSpeed; // 발사체 속도 설정 (예: 5f)
    }

    IEnumerator Pattern3()
    {
        Vector3 startPosition = CircleBulletStart.position; // CircleBulletStart 위치
        BossAnimator.SetInteger("Pattern", 3);
        yield return new WaitForSeconds(1f);

        int bulletCount = BulletCountNum; // 한 원에 10개의 총알
        float radius = 3f; // 원의 반지름
        float delayBetweenBullets = DBB; // 총알 발사 간격
        int totalRounds = TotalCountNum; // 총 10번 발사
        float rotationSpeed = 30f; // 회전 속도 (1초에 30도 회전)

        for (int round = 0; round < totalRounds; round++)
        {
            // 원의 총알들이 발사되는 각도 계산
            float angleStep = 360f / bulletCount; // 한 원에서 각 총알 사이의 각도 차이

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = i * angleStep + (rotationSpeed * round); // 회전 각도 추가 (각 회차마다 증가)

                // 원형 좌표 계산
                float xOffset = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
                float yOffset = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;

                // 총알 발사 위치
                Vector3 spawnPosition = startPosition + new Vector3(xOffset, yOffset, 0f);
                ShootCircleBullet(spawnPosition, angle);

                yield return new WaitForSeconds(delayBetweenBullets); // 각 총알 발사 간격
            }

            yield return new WaitForSeconds(delayBetweenBullets); // 원 하나 발사 후 대기
        }
        BossAnimator.SetInteger("Pattern", 0);
        Debug.Log("패턴 3 종료");
    }

    void ShootCircleBullet(Vector3 spawnPosition, float angle)
    {
        // 원형 총알 발사
        GameObject projectile = Instantiate(CircleBulletPrefab, spawnPosition, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        // 각도에 맞는 방향 계산
        Vector2 direction = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle));

        rb.velocity = direction * CirclebulletSpeed; // 총알 속도 설정
    }


    IEnumerator Pattern4()
    {
        Debug.Log("보스가 4페이즈 패턴을 준비합니다.");
        BossAnimator.SetInteger("Pattern", 4);
        yield return new WaitForSeconds(2f);
        // 플레이어를 기준으로 원형으로 배치될 VirusShark의 수
        int sharkCount = 10;
        float radius = 5f; // 원의 반지름 (플레이어와의 거리)
        float delayBetweenSharks = 0.3f; // 상어 간의 발사 간격
        float sharkSpeed = bulletSpeed * 2f; // 상어가 플레이어를 향해 달려가는 속도

        // 플레이어 주변에 VirusShark를 원형으로 배치
        List<GameObject> sharks = new List<GameObject>();
        for (int i = 0; i < sharkCount; i++)
        {
            // 원형 배열 위치 계산
            float angle = i * (360f / sharkCount); // 각 상어의 각도
            float xOffset = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            float yOffset = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            Vector3 spawnPosition = Player.transform.position + new Vector3(xOffset, yOffset, 0f);

            // VirusShark 인스턴스 생성
            GameObject shark = Instantiate(VirusShark, spawnPosition, Quaternion.identity);

            // 플레이어를 향하도록 상어 회전 설정
            Vector3 directionToPlayer = (Player.transform.position - shark.transform.position).normalized;
            float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

            // 상어의 '위'가 플레이어를 향하게 하기 위해 -90도를 조정
            shark.transform.rotation = Quaternion.Euler(0, 0, angleToPlayer - 90);

            sharks.Add(shark);

            // 잠시 대기
            yield return new WaitForSeconds(delayBetweenSharks);
        }
        BossAnimator.SetInteger("Pattern", 0);
        // 상어들이 차례대로 플레이어를 향해 돌진
        foreach (var shark in sharks)
        {
            // 각 상어가 플레이어로 이동
            StartCoroutine(MoveSharkTowardsPlayer(shark, sharkSpeed));
            yield return new WaitForSeconds(0.5f); // 상어마다 일정 시간 차이를 두고 돌진 시작
        }
        yield return new WaitForSeconds(bossAttackTime); // 패턴 지속 시간
    }



    IEnumerator MoveSharkTowardsPlayer(GameObject shark, float speed)
    {
        while (true) // 무한 루프
        {
            // 바라보는 방향으로 앞으로 이동
            shark.transform.position += shark.transform.up * speed * Time.deltaTime;

            yield return null; // 한 프레임 대기
        }
    }

    IEnumerator Pattern5()
    {
        BossAnimator.SetInteger("Pattern", 5);
        yield return new WaitForSeconds(1f);
        float attackDuration = 10f; // 발사체를 보내는 지속 시간
        float elapsedTime = 0f;
        while (elapsedTime < attackDuration)
        {
            // 왼쪽 손 공격
            BossAnimator.SetInteger("Hands", 0);
            yield return new WaitForSeconds(1f);
            GameObject LHand = Instantiate(LeftHandPrefab, leftHand.position, Quaternion.identity);
            Vector3 directionToPlayer = (Player.transform.position - LHand.transform.position).normalized;
            float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

            // 아래 방향으로 플레이어를 향하도록 회전
            LHand.transform.rotation = Quaternion.Euler(0, 0, angleToPlayer - 90);  // +90을 추가하여 아래 방향으로 회전

            StartCoroutine(MoveHandForward(LHand, bulletSpeed * 3f)); // 일정 속도로 손을 이동시킴
            elapsedTime += 1f; // 발사 후 대기 시간 2초
            yield return new WaitForSeconds(1f); // 1초 대기 후 공격

            // 오른쪽 손 공격
            BossAnimator.SetInteger("Hands", 1);
            yield return new WaitForSeconds(1f);
            GameObject Hand = Instantiate(RightHandPrefab, rightHand.position, Quaternion.identity);
            directionToPlayer = (Player.transform.position - Hand.transform.position).normalized;
            angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

            // 아래 방향으로 플레이어를 향하도록 회전
            Hand.transform.rotation = Quaternion.Euler(0, 0, angleToPlayer - 90);  // +90을 추가하여 아래 방향으로 회전

            StartCoroutine(MoveHandForward(Hand, bulletSpeed * 3f)); // 일정 속도로 손을 이동시킴
            elapsedTime += 1f; // 발사 후 대기 시간 2초
            yield return new WaitForSeconds(1f); // 1초 대기 후 공격
        }
    }

    IEnumerator MoveHandForward(GameObject hand, float speed)
    {
        while (true) // 무한 루프, 계속 이동하도록
        {
            // 바라보는 방향으로 이동
            hand.transform.position += hand.transform.up * speed * Time.deltaTime;

            yield return null; // 한 프레임 대기
        }
    }




    IEnumerator BossPhase2Pattern()
    {
        Debug.Log("보스가 2페이즈 패턴을 준비합니다.");

        // 페이즈 변환될동안 잠시 대기
        int randomPattern = Random.Range(5, 6); // 1, 2, 3 중 하나를 랜덤으로 선택

        switch (randomPattern)
        {
            case 1:
                Debug.Log("보스가 2페이즈 첫 번째 패턴을 실행합니다.");
                yield return StartCoroutine(Pattern1());
                break;

            case 2:
                Debug.Log("보스가 2페이즈 두 번째 패턴을 실행합니다.");
                yield return StartCoroutine(Pattern2());
                break;

            case 3:
                Debug.Log("보스가 2페이즈 세 번째 패턴을 실행합니다.");
                yield return StartCoroutine(Pattern3());
                break;
            case 4:
                Debug.Log("보스가 2페이즈 4 번째 패턴을 실행합니다.");
                yield return StartCoroutine(Pattern4());
                break;
            case 5:
                Debug.Log("보스가 2페이즈 5 번째 패턴을 실행합니다.");
                yield return StartCoroutine(Pattern5());
                break;
        }

        // 10초 동안 발사체를 계속 보낸 후 대기
        yield return new WaitForSeconds(bossAttackTime);
    }


    public void TakeDamage(float damage)
    {
        // GameManager에서 보스 체력을 업데이트
        GameManager.Instance.UpdateBossHP(damage);

        if (GameManager.Instance.BossHP <= 0)
        {
            HandleBossDeath();
        }
    }

    public void HandleBossDeath()
    {
        Debug.Log("보스가 죽었습니다!");
        Destroy(BGM);
        StartCoroutine("Wait");
        Destroy(gameObject);
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(1f);
    }
}
