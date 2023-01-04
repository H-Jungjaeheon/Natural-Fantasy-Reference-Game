using System.Collections;
using UnityEngine;
using System.IO;

public class SlimeEnemy : Enemy, IChangePhase
{
    [SerializeField]
    [Tooltip("스테이지 1 기믹 컴포넌트")]
    private WaterFallMachine gimmick;

    protected override string[] PattonText()
    {
        string[] path;
        int randIndex = Random.Range(1, 3);

        path = File.ReadAllText($"{Application.dataPath}/BossPattonTexts/SlimeBossPhase{(int)nowPhase}Patton{randIndex}.txt").Split(','); //보스 패턴 텍스트 가져오기 (현재 페이즈에 맞는 텍스트 파일 : 3가지 경우 중 랜덤)

        return path;
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

                if (nowActionCoolTime >= maxActionCoolTime || isChangePhase)
                {
                    WaitingTimeEnd();
                    ActionCoolTimeBarSetActive(false);

                    if (isChangePhase == false)
                    {
                        RandBehaviorStart(); //랜덤 행동
                    }
                    else
                    {
                        StartCoroutine(ChangePhase()); //페이즈 변경 애니메이션 코루틴
                    }

                    break;
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// 페이즈 변경하는 구간 
    /// </summary>
    /// <returns></returns>
    public IEnumerator ChangePhase()
    {
        nowState = NowState.ChangePhase;

        yield return new WaitForSeconds(6f); //페이즈 변경 애니메이션 종료까지 대기

        isChangePhase = false;

        Invincibility(false);

        nowCoroutine = ThornSkill();
        StartCoroutine(nowCoroutine);

        pattonText = PattonText();
    }

    /// <summary>
    /// 보스 랜덤 공격 뽑기
    /// </summary>
    protected override void RandBehaviorStart()
    {
        if (nowState == NowState.Standingby)
        {
            int randIndex = Random.Range(0, 2);
            int nowPattonIndex = 0;

            if (Energy < MaxEnergy * 0.5f && restLimitTurn >= maxRestLimitTurn && randIndex == 1)
            {
                nowCoroutine = Resting(); //휴식
                restLimitTurn = 0;
            }
            else
            {
                if (int.TryParse(pattonText[pattonCount], out nowPattonIndex))
                {
                    switch (nowPattonIndex)
                    {
                        case 0:
                            nowCoroutine = GoToAttack(true); //기본 근접 공격
                            break;
                        case 1:
                            nowCoroutine = GoToAttack(false); //내려찍기 공격
                            break;
                        case 2:
                            nowCoroutine = ShootBullet(); //총알 발사 공격
                            break;
                        case 3:
                            nowCoroutine = HowitzerAttack(); //3연속 곡사포 공격
                            break;
                        case 4:
                            nowCoroutine = LaserAttack(); //레이저 발사 공격
                            break;
                        case 5:
                            nowCoroutine = TrapSkill(); //디버프 함정 설치(2페이즈)
                            break;
                        case 6:
                            nowCoroutine = RainSkill(); //독 폭포 공격(2페이즈)
                            break;
                        case 7:
                            nowCoroutine = ThornSkill(); //주먹 변형 패턴(2페이즈)
                            break;
                    }
                    pattonCount++;
                }
                else
                {
                    RandBehaviorStart();
                }
            }

            StartCoroutine(nowCoroutine);
            restLimitTurn++;

            if (pattonCount == pattonText.Length)
            {
                pattonCount = 0;
                pattonText = PattonText();
            }
        }
    }

    IEnumerator GoToAttack(bool isBasicCloseAttack)
    {
        Vector3 Targettransform = new Vector3((isBasicCloseAttack) ? bsm.playerCharacterPos.x + 5.5f : bsm.playerCharacterPos.x + 8,
            transform.position.y); //목표 위치

        nowState = NowState.Attacking;

        Energy -= (isBasicCloseAttack) ? 2 : 3;

        animator.SetBool(moving, true);

        while (transform.position.x > Targettransform.x) //이동중
        {
            transform.position -= movetransform * Time.deltaTime;
            yield return null;
        }

        transform.position = Targettransform; //이동 완료

        animator.SetBool(moving, false);

        nowCoroutine = (isBasicCloseAttack) ? Attacking(true, nowAttackCount, 0.65f) : DefenselessCloseAttack(); //isBasicCloseAttack이 참이면, nowCoroutine에 현재 실행할 기본 근접공격 코루틴 저장(거짓이면, 내려찍기 공격 코루틴 저장)
        StartCoroutine(nowCoroutine);
    }


    public override void Hit(float damage, bool isDefending, EffectType effectType)
    {
        base.Hit(damage, isDefending, effectType);

        if (IsInvincibility == false && isDefending == false)
        {
            GameObject hitParticle = objectPoolInstance.GetObject((int)PoolObjKind.BossHitParticle);

            hitParticle.transform.position = transform.position + particlePos; //현재 파티클 스폰 위치(오브젝트 위치 + 설정한 유닛 고유 파티클 생성 위치) 
        }

        if (Hp <= maxHp * 0.5f && nowPhase == BossPhase.PhaseOne)
        {
            Invincibility(true);
            isChangePhase = true;
            nowPhase = BossPhase.PhaseTwo;
        }
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

        animator.SetTrigger(basicAttack);

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
                    
                    nowRangeInEnemysComponent.Hit(Damage, isDefence, EffectType.Shock);
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

        animator.SetBool(jumping, true);

        yield return new WaitForSeconds(0.5f); //점프 전 대기 시간

        rigid.AddForce(Vector2.up * jumpPower_F, ForceMode2D.Impulse);
        rigid.gravityScale = setJumpGravityScale_F;

        speedVector.x = 5.5f; //점프하며 플레이어 위치에 다가갈 스피드

        while (transform.position.x >= bsm.playerCharacterPos.x) //플레이어 시작 x값까지 움직임
        {
            transform.position -= (Vector3)speedVector * Time.deltaTime;
            yield return null;
        }

        isPhysicalAttacking = true;

        rigid.gravityScale = -0.7f;

        speedVector.x = 0f;

        animator.SetBool(jumping, false);
        animator.SetBool("SlappingDown", true);

        yield return defenselessCloseAttackDelay; //내려찍기 준비시간

        rigid.AddForce(Vector2.down * jumpPower_F * 3, ForceMode2D.Impulse);

        while (transform.position.y > startPos.y)
        {
            yield return null;
        }

        CamShake.CamShakeMod(true, 2f);
        transform.position = new Vector2(transform.position.x, startPos.y);
        rigid.velocity = Vector2.zero;
        rigid.gravityScale = 0;
        isPhysicalAttacking = false;
        animator.SetBool("SlappingDown", false);

        yield return new WaitForSeconds(1.25f);

        nowCoroutine = Return();
        StartCoroutine(nowCoroutine);
    }

    /// <summary>
    /// 근접공격 후 제자리로 귀환
    /// </summary>
    /// <returns></returns>
    IEnumerator Return()
    {
        transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        animator.SetBool(moving, true);

        while (transform.position.x < startPos.x)
        {
            transform.position += movetransform * Time.deltaTime;
            yield return null;
        }

        transform.rotation = Quaternion.identity;
        transform.position = startPos;
        nowAttackCount = 1;

        animator.SetBool(moving, false);

        WaitingTimeStart();
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

        WaitingTimeStart();
    }

    /// <summary>
    /// 곡사포 발사 공격
    /// </summary>
    /// <returns></returns>
    IEnumerator HowitzerAttack()
    {
        WaitForSeconds launchDelay = new WaitForSeconds(0.7f);

        nowState = NowState.Attacking;
        Energy -= 3;

        animator.SetBool("HowitzerAttack", true);
        yield return new WaitForSeconds(2f);
        animator.SetBool("HowitzerAttack", false);

        for (int nowLaunchCount = 0; nowLaunchCount < 3; nowLaunchCount++)
        {
            objectPoolInstance.GetObject((int)PoolObjKind.SlimeEnemyHowitzerBullet);
            yield return launchDelay;
        }

        WaitingTimeStart();
    }

    /// <summary>
    /// 레이저 발사 공격
    /// </summary>
    /// <returns></returns>
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
            Laser nowLaunchEnemyLaser = nowLaunchLaserObj.GetComponent<Laser>();

            nowLaunchEnemyLaser.launchAngle = (isLaunchUp) ? 10 : 20;
            nowLaunchEnemyLaser.onEnablePos.x = (isLaunchUp) ? -4.9f : -4.2f;
            nowLaunchEnemyLaser.onEnablePos.y = (isLaunchUp) ? 3.75f : 1.33f;

            isLaunchUp = (isLaunchUp) ? false : true;

            yield return launchDelay;
        }

        animator.SetBool("LazerAttack", false);
        animator.SetTrigger("LazerAttackEnd");

        yield return launchDelay;

        WaitingTimeStart();
    }

