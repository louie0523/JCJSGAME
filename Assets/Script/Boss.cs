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
                Debug.Log("플레이어의 공격턴");
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
                Debug.Log($"보스의 공격턴 현재 페이즈: {bossPhase}");
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

        Debug.Log("전투 종료!");
    }

    IEnumerator PlayerHide()
    {
        PlayerAnimator.SetBool("Hide", true);
        yield return new WaitForSeconds(0.4f);
        Player.SetActive(false);
    }

    void UpdateBossPhase()
    {
        // 보스 체력에 따라 페이즈 변경
        float maxHP = GameManager.Instance.BossMaxHp; // GameManager에서 가져온 최대 체력
        if (GameManager.Instance.BossHP <= maxHP * 0.3f)
            bossPhase = 3;
        else if (GameManager.Instance.BossHP <= maxHP * 0.6f)
            bossPhase = 2;
        else
            bossPhase = 1;

        Debug.Log($"보스의 현재 페이즈는 {bossPhase}입니다.");
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
        Debug.Log("보스가 1페이즈 패턴을 준비합니다.");
        // 1페이즈 로직을 여기에 추가하세요
        yield return new WaitForSeconds(bossAttackTime);
    }

    IEnumerator BossPhase2Pattern()
    {
        Debug.Log("보스가 2페이즈 패턴을 준비합니다.");
        // 2페이즈 로직을 여기에 추가하세요
        yield return new WaitForSeconds(bossAttackTime);
    }

    IEnumerator BossPhase3Pattern()
    {
        Debug.Log("보스가 3페이즈 패턴을 준비합니다.");
        // 3페이즈 로직을 여기에 추가하세요
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

    void HandleBossDeath()
    {
        Debug.Log("보스가 죽었습니다!");
        // 보스 죽음 처리 로직 (예: 애니메이션, 보상 지급 등)
        Destroy(gameObject);
    }
}
