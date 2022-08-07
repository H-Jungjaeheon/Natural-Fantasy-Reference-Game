using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : BasicUnitScript
{
    [Header("�ൿ ��ư ���� ����")]
    [Tooltip("�ൿ ��ư�� ������Ʈ")]
    [SerializeField]
    protected GameObject actionButtonsObj;

    private void Awake()
    {
        StartSetting();
    }

    // Update is called once per frame
    void Update()
    {
        UISetting();
        Jump();
    }

    protected override void StartSetting() //�ʱ� ���� (�Ϻ� ����)
    {
        Cam = Camera.main;
        rigid = gameObject.GetComponent<Rigidbody2D>();
        startPos_Vector = transform.position;
        maxActionCoolTime -= GameManager.Instance.ReduceCoolTimeLevel * 0.5f;
        MaxHp_F += GameManager.Instance.MaxHpUpgradeLevel * 5;
        Hp_F = MaxHp_F;
        MaxEnergy_F += GameManager.Instance.MaxEnergyUpgradeLevel * 5;
        Energy_F = MaxEnergy_F;
        Damage_I += GameManager.Instance.DamageUpgradeLevel;
        nowAttackCount_I = 1;
        BattleSceneManager.Instance.PlayerCharacterPos = transform.position;
    }

    protected override void UISetting() //���ð� �� UI���� (�Ϻ� ����)
    {
        if (isWaiting)
        {
            nullActionCoolTimeImage.transform.position = Cam.WorldToScreenPoint(transform.position + new Vector3(0, actionCoolTimeImageYPos_F, 0));
            actionCoolTimeImage.fillAmount = nowActionCoolTime / maxActionCoolTime;
            nowActionCoolTime += Time.deltaTime;
            if (nowActionCoolTime >= maxActionCoolTime)
            {
                isWaiting = false;
                actionCoolTimeObj.SetActive(false);
            }
        }
        if (isWaiting == false && isJumping == false && isAttacking == false)
        {
            ActionButtonsSetActive(true);
        }
        else
        {
            ActionButtonsSetActive(false);
        }
    }
    private void WaitingTimeStart() //���� ���� ���� (�Ϻ� ����) 
    {
        nowActionCoolTime = 0;
        isWaiting = true;
        actionCoolTimeObj.SetActive(true);
        ActionButtonsSetActive(false);
    }
    private void Jump() 
    {
        if (isJumping == false && isAttacking == false && Input.GetKey(KeyCode.Space))
        {
            isJumping = true;
            ActionButtonsSetActive(false);
            rigid.AddForce(Vector2.up * jumpPower_F, ForceMode2D.Impulse);
            rigid.gravityScale = setJumpGravityScale_F - 0.5f;
            StartCoroutine(JumpDelay());
        }
        else if (isJumping && transform.position.y < startPos_Vector.y)
        {
            isJumping = false;
            transform.position = startPos_Vector;
            rigid.velocity = Vector2.zero;
            rigid.gravityScale = 0;
        }
    }
    IEnumerator JumpDelay()
    {
        yield return new WaitForSeconds(0.3f);
        while (rigid.gravityScale >= 0.2f)
        {
            rigid.gravityScale -= Time.deltaTime * 3f;
            yield return null;
        }
        rigid.gravityScale = setJumpGravityScale_F * 1.5f;
    }

    public void CloseAttackStart()
    {
        isAttacking = true;
        ActionButtonsSetActive(false);
        StartCoroutine(GoToAttack());
    }

    IEnumerator GoToAttack()
    {
        Vector3 Movetransform = new Vector3(Speed_F * Time.deltaTime, 0, 0); //�̵��� ���� ������ ����
        Vector3 Targettransform = new Vector3(BattleSceneManager.Instance.EnemyCharacterPos.x - 5, transform.position.y); //��ǥ ��ġ

        while (transform.position.x < Targettransform.x) //�̵���
        {
            transform.position += Movetransform;
            yield return null;
        }
        transform.position = Targettransform; //�̵� �Ϸ�

        StartCoroutine(EnemyAttack(false, nowAttackCount_I, 0.3f)); //ù��° ����
    }

    IEnumerator EnemyAttack(bool isLastAttack, int nowAttackCount_I, float attacktimelimit_f) //3���� ��ͷ� ����
    {
        isComplete = false;
        var camComponent = Cam.GetComponent<CamShake>();
        if (rangeInEnemy[0] != null)
        {
            print($"���� {nowAttackCount_I}"); //���� ����(�ִϸ��̼�)
            switch (nowAttackCount_I)
            {
                case 1 :
                    camComponent.CamShakeStart(0.3f, 0.5f);
                    break;
                case 3:
                    camComponent.CamShakeStart(0.3f, 1);
                    break;
            }
            for (int nowIndex = 0; nowIndex < rangeInEnemy.Count; nowIndex++)
            {
                if (rangeInEnemy[nowIndex] != null)
                {
                    rangeInEnemy[nowIndex].GetComponent<BasicUnitScript>().Hp_F -= Damage_I;
                }
            }
        }

        if (isLastAttack == false)
        {
            float nowattacktime_f = 0;
            print("����");
            while (attacktimelimit_f > nowattacktime_f) //�⺻���� Ÿ�̹� ���
            {
                nowattacktime_f += Time.deltaTime;
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    isComplete = true;
                }
                yield return null;
            }

            if (isComplete)
            {
                switch (nowAttackCount_I)
                {
                    case 1:
                        yield return new WaitForSeconds(0.1f);
                        nowAttackCount_I++;
                        StartCoroutine(EnemyAttack(false, nowAttackCount_I, 0.3f)); //�ι�° ����
                        break;
                    case 2:
                        yield return new WaitForSeconds(0.5f);
                        nowAttackCount_I++;
                        StartCoroutine(EnemyAttack(true, nowAttackCount_I, 0.3f)); //����° ����
                        break;
                }
            }
            else //���ư���
            {
                StartCoroutine(Return());
            }
        }
        else //���ư���
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(Return());
        }
    }

    IEnumerator Return()
    {
        Vector3 Movetransform = new Vector3(Speed_F * Time.deltaTime, 0, 0); //�̵��� ���� ������ ����
        transform.rotation = Quaternion.Euler(0, 180, 0);
        while (transform.position.x > startPos_Vector.x)
        {
            transform.position -= Movetransform;
            yield return null;
        }
        transform.rotation = Quaternion.identity;
        transform.position = startPos_Vector;
        nowAttackCount_I = 1;
        isAttacking = false;
        WaitingTimeStart();
    }

    private void ActionButtonsSetActive(bool SetActive) => actionButtonsObj.SetActive(SetActive);
}
