using System.Collections;
using UnityEngine;
using System.IO;

public class TutorialEnemy : Enemy
{
    /// <summary>
    /// 보스 패턴 텍스트 가져오기 (현재 페이즈에 맞는 텍스트 파일 : 2가지 경우 중 랜덤)
    /// </summary>
    /// <returns></returns>
    protected override string[] PattonText()
    {
        string[] path;
        int randIndex = Random.Range(1, 3);

        path = File.ReadAllText($"{Application.dataPath}/BossPattonTexts/TutorialBoss/TutorialBossPhase{(int)nowPhase}Patton{randIndex}.txt").Split(',');

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

                    RandBehaviorStart();

                    break;
                }
            }
            yield return null;
        }
    }

    /// <summary>
    /// 보스 행동 실행 함수
    /// </summary>
    protected override void RandBehaviorStart()
    {
        if (nowState == NowState.Standingby)
        {
            int randIndex = Random.Range(0, 2);
            int nowPattonIndex;

            if (Energy < MaxEnergy * 0.5f && restLimitTurn >= maxRestLimitTurn && randIndex == 1)
            {
                nowCoroutine = Resting();
                restLimitTurn = 0;
            }
            else
            {
                if (int.TryParse(pattonText[pattonCount], out nowPattonIndex))
                {
                    switch (nowPattonIndex)
                    {
                        case 0:
                            nowCoroutine = GoToAttack(); //기본 근접 공격
                            break;
                        case 1:
                            nowCoroutine = GoToAttack(); //총알 발사 공격
                            break;
                        case 2:
                            nowCoroutine = GoToAttack(); //검기 발사 공격(궁극기)
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

    IEnumerator GoToAttack()
    {
        Vector2 Targettransform = new Vector2(bsm.playerCharacterPos.x + 5.5f, transform.position.y); //목표 위치

        nowState = NowState.Attacking;

        Energy -= 2;

        animator.SetBool(moving, true);

        while (transform.position.x > Targettransform.x) //이동중
        {
            transform.position -= movetransform * Time.deltaTime;
            yield return null;
        }

        transform.position = Targettransform; //이동 완료

        animator.SetBool(moving, false);

        nowCoroutine = Attacking(true, 1, 0.65f); //기본 근접공격 코루틴 저장 및 실행
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
    }

    /// <summary>
    /// 공격 함수
    /// </summary>
    /// <param name="isLastAttack"> 마지막 차례의 공격인지 판별 </param>
    /// <param name="nowAttackCount_I"> 현재 공격 회차 </param>
    /// <param name="delayTime"> 공격 준비 동작 시간 </param>
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
}
