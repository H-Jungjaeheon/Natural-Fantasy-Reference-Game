using System.Collections;
using UnityEngine;
using System.IO;
using System.Text;

public class TutorialEnemy : Enemy
{
    protected override void StartSetting()
    {
        base.StartSetting();

        plusVector = new Vector3(0.3f, 0f, 0f);
    }

    #region 튜토리얼 보스 스킬 애니메이션 이름 모음
    protected const string FIRING_A_GUN = "FiringAGun";

    protected const string FIRING_SWORD_AURA = "FiringSwordAura";
    #endregion

    /// <summary>
    /// 보스 패턴 텍스트 가져오기 (현재 페이즈에 맞는 텍스트 파일 : 2가지 경우 중 랜덤)
    /// </summary>
    /// <returns></returns>
    protected override void PattonText()
    {
        int randIndex = Random.Range(1, 3);

        sb = new StringBuilder($"{Application.dataPath}/BossPattonTexts/TutorialBoss/TutorialBossPhase", 100);
        sb.Append($"{(int)nowPhase}Patton{randIndex}.txt");

        pattonText = File.ReadAllText(sb.ToString()).Split(',');
    }

    protected override IEnumerator CoolTimeRunning()
    {
        while (true)
        {
            if (bm.nowGameState == NowGameState.Playing && isWaiting)
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
                            nowCoroutine = ShootBullet(); //총알 발사 공격
                            break;
                        case 2:
                            nowCoroutine = ShootSwordAura(); //검기 발사 공격(궁극기)
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
                PattonText();
            }
        }
    }

    IEnumerator GoToAttack()
    {
        Vector2 Targettransform = new Vector2(bm.playerCharacterPos.x + 3f, transform.position.y); //목표 위치

        nowState = NowState.Attacking;

        Energy -= 2;

        animator.SetBool(MOVING, true);

        while (transform.position.x > Targettransform.x) //이동중
        {
            transform.position -= movetransform * Time.deltaTime;
            yield return null;
        }

        transform.position = Targettransform; //이동 완료

        animator.SetBool(MOVING, false);

        nowCoroutine = Attacking(true, 1, 0.65f); //기본 근접공격 코루틴 저장 및 실행
        StartCoroutine(nowCoroutine);
    }


    public override void Hit(float damage, bool isDefending, EffectType effectType)
    {
        base.Hit(damage, isDefending, effectType);

        //if (IsInvincibility == false && isDefending == false) //타격 파티클 or 애니메이션 실행
        //{
        //}
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

        animator.SetTrigger(BASIC_ATTACK);

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
                    bool isDefence = (nowRangeInEnemysComponent.nowDefensivePos == DefensePos.Right && nowRangeInEnemysComponent.nowState == NowState.Defensing) ? true : false;

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

        animator.SetBool(MOVING, true);

        while (transform.position.x < startPos.x)
        {
            transform.position += movetransform * Time.deltaTime;
            yield return null;
        }

        transform.rotation = Quaternion.identity;
        transform.position = startPos;

        animator.SetBool(MOVING, false);

        WaitingTimeStart();
    }

    /// <summary>
    /// 원거리 탄환 발사 공격
    /// </summary>
    /// <returns></returns>
    IEnumerator ShootBullet()
    {
        Vector2 bulletPos;

        nowState = NowState.Attacking;
        Energy -= 2;

        animator.SetBool(FIRING_A_GUN, true);

        yield return new WaitForSeconds(0.7f);

        var bullet = op.GetObject((int)PoolObjKind.TutorialEnemyBullet);

        bulletPos.x = 7.5f;
        bulletPos.y = 0f;

        bullet.transform.position = bulletPos;

        animator.SetBool(FIRING_A_GUN, false);

        WaitingTimeStart();
    }

    /// <summary>
    /// 검기 발사 공격(궁극기)
    /// </summary>
    /// <returns></returns>
    IEnumerator ShootSwordAura()
    {
        Vector2 bulletPos;

        nowState = NowState.Attacking;
        Energy -= 2;

        animator.SetBool(FIRING_SWORD_AURA, true);

        yield return new WaitForSeconds(1.5f);

        var swordAura = op.GetObject((int)PoolObjKind.TutorialEnemySwordAura);

        bulletPos.x = 7.8f;
        bulletPos.y = -0.75f;

        swordAura.transform.position = bulletPos;

        animator.SetBool(FIRING_SWORD_AURA, false);

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
        battleUIAnimator.SetBool(NOW_RESTING, true);
        animator.SetBool(RESTING, true);

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

        battleUIAnimator.SetBool(NOW_RESTING, false);
        animator.SetBool(RESTING, false);

        if (isSlowing)
        {
            battleUIAnimator.SetBool(NOW_SLOWING, true);
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

        animator.SetTrigger(DEAD);
        spriteRenderer.sortingOrder = 5;

        rigid.velocity = Vector2.zero;
        rigid.gravityScale = 0;

        battleUIObjScript.BattleUIObjSetActiveFalse();

        ActionCoolTimeBarSetActive(false);

        bm.StartGameEndPanelAnim(false, new Vector3(0f, 0f, -10f));

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
        battleUIAnimator.SetBool(NOW_FAINTING, true);
        animator.SetBool(FAINTING, true);

        yield return new WaitForSeconds(8f);

        animator.SetBool(FAINTING, false);
        battleUIAnimator.SetBool(NOW_FAINTING, false);

        if (isSlowing)
        {
            battleUIAnimator.SetBool(NOW_SLOWING, true);
        }
        else
        {
            battleUIObjScript.BattleUIObjSetActiveFalse();
        }

        Energy = MaxEnergy;
        WaitingTimeStart();
    }
}
