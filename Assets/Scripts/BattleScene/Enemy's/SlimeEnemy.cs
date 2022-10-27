using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeEnemy : BasicUnitScript
{
    [SerializeField]
    [Tooltip("보스 패턴 사용 시 사용할 움직임 벡터")]
    private Vector2 motionVectorWithPattern = new Vector2(0, 0);

    protected override void StartSetting()
    {
        nowState = NowState.Standingby;
        nowDefensivePosition = DefensePos.None;

        isWaiting = true;
        actionCoolTimeObj.SetActive(true);
        
        BattleSceneManager.Instance.enemyCharacterPos = transform.position;
        BattleSceneManager.Instance.Enemy = gameObject;

        Hp_F = MaxHp_F;
        Energy_F = MaxEnergy_F;

        restWaitTime = 1.25f;
    }

    protected override void UISetting()
    {
        if (isWaiting)
        {
            actionCoolTimeImage.fillAmount = nowActionCoolTime / maxActionCoolTime;
            nowActionCoolTime += Time.deltaTime;
            if (nowActionCoolTime >= maxActionCoolTime)
            {
                isWaiting = false;
                nowActionCoolTime = 0;
                ActionCoolTimeBarSetActive(false);
                RandBehaviorStart(); //랜덤 행동
            }
        }
        nullActionCoolTimeImage.transform.position = Cam.WorldToScreenPoint(transform.position + new Vector3(0, actionCoolTimeImageYPos_F, 0));
    }

    public void RandBehaviorStart()
    {
        // StartCoroutine(Resting()); - 휴식
        // StartCoroutine(GoToAttack(true)); - 기본 근접 공격
        // StartCoroutine(GoToAttack(false)); - 방어 불가 스킬 근접 공격
        //
        //
        //

        if (nowState == NowState.Standingby)
        {
            int behaviorProbability = Random.Range(0, 100);
            
            if (Energy_F <= MaxEnergy_F / 2)
            {
                if (behaviorProbability <= 29)
                {
                    StartCoroutine(Resting());
                }
                else
                {
                    StartCoroutine(GoToAttack(false));
                }
            }
            else
            {
                nowState = NowState.Attacking;
                StartCoroutine(GoToAttack(false));
            }   
        }
    }

    IEnumerator GoToAttack(bool isBasicCloseAttack)
    {
        Vector3 Movetransform = new Vector3(Speed_F, 0, 0); //이동을 위해 더해줄 연산
        Vector3 Targettransform = new Vector3(BattleSceneManager.Instance.playerCharacterPos.x + 5.5f, transform.position.y); //목표 위치

        Energy_F -= 2;

        while (transform.position.x > Targettransform.x) //이동중
        {
            transform.position -= Movetransform * Time.deltaTime;
            yield return null;
        }
        transform.position = Targettransform; //이동 완료

        if (isBasicCloseAttack)
        {
            StartCoroutine(Attacking(true, nowAttackCount_I, 1f)); //첫번째 공격 실행
        }
        else
        {
            StartCoroutine(AnIndefensibleCloseAttack());
        }
    }

    public override void Hit(float damage, bool isDefending)
    {
        base.Hit(damage, isDefending);
    }

    IEnumerator Attacking(bool isLastAttack, int nowAttackCount_I, float delayTime)
    {
        float nowdelayTime = 0;
        //공격 애니메이션 실행
        while (nowdelayTime < delayTime) //공격 준비 동작
        {
            nowdelayTime += Time.deltaTime;
            yield return null;
        }

        if (rangeInEnemy.Count != 0) //기본공격 실행 함수 및 기본공격 애니메이션 시작
        {
            CamShake.CamShakeMod(false, 2f);
            for (int nowIndex = 0; nowIndex < rangeInEnemy.Count; nowIndex++)
            {
                if (rangeInEnemy[nowIndex] != null)
                {
                    var nowRangeInEnemysComponent = rangeInEnemy[nowIndex].GetComponent<BasicUnitScript>();
                    bool isDefence = (nowRangeInEnemysComponent.nowDefensivePosition == DefensePos.Right && nowRangeInEnemysComponent.nowState == NowState.Defensing) ? true : false;
                    rangeInEnemy[nowIndex].GetComponent<BasicUnitScript>().Hit(Damage_I, isDefence);
                }
            }
        }

        if (isLastAttack == true)
        {
            yield return new WaitForSeconds(1f);
        }

        StartCoroutine(Return());
    }

    IEnumerator AnIndefensibleCloseAttack()
    {
        rigid.AddForce(Vector2.up * jumpPower_F * 1.5f, ForceMode2D.Impulse);
        rigid.gravityScale = setJumpGravityScale_F;

        motionVectorWithPattern.x = 4f; //점프하며 플레이어 위치에 다가갈 스피드

        while (transform.position.x >= BattleSceneManager.Instance.playerCharacterPos.x)
        {
            transform.position -= (Vector3)motionVectorWithPattern * Time.deltaTime;
            yield return null;
        }

        motionVectorWithPattern.x = 0;
        rigid.gravityScale = setJumpGravityScale_F * 5;

        while (transform.position.y > startPos_Vector.y)
        {
            yield return null;
        }

        transform.position = new Vector2(transform.position.x, BattleSceneManager.Instance.enemyCharacterPos.y);
        rigid.velocity = Vector2.zero;
        rigid.gravityScale = 0;

        yield return new WaitForSeconds(1f);

        StartCoroutine(Return());
    }

    IEnumerator Return() //근접공격 후 돌아오기
    {
        Vector3 Movetransform = new Vector3(Speed_F, 0, 0);
        transform.rotation = Quaternion.Euler(0, 180, 0);
        while (transform.position.x < startPos_Vector.x)
        {
            transform.position += Movetransform * Time.deltaTime;
            yield return null;
        }

        transform.rotation = Quaternion.identity;
        transform.position = startPos_Vector;
        nowAttackCount_I = 1;

        if (Energy_F > 0)
        {
            WaitingTimeStart();
        }
        else
        {
            nowState = NowState.Standingby;
        }
    }
    private void WaitingTimeStart() //공격 후의 세팅 (일부 공통) 
    {
        nowState = NowState.Standingby;

        if (Hp_F > 0)
        {
            isWaiting = true;
            if (nowActionCoolTime < maxActionCoolTime)
            {
                ActionCoolTimeBarSetActive(true);
            }
        }
    }

    protected override IEnumerator Resting()
    {
        nowState = NowState.Resting;

        int nowRestingCount = 0;
        WaitForSeconds RestWaitTime = new WaitForSeconds(restWaitTime);
        while (3 > nowRestingCount)
        {
            if (Energy_F >= MaxEnergy_F)
            {
                Energy_F = MaxEnergy_F;
                break;
            }
            yield return RestWaitTime;
            Energy_F += 1;
            nowRestingCount += 1;
        }

        nowActionCoolTime = maxActionCoolTime;
        WaitingTimeStart();
    }

    protected override void Defense()
    {

    }
    protected override void SetDefensing(DefensePos nowDefensePos, float setRotation)
    {

    }

    protected override IEnumerator Dead()
    {
        yield return null;
    }

    protected override void Faint()
    {
        if (Energy_F <= 0 && (nowState == NowState.Standingby || nowState == NowState.Defensing))
        {
            StartCoroutine(Fainting());
        }
    }

    protected override IEnumerator Fainting()
    {
        nowState = NowState.Fainting;
        yield return new WaitForSeconds(5); 
        Energy_F = MaxEnergy_F; 
        WaitingTimeStart();
    }

    protected override IEnumerator PropertyPassiveAbilityStart()
    {

        yield return null;
    }
}
