using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public GameObject Player;
    public GameObject HpBar;
    public float playerAttackTime = 15f;
    public float bossAttackTime = 30f;
    private bool isPlayerTurn = true;
    private int bossPhase = 1;
    [SerializeField] private GameObject attackBox;

    private Animator attackBoxAnimator;
    private Animator BossAnimator;
    private Animator PlayerAnimator;

    void Start()
    {
        attackBoxAnimator = attackBox.GetComponent<Animator>();
        BossAnimator = this.gameObject.GetComponent<Animator>();
        PlayerAnimator = Player.GetComponent<Animator>();

        StartCoroutine(BattleLoop());
    }

    IEnumerator BattleLoop()
    {
        while (GameManager.Instance.BossHP > 0)
        {
            if (isPlayerTurn)
            {
                Debug.Log("�÷��̾��� ������");
                attackBoxAnimator.SetBool("PlayerTurn", true);
                BossAnimator.SetBool("BossTurn", false);
                HpBar.SetActive(true);
                StartCoroutine(PlayerHide());

                yield return new WaitForSeconds(playerAttackTime);
                isPlayerTurn = false;

                attackBoxAnimator.SetBool("PlayerTurn", false);
                BossAnimator.SetBool("BossTurn", true);
            }
            else
            {
                Debug.Log($"������ ������ ���� ������: {bossPhase}");
                HpBar.SetActive(false);
                Player.transform.position = new Vector3(0, -2.84f, 0);
                Player.SetActive(true);
                PlayerAnimator.SetBool("Hide", false);
                yield return StartCoroutine(HandleBossTurn());
                isPlayerTurn = true;
            }

            UpdateBossPhase();

            if (GameManager.Instance.BossHP <= 0)
            {
                HandleBossDeath();
                yield break;
            }
        }

        Debug.Log("���� ����!");
    }

    IEnumerator PlayerHide()
    {
        PlayerAnimator.SetBool("Hide", true);
        yield return new WaitForSeconds(0.4f);
        Player.SetActive(false);
    }

    void UpdateBossPhase()
    {
        // ���� ü�¿� ���� ������ ����
        float maxHP = GameManager.Instance.BossMaxHp; // GameManager���� ������ �ִ� ü��
        if (GameManager.Instance.BossHP <= maxHP * 0.3f)
            bossPhase = 3;
        else if (GameManager.Instance.BossHP <= maxHP * 0.6f)
            bossPhase = 2;
        else
            bossPhase = 1;

        Debug.Log($"������ ���� ������� {bossPhase}�Դϴ�.");
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
            case 3:
                yield return BossPhase3Pattern();
                break;
        }
    }

    IEnumerator BossPhase1Pattern()
    {
        Debug.Log("������ 1������ ������ �غ��մϴ�.");
        // 1������ ������ ���⿡ �߰��ϼ���
        yield return new WaitForSeconds(bossAttackTime);
    }

    IEnumerator BossPhase2Pattern()
    {
        Debug.Log("������ 2������ ������ �غ��մϴ�.");
        // 2������ ������ ���⿡ �߰��ϼ���
        yield return new WaitForSeconds(bossAttackTime);
    }

    IEnumerator BossPhase3Pattern()
    {
        Debug.Log("������ 3������ ������ �غ��մϴ�.");
        // 3������ ������ ���⿡ �߰��ϼ���
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

    void HandleBossDeath()
    {
        Debug.Log("������ �׾����ϴ�!");
        // ���� ���� ó�� ���� (��: �ִϸ��̼�, ���� ���� ��)
        Destroy(gameObject);
    }
}
