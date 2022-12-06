using System.Collections;
using UnityEngine;

public class SlimeEnemy : BasicUnitScript
{
    private Vector2 speedVectorWithPattern = new Vector2(0, 0); //이동이 필요한 패턴 사용 시 사용할 속도 벡터

    private Vector2 spawnPos = new Vector2(0, 0); //패턴에 소환하는 오브젝트들 초기 위치 설정 벡터

    private bool isPhysicalAttacking; //특정 패턴 사용 시 몸체 충돌 데미지 판정 판별 변수

    private int restLimitTurn;

    private const int maxRestLimitTurn = 3;

    /// <summary>
    /// 게임 처음 세팅
    /// </summary>
    protected override void StartSetting()
    {
        nowState = NowState.Standingby;
        nowDefensivePosition = DefensePos.None;

        isWaiting = true;

        bsm.enemyCharacterPos = transform.position;
        bsm.enemy = gameObject;

        Energy = MaxEnergy;
        Hp = MaxHp;

        restWaitTime = 1.85f;

        StartCoroutine(WaitUntilTheGameStarts());
    }

    /// <summary>
    /// 오프닝 시간동안 대기
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitUntilTheGameStarts()
    {
        while (true)
        {
            if (bsm.nowGameState == NowGameState.Playing)
            {
                actionCoolTimeObj.SetActive(true);
                WaitingTimeStart();
                break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 공격 쿨타임
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator UISetting()
    {
        while (true)
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
                    break;
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// 보스 랜덤 공격 뽑기
    /// </summary>
    public void RandBehaviorStart()
    {
        // StartCoroutine(Resting()); //- 휴식
        // StartCoroutine(GoToAttack(true)); //- 기본 근접 공격
        // StartCoroutine(GoToAttack(false)); //- 방어 불가 스킬 근접 공격
        // StartCoroutine(ShootBullet()); //- 원거리 총알 발사 공격
        // StartCoroutine(HowitzerAttack()); //- 3연 곡사포 공격
        // StartCoroutine(LaserAttack()); //- 레이저 발사 공격
        // StartCoroutine(TrapSkill()); //트랩 설치(2페이즈)

        if (nowState == NowState.Standingby)
        {
            int behaviorProbability = Random.Range(1, 101);

            if (behaviorProbability <= 20)
            {
                if (Energy <= MaxEnergy / 3 && restLimitTurn >= maxRestLimitTurn)
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
            else if (behaviorProbability <= 100)
            {
                nowCoroutine = ShootBullet();
                StartCoroutine(nowCoroutine);
            }

            restLimitTurn++;
        }
    }

    IEnumerator GoToAttack(bool isBasicCloseAttack)
    {
        Vector3 Targettransform = new Vector3(0, transform.position.y); //목표 위치

        nowState = NowState.Attacking;
        Targettransform.x = (isBasicCloseAttack) ? bsm.playerCharacterPos.x + 5.5f : bsm.playerCharacterPos.x + 8;
        Energy -= (isBasicCloseAttack) ? 2 : 3;

        animator.SetBool("Moving", true);

        while (transform.position.x > Targettransform.x) //이동중
        {
            transform.position -= movetransform * Time.deltaTime;
            yield return null;
        }
        transform.position = Targettransform; //이동 완료

        animator.SetBool("Moving", false);

        if (isBasicCloseAttack)
        {
            nowCoroutine = Attacking(true, nowAttackCount_I, 0.65f); //기본공격 실행
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

    /// <summary>
    /// 공격 함수
    /// </summary>
    /// <param name="isLastAttack"> 마지막 차례의 공격인지 판별 </param>
    /// <param name="nowAttackCount_I"></param>
    /// <param name="delayTime"></param>
    /// <returns></returns>
    IEnumerator Attacking(bool isLastAttack, int nowAttackCount_I, float delayTime)
    {
        float nowdelayTime = 0;

        animator.SetTrigger("BasicAttack");

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
                    nowRangeInEnemysComponent.Hit(Damage, isDefence);
                }
            }
        }

        if (isLastAttack == true)
        {
            yield return new WaitForSeconds(0.45f);
        }

        nowCoroutine = Return();
        StartCoroutine(nowCoroutine);
    }

    /// <summary>
    /// 내려찍기 공격
    /// </summary>
    /// <returns></returns>
    IEnumerator DefenselessCloseAttack()
    {
        WaitForSeconds defenselessCloseAttackDelay = new WaitForSeconds(0.3f);

        animator.SetBool("Jumping", true);

        yield return new WaitForSeconds(0.5f); //점프 전 대기 시간

        rigid.AddForce(Vector2.up * jumpPower_F, ForceMode2D.Impulse);
        rigid.gravityScale = setJumpGravityScale_F;

        speedVectorWithPattern.x = 5.5f; //점프하며 플레이어 위치에 다가갈 스피드

        while (transform.position.x >= bsm.playerCharacterPos.x) //플레이어 시작 x값까지 움직임
        {
            transform.position -= (Vector3)speedVectorWithPattern * Time.deltaTime;
            yield return null;
        }

        isPhysicalAttacking = true;

        rigid.gravityScale = -0.7f;

        speedVectorWithPattern.x = 0;

        animator.SetBool("Jumping", false);
        animator.SetBool("SlappingDown", true);

        yield return defenselessCloseAttackDelay; //내려찍기 준비시간

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
        animator.SetBool("SlappingDown", false);

        yield return new WaitForSeconds(1.25f);

        nowCoroutine = Return();
        StartCoroutine(nowCoroutine);
    }

    /// <summary>
    /// 근접공격 후 제자리로 돌아오는 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator Return()
    {
        transform.rotation = Quaternion.Euler(0, 180, 0);

        animator.SetBool("Moving", true);

        while (transform.position.x < startPos_Vector.x)
        {
            transform.position += movetransform * Time.deltaTime;
            yield return null;
        }

        transform.rotation = Quaternion.identity;
        transform.position = startPos_Vector;
        nowAttackCount_I = 1;

        animator.SetBool("Moving", false);

        if (Energy > 0)
        {
            WaitingTimeStart();
        }
        else
        {
            nowState = NowState.Standingby;
        }
    }

    /// <summary>
    /// 원거리 탄환 발사 공격
    /// </summary>
    /// <returns></returns>
    IEnumerator ShootBullet()
    {
        Vector2 spawnSlimeBulletPosition;

        nowState = NowState.Attacking;
        Energy -= 2;

        animator.SetBool("FrontShoot", true);

        yield return new WaitForSeconds(0.7f);

        var slimeBullet = objectPoolInstance.GetObject((int)PoolObjKind.SlimeEnemyBullet);

        spawnSlimeBulletPosition.x = 6;
        spawnSlimeBulletPosition.y = -1.65f;

        slimeBullet.transform.position = spawnSlimeBulletPosition;

        animator.SetBool("FrontShoot", false);

        if (Energy > 0)
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
        Energy -= 3;

        animator.SetBool("HowitzerAttack", true);
        yield return new WaitForSeconds(2);
        animator.SetBool("HowitzerAttack", false);

        for (int nowLaunchCount = 0; nowLaunchCount < 3; nowLaunchCount++)
        {
            objectPoolInstance.GetObject((int)PoolObjKind.SlimeEnemyHowitzerBullet);
            yield return launchDelay;
        }

        if (Energy > 0)
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
        int randLaunch = Random.Range(1, 101);

        nowState = NowState.Attacking;
        Energy -= 5;

        if (randLaunch > 49)
        {
            isLaunchUp = true;
        }

        animator.SetBool("LazerAttack", true);

        yield return launchDelay;

        for (int nowLaunchCount = 0; nowLaunchCount < 2; nowLaunchCount++)
        {
            GameObject nowLaunchLaserObj = objectPoolInstance.GetObject((int)PoolObjKind.SlimeEnemyLaser);
            EnemysLaser nowLaunchEnemyLaser = nowLaunchLaserObj.GetComponent<EnemysLaser>();

            nowLaunchEnemyLaser.launchAngle = (isLaunchUp) ? 10 : 20;
            nowLaunchEnemyLaser.onEnablePos.x = (isLaunchUp) ? -4.9f : -4.2f;
            nowLaunchEnemyLaser.onEnablePos.y = (isLaunchUp) ? 3.75f : 1.33f;

            isLaunchUp = (isLaunchUp) ? false : true;

            yield return launchDelay;
        }

        animator.SetBool("LazerAttack", false);
        animator.SetTrigger("LazerAttackEnd");

        yield return launchDelay;

        if (Energy > 0)
        {
            WaitingTimeStart();
        }
        else
        {
            nowState = NowState.Standingby;
        }
    }

    IEnumerator TrapSkill()
    {
        WaitForSeconds shootDelay = new WaitForSeconds(0.65f);

        nowState = NowState.Attacking;
        Energy -= 3;

        //애니메이션 실행
        yield return new WaitForSeconds(1.5f);

        for (int nowIndex = 0; nowIndex < 2; nowIndex++)
        {
            var trapObj = objectPoolInstance.GetObject((int)PoolObjKind.SlimeEnemyTrap);

            spawnPos.x = 8.5f;
            spawnPos.y = 1.2f;

            trapObj.transform.position = spawnPos;
            yield return shootDelay;
        }

        if (Energy > 0)
        {
            WaitingTimeStart();
        }
        else
        {
            nowState = NowState.Standingby;
        }
    }

    IEnumerator RainSkill()
    {

        yield return null;
    }

    IEnumerator ThornSkill()
    {

        yield return null;
    }

    /// <summary>
    /// 공격 후의 세팅
    /// </summary>
    private void WaitingTimeStart()
    {
        nowState = NowState.Standingby;

        if (Hp > 0)
        {
            isWaiting = true;
            
            if (nowActionCoolTime < maxActionCoolTime)
            {
                ActionCoolTimeBarSetActive(true);
            }

            StartCoroutine(UISetting());
        }
    }

    /// <summary>
    /// 휴식 함수
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator Resting()
    {
        int nowRestingCount = 0; //현재 쉰 횟수

        WaitForSeconds RestWaitTime = new WaitForSeconds(restWaitTime);

        nowState = NowState.Resting;

        battleUIObjScript.BattleUIObjSetActiveTrue(ChangeBattleUIAnim.Rest);
        battleUIAnimator.SetBool("NowResting", true);
        animator.SetBool("Resting", true);

        while (nowRestingCount < 2)
        {
            if (Energy >= MaxEnergy)
            {
                Energy = MaxEnergy;
                break;
            }
            yield return RestWaitTime;
            Energy += 1;
            nowRestingCount += 1;
        }

        battleUIAnimator.SetBool("NowResting", false);
        animator.SetBool("Resting", false);
        battleUIObjScript.BattleUIObjSetActiveFalse();

        nowActionCoolTime = maxActionCoolTime;
        WaitingTimeStart();
    }

    /// <summary>
    /// 죽을 때 함수
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator Dead()
    {
        nowState = NowState.Dead;
        bsm.NowGetBasicGood += 50;

        animator.SetTrigger("Dead");
        spriteRenderer.sortingOrder = 5;

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

    /// <summary>
    /// 기절 함수
    /// </summary>
    /// <returns></returns>
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

        Energy = MaxEnergy; 
        WaitingTimeStart();
    }

    /// <summary>
    /// 몸으로 공격하는 패턴일 때 피격 판정
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPhysicalAttacking && collision.gameObject.CompareTag("Player"))
        {
            CamShake.CamShakeMod(false, 2f);
            collision.gameObject.GetComponent<BasicUnitScript>().Hit(Damage + Mathf.Round(Damage / 2), false);
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
