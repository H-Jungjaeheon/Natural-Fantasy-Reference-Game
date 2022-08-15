using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : BasicUnitScript
{
    private void Awake()
    {
        StartSetting();
    }

    void Update()
    {
        UISetting();
    }

    protected override void StartSetting()
    {
        isWaiting = true;
        nowActionCoolTime = 0;
        Cam = Camera.main;
        rigid = gameObject.GetComponent<Rigidbody2D>();
        startPos_Vector = transform.position;
        nowAttackCount_I = 1;
        BattleSceneManager.Instance.PlayerCharacterPos = transform.position;
        actionCoolTimeObj.SetActive(true);
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
        Vector3 Targettransform = new Vector3(BattleSceneManager.Instance.PlayerCharacterPos.x + 5, transform.position.y); //목표 위치

        while (transform.position.x > Targettransform.x) //이동중
        {
            transform.position -= Movetransform * Time.deltaTime;
            yield return null;
        }
        transform.position = Targettransform; //이동 완료

        StartCoroutine(Attacking(true, nowAttackCount_I, 1f)); //첫번째 공격 실행
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
            CamShake.NowCamShakeStart(0.3f, 0.5f);
            for (int nowIndex = 0; nowIndex < rangeInEnemy.Count; nowIndex++)
            {
                if (rangeInEnemy[nowIndex] != null)
                {
                    switch (rangeInEnemy[nowIndex].GetComponent<BasicUnitScript>().nowDefensivePosition_B[(int)NowDefensePos.Right])
                    {
                        case true:
                            rangeInEnemy[nowIndex].GetComponent<BasicUnitScript>().Energy_F -= 1;
                            break;
                        case false:
                            rangeInEnemy[nowIndex].GetComponent<BasicUnitScript>().Hp_F -= Damage_I;
                            break;
                    }
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
        isAttacking = false;
        WaitingTimeStart();
    }
    private void WaitingTimeStart() //공격 후의 세팅 (일부 공통) 
    {
        isWaiting = true;
        ActionCoolTimeBarSetActive(true);
    }

    protected override void Defense()
    {
        
    }
    protected override void SetDefensing(int defensingDirectionIndex, float setRotation)
    {

    }
}
