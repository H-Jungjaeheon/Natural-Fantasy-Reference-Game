using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : BasicUnitScript
{
    protected override void StartSetting()
    {
        isWaiting = true;
        BattleSceneManager.Instance.EnemyCharacterPos = transform.position;
        BattleSceneManager.Instance.Enemy = gameObject;
        actionCoolTimeObj.SetActive(true);
        Energy_F = MaxEnergy_F;
        Hp_F = MaxHp_F;
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
                RandAttackStart();
            }
        }
        nullActionCoolTimeImage.transform.position = Cam.WorldToScreenPoint(transform.position + new Vector3(0, actionCoolTimeImageYPos_F, 0));
    }

    public void RandAttackStart()
    {
        StartCoroutine(GoToAttack());
    }

    IEnumerator GoToAttack()
    {
        Vector3 Movetransform = new Vector3(Speed_F, 0, 0); //이동을 위해 더해줄 연산
        Vector3 Targettransform = new Vector3(BattleSceneManager.Instance.PlayerCharacterPos.x + 5.5f, transform.position.y); //목표 위치

        while (transform.position.x > Targettransform.x) //이동중
        {
            transform.position -= Movetransform * Time.deltaTime;
            yield return null;
        }
        transform.position = Targettransform; //이동 완료

        StartCoroutine(Attacking(true, nowAttackCount_I, 1f)); //첫번째 공격 실행
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
                    bool isDefence = rangeInEnemy[nowIndex].GetComponent<BasicUnitScript>().nowDefensivePosition == DefensePos.Right ? true : false;
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
        WaitingTimeStart();
    }
    private void WaitingTimeStart() //공격 후의 세팅 (일부 공통) 
    {
        nowState = NowState.Standingby;
        isWaiting = true;
        if (nowActionCoolTime < maxActionCoolTime)
        {
            ActionCoolTimeBarSetActive(true);
        }
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
        //기절 애니 및 이벤트
        yield return null;
    }
    protected override void Faint()
    {
        if (Energy_F <= 0 && (nowState == NowState.Standingby || nowState == NowState.Defensing))
        {
            StartCoroutine(Fainting());
        }
    }

    protected override IEnumerator PropertyPassiveAbilityStart()
    {

        yield return null;
    }
}