    /// <summary>
    /// 함정 설치
    /// </summary>
    /// <returns></returns>
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

        WaitingTimeStart();
    }

    /// <summary>
    /// 독 폭포
    /// </summary>
    /// <returns></returns>
    IEnumerator RainSkill()
    {
        nowState = NowState.Attacking;
        Energy -= 6;

        yield return new WaitForSeconds(1f); //점프 전 대기 시간

        for (int nowIndex = 0; nowIndex < 2; nowIndex++)
        {   
            rigid.gravityScale = setJumpGravityScale_F;
            rigid.AddForce(Vector2.up * 14, ForceMode2D.Impulse);

            if (nowIndex == 0)
            {
                yield return new WaitForSeconds(0.7f); //내려찍기 준비시간
            }
            else
            {
                yield return new WaitForSeconds(0.4f); //내려찍기 준비시간   
            }

            rigid.AddForce(Vector2.down * jumpPower_F * 6, ForceMode2D.Impulse);

            while (transform.position.y > startPos.y)
            {
                yield return null;
            }

            CamShake.CamShakeMod(true, 3f);
            transform.position = new Vector2(startPos.x, startPos.y);
            rigid.velocity = Vector2.zero;
        }

        rigid.gravityScale = 0;

        GameObject waterfallObj = objectPoolInstance.GetObject((int)PoolObjKind.SlimePoisonWaterfall);
        Laser nowWaterfallObj = waterfallObj.GetComponent<Laser>();

        nowWaterfallObj.onEnablePos.x = Random.Range(-10, 6);
        nowWaterfallObj.onEnablePos.y = 6.75f;

        WaitingTimeStart();
    }

    /// <summary>
    /// 어퍼컷(궁극기)
    /// </summary>
    /// <returns></returns>
    IEnumerator ThornSkill()
    {
        Vector2 rangePos = new Vector2(0f, -0.5f);
        Vector2 attackPos = new Vector2(0f, 5.1f);
        Vector2 nowOffset = new Vector2(0f, 0f);

        float nowOffsetY = -1.84f;
        int nowAttackPosX = -20;

        nowState = NowState.Attacking;
        Energy -= 15;
        Damage *= 2;

        SlowDebuff(false, 0); //버프 해제(슬로우)
        nowBurnDamageLimitTime = maxBurnDamageLimitTime; //버프 해제(화상)
        isImmunity = true;

        //바닥으로 들어가는 애니메이션 재생
        while (true)
        {
            if (nowOffsetY <= -4)
            {
                nowOffsetY = -4;
                nowOffset.y = nowOffsetY;
                ownCollider.offset = nowOffset;
                break;
            }

            ownCollider.offset = nowOffset;
            nowOffset.y = nowOffsetY;
            nowOffsetY -= Time.deltaTime * 2f;

            yield return null;
        }

        transform.position = attackPos;
        isPhysicalAttacking = true;

        for (int nowCount = 0; nowCount < 3; nowCount++)
        {
            nowAttackPosX += 10;

            rangePos.x = nowAttackPosX;
            attackPos.x = nowAttackPosX;

            transform.position = attackPos;

            var displayObj = objectPoolInstance.GetObject((int)PoolObjKind.RangeDisplay);
            displayObj.transform.position = rangePos;

            displayObj.GetComponent<RangeDisplayObj>().OnEnableSetting(new Vector2(6.8f, 5.5f), 1.5f);

            yield return new WaitForSeconds(1.5f);
            //공격 애니메이션 실행

            while (true)
            {
                if (nowOffsetY >= -1.84f)
                {
                    nowOffsetY = -1.84f;
                    nowOffset.y = nowOffsetY;
                    ownCollider.offset = nowOffset;
                    break;
                }

                ownCollider.offset = nowOffset;
                nowOffset.y = nowOffsetY;
                nowOffsetY += Time.deltaTime * 6;

                yield return null;
            }

            yield return new WaitForSeconds(0.5f);
            //내려가는 애니메이션 실행
            while (true)
            {
                if (nowOffsetY <= -4)
                {
                    nowOffsetY = -4;
                    nowOffset.y = nowOffsetY;
                    ownCollider.offset = nowOffset;
                    break;
                }

                ownCollider.offset = nowOffset;
                nowOffset.y = nowOffsetY;
                nowOffsetY -= Time.deltaTime * 4.5f;

                yield return null;
            }
        }

        isPhysicalAttacking = false;
        Damage /= 2;

        //올라오는 애니메이션 실행

        while (true)
        {
            if (nowOffsetY >= -1.84f)
            {
                nowOffsetY = -1.84f;
                nowOffset.y = nowOffsetY;
                ownCollider.offset = nowOffset;
                break;
            }

            ownCollider.offset = nowOffset;
            nowOffset.y = nowOffsetY;
            nowOffsetY += Time.deltaTime * 2f;

            yield return null;
        }

        isImmunity = false;
        nowBurnDamageLimitTime = 0;

        WaitingTimeStart();
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
        battleUIAnimator.SetBool(nowResting, true);
        animator.SetBool(resting, true);

        while (nowRestingCount < 2)
        {
            if (Energy >= MaxEnergy)
            {
                Energy = MaxEnergy;
                break;
            }
            yield return RestWaitTime;
            Energy += 1f;
            nowRestingCount += 1;
        }

        battleUIAnimator.SetBool(nowResting, false);
        animator.SetBool(resting, false);

        if (isSlowing)
        {
            battleUIAnimator.SetBool(nowSlowing, true);
        }
        else
        {
            battleUIObjScript.BattleUIObjSetActiveFalse();
        }

        nowActionCoolTime = maxActionCoolTime;
        WaitingTimeStart();
    }

    /// <summary>
    /// 죽을 때 함수
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator Dead()
    {
        if (nowCoroutine != null)
        {
            StopCoroutine(nowCoroutine);
        }

        nowState = NowState.Dead;
        bsm.NowGetBasicGood += 50;

        gimmick.StopFunction();

        animator.SetTrigger(dead);
        spriteRenderer.sortingOrder = 5;

        rigid.velocity = Vector2.zero;
        rigid.gravityScale = 0;

        battleUIObjScript.BattleUIObjSetActiveFalse();

        ActionCoolTimeBarSetActive(false);

        bsm.StartGameEndPanelAnim(false);

        yield return new WaitForSeconds(0.5f);

        deadEffectObj.SetActive(true);
    }

    /// <summary>
    /// 기절 함수
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator Fainting()
    {
        nowState = NowState.Fainting;

        battleUIObjScript.BattleUIObjSetActiveTrue(ChangeBattleUIAnim.Faint);
        battleUIAnimator.SetBool(nowFainting, true);
        animator.SetBool(fainting, true);

        yield return new WaitForSeconds(8f);

        animator.SetBool(fainting, false);
        battleUIAnimator.SetBool(nowFainting, false);

        if (isSlowing)
        {
            battleUIAnimator.SetBool(nowSlowing, true);
        }
        else
        {
            battleUIObjScript.BattleUIObjSetActiveFalse();
        }

        Energy = MaxEnergy; 
        WaitingTimeStart();
    }

    /// <summary>
    /// 몸으로 공격하는 패턴일 때 피격 판정
    /// </summary>
    /// <param name="collision"></param>
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPhysicalAttacking && collision.gameObject.CompareTag("Player"))
        {
            CamShake.CamShakeMod(false, 2f);
            collision.gameObject.GetComponent<BasicUnitScript>().Hit(Damage + Mathf.Round(Damage / 2), false, EffectType.Shock);
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
