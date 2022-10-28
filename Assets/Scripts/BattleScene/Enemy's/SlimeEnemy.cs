using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeEnemy : BasicUnitScript
{
    [SerializeField]
    [Tooltip("이동이 필요한 패턴 사용 시 사용할 속도 벡터")]
    private Vector2 speedVectorWithPattern = new Vector2(0, 0);

    [SerializeField]
    [Tooltip("특정 패턴 사용 시 몸체 충돌 데미지 판정 판별 변수")]
    private bool isPhysicalAttacking;

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

        restWaitTime = 1.85f;
    }

    protected override void UISetting()
    {
        if (isWaiting)
        {
            actionCoolTimeImage.fillAmount = nowActionCoolTime / maxActionCoolTime;
            nowActionCoolTime += Time.deltaTime;
            if (nowActionCoolTime >= maxActionCoolTime)
            {
                WaitingTimeEnd();
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
        // StartCoroutine(ShootBullet()); - 원거리 총알 발사 공격
        //
        //

        if (nowState == NowState.Standingby)
        {
            int behaviorProbability = Random.Range(0, 100);

            if (behaviorProbability <= 39)
            {
                if (Energy_F <= MaxEnergy_F / 3)
                {
                    StartCoroutine(Resting());
                }
                behaviorProbability = Random.Range(0, 100);
                if (behaviorProbability <= 29)
                {
                    StartCoroutine(GoToAttack(false));
                }
                else if (behaviorProbability <= 59)
                {
                    StartCoroutine(GoToAttack(true));
                }
                else
                {
                    StartCoroutine(ShootBullet());
                }
            }
            else if (behaviorProbability <= 69)
            {
                StartCoroutine(GoToAttack(true));
            }
            else if (behaviorProbability <= 89)
            {
                StartCoroutine(GoToAttack(false));
            }
            else
            {
                StartCoroutine(ShootBullet());
            }
        }
    }

    IEnumerator GoToAttack(bool isBasicCloseAttack)
    {
        Vector3 Movetransform = new Vector3(Speed_F, 0, 0); //이동을 위해 더해줄 연산
        Vector3 Targettransform = new Vector3(0, transform.position.y); //목표 위치
        var battleSceneManagerInstance = BattleSceneManager.Instance;

        nowState = NowState.Attacking;
        Targettransform.x = (isBasicCloseAttack) ? battleSceneManagerInstance.playerCharacterPos.x + 5.5f : battleSceneManagerInstance.playerCharacterPos.x + 8;
        Energy_F -= (isBasicCloseAttack) ? 2 : 3;

        while (transform.position.x > Targettransform.x) //이동중
        {
            transform.position -= Movetransform * Time.deltaTime;
            yield return null;
        }
        transform.position = Targettransform; //이동 완료

        if (isBasicCloseAttack)
        {
            StartCoroutine(Attacking(true, nowAttackCount_I, 1f)); //기본 공격 실행
        }
        else
        {
            StartCoroutine(DefenselessCloseAttack()); //내려찍기 공격 실행
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
                    nowRangeInEnemysComponent.Hit(Damage_I, isDefence);
                }
            }
        }

        if (isLastAttack == true)
        {
            yield return new WaitForSeconds(1f);
        }

        StartCoroutine(Return());
    }

    IEnumerator DefenselessCloseAttack() //내려찍기 공격
    {
        WaitForSeconds defenselessCloseAttackDelay = new WaitForSeconds(0.3f);

        yield return new WaitForSeconds(0.5f); //점프 전 대기 시간
        rigid.AddForce(Vector2.up * jumpPower_F, ForceMode2D.Impulse);
        rigid.gravityScale = setJumpGravityScale_F;

        speedVectorWithPattern.x = 5.5f; //점프하며 플레이어 위치에 다가갈 스피드

        while (transform.position.x >= BattleSceneManager.Instance.playerCharacterPos.x) //플레이어 시작 x값까지 움직임
        {
            transform.position -= (Vector3)speedVectorWithPattern * Time.deltaTime;
            yield return null;
        }

        rigid.gravityScale = -0.7f;

        speedVectorWithPattern.x = 0;

        yield return defenselessCloseAttackDelay; //내려찍기 준비시간

        isPhysicalAttacking = true;

        rigid.AddForce(Vector2.down * jumpPower_F * 3, ForceMode2D.Impulse);

        while (transform.position.y > startPos_Vector.y)
        {
            yield return null;
        }

        CamShake.CamShakeMod(true, 2f);
        transform.position = new Vector2(transform.position.x, startPos_Vector.y);
        rigid.velocity = Vector2.zero;
        rigid.gravityScale = 0;
        isPhysicalAttacking = false;

        yield return new WaitForSeconds(1.25f);

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

    IEnumerator ShootBullet() //원거리 발사 공격
    {
        Vector2 spawnSlimeBulletPosition;

        Energy_F -= 2;

        yield return new WaitForSeconds(1.5f);

        var slimeBullet = ObjectPool.Instance.GetObject((int)PoolObjKind.SlimeEnemyBullet);

        spawnSlimeBulletPosition.x = transform.position.x;
        spawnSlimeBulletPosition.y = slimeBullet.transform.position.y;

        slimeBullet.transform.position = spawnSlimeBulletPosition;

        yield return null;
        WaitingTimeStart();
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
        int nowRestingCount = 0;
        WaitForSeconds RestWaitTime = new WaitForSeconds(restWaitTime);

        nowState = NowState.Resting;

        battleUIObjScript.ChangeRestAnimObjScale();
        battleUIAnimator.SetBool("NowResting", true);

        while (2 > nowRestingCount)
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

        battleUIAnimator.SetBool("NowResting", false);
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

    protected override IEnumerator Fainting()
    {
        nowState = NowState.Fainting;

        battleUIObjScript.ChangeFaintAnimObjScale();
        battleUIAnimator.SetBool("NowFainting", true);

        yield return new WaitForSeconds(5);

        battleUIAnimator.SetBool("NowFainting", false);
        Energy_F = MaxEnergy_F; 
        WaitingTimeStart();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPhysicalAttacking && collision.gameObject.CompareTag("Player"))
        {
            CamShake.CamShakeMod(false, 2f);
            collision.gameObject.GetComponent<BasicUnitScript>().Hit(Damage_I + Mathf.Round(Damage_I / 2), false);
        }
    }
}
