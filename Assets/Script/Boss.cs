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
    [SerializeField] private GameObject projectilePrefab; // �߻�ü ������
    [SerializeField] private GameObject LaygerProjectilePrefab; // ������ ������
    [SerializeField] private GameObject CircleBulletPrefab;
    [SerializeField] private GameObject VirusShark;
    [SerializeField] private GameObject LeftHandPrefab;
    [SerializeField] private GameObject RightHandPrefab;
    [SerializeField] private Transform leftHand; // ������ ���� ��
    [SerializeField] private Transform rightHand; // ������ ������ ��
    [SerializeField] private Transform CircleBulletStart;
    public bool BossNoHitDamage = false;
    private bool isPhaseTransitioning = false; // ������ ��ȯ ����
    //public GameObject[] Eyes;
    public AudioClip[] LineAttackSound; // �Ҹ� ���� �迭
    private AudioSource audioSource; // AudioSource ������Ʈ
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
                // �÷��̾��� ���� �� ó��
                attackBoxAnimator.SetBool("PlayerTurn", true);
                BossAnimator.SetBool("BossTurn", false);
                HpBar.SetActive(true);
                lineAttack.lineDuration = 1f;

                PlayerAnimator.SetBool("Hide", true);
                yield return new WaitForSeconds(0.4f);
                Player.SetActive(false);
                player.NoHitDamage = false;

                yield return new WaitForSeconds(playerAttackTime - 0.4f);

                // �� ��ü
                isPlayerTurn = false;
                attackBoxAnimator.SetBool("PlayerTurn", false);
                BossAnimator.SetBool("BossTurn", true);
            }
            else
            {
                if (isPhaseTransitioning)
                {
                    Debug.Log("������ ��ȯ ��, ���� ������ �Ͻ� �ߴ��մϴ�.");
                    yield return new WaitUntil(() => !isPhaseTransitioning);
                }

                // ������ ������ ó��
                Debug.Log($"������ ������ ���� ������: {bossPhase}");
                HpBar.SetActive(false);
                lineAttack.lineDuration = 0.25f;
                Player.transform.position = new Vector3(0, -4f, 0);
                Player.SetActive(true);
                player.NoHitDamage = true;
                yield return new WaitForSeconds(0.5f);
                player.NoHitDamage = false;
                PlayerAnimator.SetBool("Hide", false);

                // ���� ���� ���� ����
                yield return StartCoroutine(HandleBossTurn());

                yield return new WaitUntil(() => !isPhaseTransitioning); // ������ ��ȯ �Ϸ� ���

                isPlayerTurn = true;
            }

            if (GameManager.Instance.BossHP <= 0)
            {
                HandleBossDeath();
                yield break;
            }
        }

        Debug.Log("���� ����!");
    }





    IEnumerator HandlePhaseTransition()
    {
        Debug.Log($"������ ��ȯ ����: ���� ������ = {bossPhase}, PhaseAni = {PhaseAni}");

        if (bossPhase == 2 && PhaseAni == 1) // 2������ ��ȯ
        {
            isPhaseTransitioning = true;
            BossNoHitDamage = true;
            Phase2.SetActive(true);
            CircleBg.SetActive(true);

            // �ִϸ��̼��� ����
            yield return new WaitForSeconds(1.5f);
            bulletSpeed += 5f;
            CirclebulletSpeed += 4f;
            BulletCountNum += 2;
            TotalCountNum += 5;
            DBB = 0.025f;
            BossNoHitDamage = false;
            Debug.Log("2������� ��ȯ �Ϸ�");
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
        Debug.Log("������ 1������ ������ �غ��մϴ�.");

        // ������ ��ȯ�ɵ��� ��� ���
        int randomPattern = Random.Range(1, 4); // 1, 2, 3 �� �ϳ��� �������� ����

        switch (randomPattern)
        {
            case 1:
                Debug.Log("������ 1������ ù ��° ������ �����մϴ�.");
                yield return StartCoroutine(Pattern1());
                break;

            case 2:
                Debug.Log("������ 1������ �� ��° ������ �����մϴ�.");
                yield return StartCoroutine(Pattern2());
                break;

            case 3:
                Debug.Log("������ 1������ �� ��° ������ �����մϴ�.");
                yield return StartCoroutine(Pattern3());
                break;
            case 4:
                Debug.Log("������ 1������ 4 ��° ������ �����մϴ�.");
                yield return StartCoroutine(Pattern4());
                break;
        }

        // 10�� ���� �߻�ü�� ��� ���� �� ���
        yield return new WaitForSeconds(bossAttackTime);
    }

    IEnumerator Pattern1()
    {

        float attackDuration = 10f; // �߻�ü�� ������ ���� �ð�
        float elapsedTime = 0f;

        while (elapsedTime < attackDuration)
        {
            // ��տ��� �߻�ü�� �߻�
            ShootProjectile(leftHand.position);
            ShootProjectile(rightHand.position);

            elapsedTime += 0.3f; // 1�ʸ��� �߻�
            yield return new WaitForSeconds(0.3f); // 1�� ���
        }
    }

    void ShootProjectile(Vector3 spawnPosition)
    {
        // �߻�ü ������Ʈ ����
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        // �÷��̾� �������� �߻�ü �̵�
        Vector2 direction = (Player.transform.position - spawnPosition).normalized;
        rb.velocity = direction * bulletSpeed; // �߻�ü �ӵ� ���� (��: 5f)
    }

    IEnumerator Pattern2()
    {
        BossAnimator.SetInteger("Pattern", 2); // ���� �ִϸ��̼� ����
        Layger.SetActive(true); // ������ Ȱ��ȭ

        Vector3 initialPosition = transform.position; // ������ �ʱ� ��ġ ����
        int count = 0; // �ݺ� Ƚ��

        while (count < 5) // 5�� �ݺ�
        {
            // 1. �÷��̾��� x��ǥ�� �̵�
            float targetX = Player.transform.position.x; // ��ǥ x��ǥ
            Vector3 targetPosition = new Vector3(targetX, transform.position.y, transform.position.z); // ��ǥ ��ġ
            float moveDuration = 0.5f; // �̵� �ð� (0.5��)
            float elapsedTime = 0f;

            // ������ ��ǥ ��ġ�� �ε巴�� �̵�
            while (elapsedTime < moveDuration)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, elapsedTime / moveDuration);
                elapsedTime += Time.deltaTime;
                yield return null; // ���� ������ ���
            }

            // ��Ȯ�� ��ǥ ��ġ�� ����
            transform.position = targetPosition;

            Debug.Log($"������ �÷��̾��� x��ǥ {targetX}�� �̵��߽��ϴ�.");

            // 2. �߻� ���� ��� ����
            yield return new WaitForSeconds(0.6f); // 1�� ���� ����

            // 3. ���������� �簢�� �Ѿ� �߻�
            Debug.Log($"������ x��ǥ {targetX}���� ���������� �簢�� �Ѿ��� �߻��մϴ�.");
            ShootProjectileFromLaser(Layger.transform.position); // ���������� �Ѿ� �߻�

            yield return new WaitForSeconds(0.5f);

            count++; // �ݺ� Ƚ�� ����
        }

        Layger.SetActive(false); // ������ ��Ȱ��ȭ
        BossAnimator.SetInteger("Pattern", 0); // ���� �ִϸ��̼� �ʱ�ȭ
        Debug.Log("���� 2 ����");

        // 4. ���� ���� �� ���� ��ġ�� ����
        float returnDuration = 1f; // ���� �ð�
        float returnElapsedTime = 0f;

        while (returnElapsedTime < returnDuration)
        {
            transform.position = Vector3.Lerp(transform.position, initialPosition, returnElapsedTime / returnDuration);
            returnElapsedTime += Time.deltaTime;
            yield return null; // ���� ������ ���
        }

        transform.position = initialPosition; // ��Ȯ�� �ʱ� ��ġ�� ����
        Debug.Log("������ ���� ��ġ�� ���ƿԽ��ϴ�.");
    }

    void ShootProjectileFromLaser(Vector3 spawnPosition)
    {
        GameObject projectile = Instantiate(LaygerProjectilePrefab, spawnPosition, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        Vector2 direction = Vector2.down; // �Ʒ� ����
        rb.velocity = direction * LaygerSpeed; // �߻�ü �ӵ� ���� (��: 5f)
    }

    IEnumerator Pattern3()
    {
        Vector3 startPosition = CircleBulletStart.position; // CircleBulletStart ��ġ
        BossAnimator.SetInteger("Pattern", 3);
        yield return new WaitForSeconds(1f);

        int bulletCount = BulletCountNum; // �� ���� 10���� �Ѿ�
        float radius = 3f; // ���� ������
        float delayBetweenBullets = DBB; // �Ѿ� �߻� ����
        int totalRounds = TotalCountNum; // �� 10�� �߻�
        float rotationSpeed = 30f; // ȸ�� �ӵ� (1�ʿ� 30�� ȸ��)

        for (int round = 0; round < totalRounds; round++)
        {
            // ���� �Ѿ˵��� �߻�Ǵ� ���� ���
            float angleStep = 360f / bulletCount; // �� ������ �� �Ѿ� ������ ���� ����

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = i * angleStep + (rotationSpeed * round); // ȸ�� ���� �߰� (�� ȸ������ ����)

                // ���� ��ǥ ���
                float xOffset = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
                float yOffset = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;

                // �Ѿ� �߻� ��ġ
                Vector3 spawnPosition = startPosition + new Vector3(xOffset, yOffset, 0f);
                ShootCircleBullet(spawnPosition, angle);

                yield return new WaitForSeconds(delayBetweenBullets); // �� �Ѿ� �߻� ����
            }

            yield return new WaitForSeconds(delayBetweenBullets); // �� �ϳ� �߻� �� ���
        }
        BossAnimator.SetInteger("Pattern", 0);
        Debug.Log("���� 3 ����");
    }

    void ShootCircleBullet(Vector3 spawnPosition, float angle)
    {
        // ���� �Ѿ� �߻�
        GameObject projectile = Instantiate(CircleBulletPrefab, spawnPosition, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        // ������ �´� ���� ���
        Vector2 direction = new Vector2(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle));

        rb.velocity = direction * CirclebulletSpeed; // �Ѿ� �ӵ� ����
    }


    IEnumerator Pattern4()
    {
        Debug.Log("������ 4������ ������ �غ��մϴ�.");
        BossAnimator.SetInteger("Pattern", 4);
        yield return new WaitForSeconds(2f);
        // �÷��̾ �������� �������� ��ġ�� VirusShark�� ��
        int sharkCount = 10;
        float radius = 5f; // ���� ������ (�÷��̾���� �Ÿ�)
        float delayBetweenSharks = 0.3f; // ��� ���� �߻� ����
        float sharkSpeed = bulletSpeed * 2f; // �� �÷��̾ ���� �޷����� �ӵ�

        // �÷��̾� �ֺ��� VirusShark�� �������� ��ġ
        List<GameObject> sharks = new List<GameObject>();
        for (int i = 0; i < sharkCount; i++)
        {
            // ���� �迭 ��ġ ���
            float angle = i * (360f / sharkCount); // �� ����� ����
            float xOffset = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            float yOffset = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            Vector3 spawnPosition = Player.transform.position + new Vector3(xOffset, yOffset, 0f);

            // VirusShark �ν��Ͻ� ����
            GameObject shark = Instantiate(VirusShark, spawnPosition, Quaternion.identity);

            // �÷��̾ ���ϵ��� ��� ȸ�� ����
            Vector3 directionToPlayer = (Player.transform.position - shark.transform.position).normalized;
            float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

            // ����� '��'�� �÷��̾ ���ϰ� �ϱ� ���� -90���� ����
            shark.transform.rotation = Quaternion.Euler(0, 0, angleToPlayer - 90);

            sharks.Add(shark);

            // ��� ���
            yield return new WaitForSeconds(delayBetweenSharks);
        }
        BossAnimator.SetInteger("Pattern", 0);
        // ������ ���ʴ�� �÷��̾ ���� ����
        foreach (var shark in sharks)
        {
            // �� �� �÷��̾�� �̵�
            StartCoroutine(MoveSharkTowardsPlayer(shark, sharkSpeed));
            yield return new WaitForSeconds(0.5f); // ���� ���� �ð� ���̸� �ΰ� ���� ����
        }
        yield return new WaitForSeconds(bossAttackTime); // ���� ���� �ð�
    }



    IEnumerator MoveSharkTowardsPlayer(GameObject shark, float speed)
    {
        while (true) // ���� ����
        {
            // �ٶ󺸴� �������� ������ �̵�
            shark.transform.position += shark.transform.up * speed * Time.deltaTime;

            yield return null; // �� ������ ���
        }
    }

    IEnumerator Pattern5()
    {
        BossAnimator.SetInteger("Pattern", 5);
        yield return new WaitForSeconds(1f);
        float attackDuration = 10f; // �߻�ü�� ������ ���� �ð�
        float elapsedTime = 0f;
        while (elapsedTime < attackDuration)
        {
            // ���� �� ����
            BossAnimator.SetInteger("Hands", 0);
            yield return new WaitForSeconds(1f);
            GameObject LHand = Instantiate(LeftHandPrefab, leftHand.position, Quaternion.identity);
            Vector3 directionToPlayer = (Player.transform.position - LHand.transform.position).normalized;
            float angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

            // �Ʒ� �������� �÷��̾ ���ϵ��� ȸ��
            LHand.transform.rotation = Quaternion.Euler(0, 0, angleToPlayer - 90);  // +90�� �߰��Ͽ� �Ʒ� �������� ȸ��

            StartCoroutine(MoveHandForward(LHand, bulletSpeed * 3f)); // ���� �ӵ��� ���� �̵���Ŵ
            elapsedTime += 1f; // �߻� �� ��� �ð� 2��
            yield return new WaitForSeconds(1f); // 1�� ��� �� ����

            // ������ �� ����
            BossAnimator.SetInteger("Hands", 1);
            yield return new WaitForSeconds(1f);
            GameObject Hand = Instantiate(RightHandPrefab, rightHand.position, Quaternion.identity);
            directionToPlayer = (Player.transform.position - Hand.transform.position).normalized;
            angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

            // �Ʒ� �������� �÷��̾ ���ϵ��� ȸ��
            Hand.transform.rotation = Quaternion.Euler(0, 0, angleToPlayer - 90);  // +90�� �߰��Ͽ� �Ʒ� �������� ȸ��

            StartCoroutine(MoveHandForward(Hand, bulletSpeed * 3f)); // ���� �ӵ��� ���� �̵���Ŵ
            elapsedTime += 1f; // �߻� �� ��� �ð� 2��
            yield return new WaitForSeconds(1f); // 1�� ��� �� ����
        }
    }

    IEnumerator MoveHandForward(GameObject hand, float speed)
    {
        while (true) // ���� ����, ��� �̵��ϵ���
        {
            // �ٶ󺸴� �������� �̵�
            hand.transform.position += hand.transform.up * speed * Time.deltaTime;

            yield return null; // �� ������ ���
        }
    }




    IEnumerator BossPhase2Pattern()
    {
        Debug.Log("������ 2������ ������ �غ��մϴ�.");

        // ������ ��ȯ�ɵ��� ��� ���
        int randomPattern = Random.Range(5, 6); // 1, 2, 3 �� �ϳ��� �������� ����

        switch (randomPattern)
        {
            case 1:
                Debug.Log("������ 2������ ù ��° ������ �����մϴ�.");
                yield return StartCoroutine(Pattern1());
                break;

            case 2:
                Debug.Log("������ 2������ �� ��° ������ �����մϴ�.");
                yield return StartCoroutine(Pattern2());
                break;

            case 3:
                Debug.Log("������ 2������ �� ��° ������ �����մϴ�.");
                yield return StartCoroutine(Pattern3());
                break;
            case 4:
                Debug.Log("������ 2������ 4 ��° ������ �����մϴ�.");
                yield return StartCoroutine(Pattern4());
                break;
            case 5:
                Debug.Log("������ 2������ 5 ��° ������ �����մϴ�.");
                yield return StartCoroutine(Pattern5());
                break;
        }

        // 10�� ���� �߻�ü�� ��� ���� �� ���
        yield return new WaitForSeconds(bossAttackTime);
    }


    public void TakeDamage(float damage)
    {
        // GameManager���� ���� ü���� ������Ʈ
        GameManager.Instance.UpdateBossHP(damage);

        if (GameManager.Instance.BossHP <= 0)
        {
            HandleBossDeath();
        }
    }

    public void HandleBossDeath()
    {
        Debug.Log("������ �׾����ϴ�!");
        Destroy(BGM);
        StartCoroutine("Wait");
        Destroy(gameObject);
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(1f);
    }
}
