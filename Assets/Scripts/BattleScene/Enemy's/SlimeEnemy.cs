using System.Collections;
using UnityEngine;

public class SlimeEnemy : BasicUnitScript
{
    private Vector2 speedVectorWithPattern = new Vector2(0, 0); //이동이 필요한 패턴 사용 시 사용할 속도 벡터

    private bool isPhysicalAttacking; //특정 패턴 사용 시 몸체 충돌 데미지 판정 판별 변수

    private int restLimitTurn;

    private const int maxRestLimitTurn = 3;

    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Hp_F -= 10;
        }
    }

    protected override void StartSetting()
    {
        nowState = NowState.Standingby;
        nowDefensivePosition = DefensePos.None;

        isWaiting = true;

        bsm.enemyCharacterPos = transform.position;
        bsm.Enemy = gameObject;

        Hp_F = MaxHp_F;
        Energy_F = MaxEnergy_F;

        restWaitTime = 1.85f;

        StartCoroutine(WaitUntilTheGameStarts());
    }

    IEnumerator WaitUntilTheGameStarts()
    {
        while (true)
        {
            if (bsm.nowGameState == NowGameState.Playing)
            {
                actionCoolTimeObj.SetActive(true);
                break;
            }
            yield return null;
        }
    }

    protected override void UISetting()
    {
        if (bsm.nowGameState == NowGameState.Playing && isWaiting)
        {
            actionCoolTimeObj.transform.position = transform.position + (Vector3)actionCoolTimeObjPlusPos;
            actionCoolTimeImage.fillAmount = nowActionCoolTime / maxActionCoolTime;
            nowActionCoolTime += Time.deltaTime;
            if (nowActionCoolTime >= maxActionCoolTime)
            {
                WaitingTimeEnd();
                ActionCoolTimeBarSetActive(false);
                RandBehaviorStart(); //랜덤 행동
            }
        }
    }

    public void RandBehaviorStart()
    {
        // StartCoroutine(Resting()); //- 휴식
        // StartCoroutine(GoToAttack(true)); //- 기본 근접 공격
        // StartCoroutine(GoToAttack(false)); //- 방어 불가 스킬 근접 공격
        // StartCoroutine(ShootBullet()); //- 원거리 총알 발사 공격
        // StartCoroutine(HowitzerAttack()); //- 3연 곡사포 공격
        // StartCoroutine(LaserAttack()); //- 레이저 발사 공격

        if (nowState == NowState.Standingby)
        {
            int behaviorProbability = Random.Range(1, 101);

            if (behaviorProbability <= 20)
            {
                if (Energy_F <= MaxEnergy_F / 3 && restLimitTurn >= maxRestLimitTurn)
                {
                    restLimitTurn = 0;
                    nowCoroutine = Resting();
                    StartCoroutine(nowCoroutine);
                }
                else
                {
                    behaviorProbability = Random.Range(1, 101);
                    if (behaviorProbability <= 20)
                    {
                        nowCoroutine = GoToAttack(false);
                        StartCoroutine(nowCoroutine);
                    }
                    else if (behaviorProbability <= 60)
                    {
                        nowCoroutine = HowitzerAttack();
                        StartCoroutine(nowCoroutine);
                    }
                    else if (behaviorProbability <= 100)
                    {
                        nowCoroutine = LaserAttack();
                        StartCoroutine(nowCoroutine);
                    }
                }
            }
            else if (behaviorProbability <= 55)
            {
                nowCoroutine = GoToAttack(true);
                StartCoroutine(nowCoroutine);
            }
            else if (behaviorProbability <= 80)
            {
                nowCoroutine = GoToAttack(false);
                StartCoroutine(nowCoroutine);
            }
            else if(behaviorProbability <= 100)
            {
                nowCoroutine = ShootBullet();
                StartCoroutine(nowCoroutine);
            }

            restLimitTurn++;
        }
    }

    IEnumerator GoToAttack(bool isBasicCloseAttack)
    {
        Vector3 Movetransform = new Vector3(Speed_F, 0, 0); //이동을 위해 더해줄 연산
        Vector3 Targettransform = new Vector3(0, transform.position.y); //목표 위치

        nowState = NowState.Attacking;
        Targettransform.x = (isBasicCloseAttack) ? bsm.playerCharacterPos.x + 5.5f : bsm.playerCharacterPos.x + 8;
        Energy_F -= (isBasicCloseAttack) ? 2 : 3;

        while (transform.position.x > Targettransform.x) //이동중
        {
            transform.position -= Movetransform * Time.deltaTime;
            yield return null;
        }
        transform.position = Targettransform; //이동 완료

        if (isBasicCloseAttack)
        {
            nowCoroutine = Attacking(true, nowAttackCount_I, 1f); //기본공격 실행
            StartCoroutine(nowCoroutine);
        }
        else
        {
            nowCoroutine = DefenselessCloseAttack(); //내려찍기 공격 실행
            StartCoroutine(nowCoroutine);
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

        nowCoroutine = Return();
        StartCoroutine(nowCoroutine);
    }

    IEnumerator DefenselessCloseAttack() //내려찍기 공격
    {
        WaitForSeconds defenselessCloseAttackDelay = new WaitForSeconds(0.3f);

        yield return new WaitForSeconds(0.5f); //점프 전 대기 시간
        rigid.AddForce(Vector2.up * jumpPower_F, ForceMode2D.Impulse);
        rigid.gravityScale = setJumpGravityScale_F;

        speedVectorWithPattern.x = 5.5f; //점프하며 플레이어 위치에 다가갈 스피드

        while (transform.position.x >= bsm.playerCharacterPos.x) //플레이어 시작 x값까지 움직임
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

        nowCoroutine = Return();
        StartCoroutine(nowCoroutine);
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

        nowState = NowState.Attacking;
        Energy_F -= 2;

        yield return new WaitForSeconds(1.5f);

        var slimeBullet = ObjectPool.Instance.GetObject((int)PoolObjKind.SlimeEnemyBullet);

        spawnSlimeBulletPosition.x = transform.position.x;
        spawnSlimeBulletPosition.y = slimeBullet.transform.position.y;

        slimeBullet.transform.position = spawnSlimeBulletPosition;

        if (Energy_F > 0)
        {
            WaitingTimeStart();
        }
        else
        {
            nowState = NowState.Standingby;
        }
    }

    IEnumerator HowitzerAttack()
    {
        WaitForSeconds launchDelay = new WaitForSeconds(0.7f);

        nowState = NowState.Attacking;
        
        Energy_F -= 3;

        //발사 애니메이션 실행
        yield return new WaitForSeconds(3);
        //Idle 애니메이션 전환

        for (int nowLaunchCount = 0; nowLaunchCount < 3; nowLaunchCount++)
        {
            ObjectPool.Instance.GetObject((int)PoolObjKind.SlimeEnemyHowitzerBullet);
            yield return launchDelay;
        }

        if (Energy_F > 0)
        {
            WaitingTimeStart();
        }
        else
        {
            nowState = NowState.Standingby;
        }
    }

    IEnumerator LaserAttack()
    {
        WaitForSeconds launchDelay = new WaitForSeconds(1f);
        bool isLaunchUp = false;
        int randLaunch = Random.Range(0, 100);

        nowState = NowState.Attacking;
        
        Energy_F -= 5;

        if (randLaunch > 49)
        {
            isLaunchUp = true;
        }

        //발사 준비 애니메이션 실행
        yield return new WaitForSeconds(1.5f);
        //발사 애니메이션 실행

        for (int nowLaunchCount = 0; nowLaunchCount < 2; nowLaunchCount++)
        {
            GameObject nowLaunchLaserObj = ObjectPool.Instance.GetObject((int)PoolObjKind.SlimeEnemyLaser);
            EnemysLaser nowLaunchEnemyLaser = nowLaunchLaserObj.GetComponent<EnemysLaser>();

            nowLaunchEnemyLaser.launchAngle = (isLaunchUp) ? -80 : -70;
            nowLaunchEnemyLaser.onEnablePos.x = (isLaunchUp) ? -7.5f : -6.7f;
            nowLaunchEnemyLaser.onEnablePos.y = (isLaunchUp) ? 3 : 0;
            isLaunchUp = (isLaunchUp) ? false : true;

            yield return launchDelay;
        }

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

    protected override IEnumerator Resting()/////////////////////////////////////////////
    {
        int nowRestingCount = 0;
        WaitForSeconds RestWaitTime = new WaitForSeconds(restWaitTime);

        nowState = NowState.Resting;

        battleUIObjScript.BattleUIObjSetActiveTrue(ChangeBattleUIAnim.Rest);
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
        battleUIObjScript.BattleUIObjSetActiveFalse();

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
        nowState = NowState.Dead;
        bsm.NowGetBasicGood += 50;

        rigid.velocity = Vector2.zero;
        rigid.gravityScale = 0;

        battleUIAnimator.SetBool("NowFainting", false);
        battleUIObjScript.BattleUIObjSetActiveFalse();

        battleUIAnimator.SetBool("NowResting", false);
        battleUIObjScript.BattleUIObjSetActiveFalse();

        ActionCoolTimeBarSetActive(false);

        if (nowCoroutine != null)
        {
            StopCoroutine(nowCoroutine);
        }

        bsm.StartGameEndPanelAnim(false);

        //죽는 애니메이션 재생
        yield return null;
    }

    protected override IEnumerator Fainting()
    {
        while (true)
        {
            if (nowState == NowState.Standingby || nowState == NowState.Defensing)
            {
                break;
            }
            yield return null;
        }

        nowState = NowState.Fainting;

        battleUIObjScript.BattleUIObjSetActiveTrue(ChangeBattleUIAnim.Faint);
        battleUIAnimator.SetBool("NowFainting", true);

        yield return new WaitForSeconds(8);

        battleUIAnimator.SetBool("NowFainting", false);
        battleUIObjScript.BattleUIObjSetActiveFalse();

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
    //IEnumerator TestBulletFire() //360도로 총알 발사되는 것
    //{
    //    //mathf탄막 연습
    //    for (int i = 0; i < 360; i += 30)
    //    {
    //        GameObject bulletObj;
    //        Vector3 dir = new Vector2(Mathf.Cos(i * Mathf.Deg2Rad), Mathf.Sin(i * Mathf.Deg2Rad)); //* Mathf.Deg2Rad
    //        bulletObj = Instantiate(testBullet, BattleSceneManager.Instance.Enemy.transform.position, Quaternion.identity);
    //        bulletObj.GetComponent<EnemysBullet>().moveDirection = dir;//(dir - transform.position).normalized;
    //        yield return null;
    //    }
    //}
}
