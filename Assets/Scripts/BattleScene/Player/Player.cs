using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : BasicUnitScript
{
    [Header("공격에 필요한 오브젝트 모음")]
    [SerializeField]
    private GameObject swordAuraObj;

    BattleButtonManager BBM;

    protected override void Update()
    {
        base.Update();
        Deflect();
        Defense();
        Jump();
    }
  
    protected override void StartSetting() //초기 세팅 (일부 공통)
    {
        BBM = BattleButtonManager.Instance;
        maxActionCoolTime -= GameManager.Instance.ReduceCoolTimeLevel * 0.5f;
        MaxHp_F += GameManager.Instance.MaxHpUpgradeLevel * 5;
        MaxEnergy_F += GameManager.Instance.MaxEnergyUpgradeLevel * 5;
        Damage_I += GameManager.Instance.DamageUpgradeLevel;
        BattleSceneManager.Instance.PlayerCharacterPos = transform.position;
        Energy_F = MaxEnergy_F;
        Hp_F = MaxHp_F;
    }

    protected override void Defense()
    {
        if (isDefensing == false && isDeflecting == false && isJumping == false && isAttacking == false && isResting == false && isFainting == false)
        {
            if (Input.GetKey(KeyCode.A))
            {
                SetDefensing(DefensePos.Left, 180);
                //방어 애니메이션
            }
            else if (Input.GetKey(KeyCode.D))
            {
                SetDefensing(DefensePos.Right, 0);
                //방어 애니메이션
            }
            else if (Input.GetKey(KeyCode.W))
            {
                SetDefensing(DefensePos.Up, 0);
                //방어 애니메이션
            }
            if (nowDefensivePosition != DefensePos.Left)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
        else if (isDefensing && isDeflecting == false)
        {
            if (nowDefensivePosition == DefensePos.Left && !Input.GetKey(KeyCode.A) || nowDefensivePosition == DefensePos.Right && !Input.GetKey(KeyCode.D)
                || nowDefensivePosition == DefensePos.Up && !Input.GetKey(KeyCode.W))
            {
                ReleaseDefense();
            }
        }
    }

    protected override void Faint()
    {
        if (isFaintingReady && isAttacking == false)
        {
            isFaintingReady = false;
            StartCoroutine(Fainting());
        }
    }

    protected override void SetDefensing(DefensePos nowDefensePos, float setRotation)
    {
        isDefensing = true;
        nowDefensivePosition = nowDefensePos;
        transform.rotation = Quaternion.Euler(0, setRotation, 0);
    }

    private void Deflect()
    {
        if (isDefensing && isDeflecting == false && Input.GetKeyDown(KeyCode.Space))
        {
            if (nowDefensivePosition == DefensePos.Right)
            {
                StartCoroutine(Deflecting(0));
            }
            else if (nowDefensivePosition == DefensePos.Left)
            {
                StartCoroutine(Deflecting(180));
            }
        }
    }

    IEnumerator Deflecting(int setRotation)
    {
        isDeflecting = true;
        nowDefensivePosition = DefensePos.None;
        BBM.ActionButtonsSetActive(false, false, false);
        transform.rotation = Quaternion.Euler(0, setRotation, 0);
        ChangeAttackRange(new Vector2(0.55f, 2.1f), new Vector2(-0.1f, 0));
        //애니 실행
        yield return new WaitForSeconds(0.15f); //치기 전까지 기다림
        if (rangeInDeflectAbleObj.Count != 0)
        {
            CamShake.CamShakeMod(true, 1.1f);
            for (int nowIndex = 0; nowIndex < rangeInDeflectAbleObj.Count; nowIndex++)
            {
                rangeInDeflectAbleObj[nowIndex].GetComponent<EnemysBullet>().Reflex(true);
            }
        }
        yield return new WaitForSeconds(0.5f); //애니메이션 종료까지 기다림
        InitializationAttackRange();
        if (isWaiting == false)
        {
            BBM.ActionButtonsSetActive(true, false, false);
        }
        isDeflecting = false;
        if (nowActionCoolTime != 0)
        {
            ActionCoolTimeBarSetActive(true);
        }
        if (!Input.GetKey(KeyCode.A))
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        isDefensing = false;
        yield return null;
    }


    protected override void UISetting() //대기시간 및 UI세팅 (일부 공통)
    {
        if (isFainting == false && isWaiting && isDeflecting == false)
        {
            actionCoolTimeImage.fillAmount = nowActionCoolTime / maxActionCoolTime;
            nowActionCoolTime += Time.deltaTime;
            if (nowActionCoolTime >= maxActionCoolTime)
            {
                isWaiting = false;
                nowActionCoolTime = 0;
                ActionCoolTimeBarSetActive(false);
                BBM.ActionButtonsSetActive(true, false, false);
            }
        }
        if (isWaiting == false && isJumping == false && isAttacking == false && isDeflecting == false && isResting == false && isFainting == false) //isSkillButtonPage == false
        {
            ActionCoolTimeBarSetActive(false);
        }
        else if (isDeflecting == true)
        {
            ActionCoolTimeBarSetActive(false);
        }
        nullActionCoolTimeImage.transform.position = Cam.WorldToScreenPoint(transform.position + new Vector3(0, actionCoolTimeImageYPos_F, 0));
    }

    private void WaitingTimeStart() //공격 후의 세팅 (일부 공통, 한번만 실행) 
    {
        isWaiting = true;
        if (nowActionCoolTime < maxActionCoolTime)
        {
            ActionCoolTimeBarSetActive(true);
        }
        BBM.ActionButtonsSetActive(false, false, false);
    }
        
    private void Jump()
    {
        if (isJumping == false && isResting == false && isAttacking == false && isDefensing == false && isDeflecting == false && isFainting == false && Input.GetKey(KeyCode.Space))
        {
            isJumping = true;
            BBM.ActionButtonsSetActive(false, false, false);
            CamShake.JumpStart();
            rigid.AddForce(Vector2.up * jumpPower_F, ForceMode2D.Impulse);
            rigid.gravityScale = setJumpGravityScale_F - 0.5f;
            StartCoroutine(JumpDelay());
        }
        else if (isJumping && transform.position.y < startPos_Vector.y)
        {
            isJumping = false;
            if (isWaiting == false)
            {
                if (BBM.nowButtonPage == ButtonPage.SecondPage)
                {
                    BBM.ActionButtonsSetActive(false, true, false);
                }
                else
                {
                    BBM.ActionButtonsSetActive(true, false, false);
                }
            }
            CamShake.JumpStop();
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
        if (isDefensing == false)
        {
            isAttacking = true;
            BBM.ActionButtonsSetActive(false, false, false);
            StartCoroutine(GoToAttack());
        }
    }

    public void RestStart()
    {
        if (isDefensing == false)
        {
            isResting = true;
            BBM.ActionButtonsSetActive(false, false, false);
            StartCoroutine(Resting());
        }
    }

    IEnumerator Resting()
    {
        int nowRestingCount = 0;
        WaitForSeconds RestWaitTime = new WaitForSeconds(1.25f);
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
        isResting = false;
        BBM.ActionButtonsSetActive(true, false, false);
    }

    IEnumerator GoToAttack()
    {
        Vector3 Movetransform = new Vector3(Speed_F, 0, 0); //이동을 위해 더해줄 연산
        Vector3 Targettransform = new Vector3(BattleSceneManager.Instance.EnemyCharacterPos.x - 5.5f, transform.position.y); //목표 위치

        while (transform.position.x < Targettransform.x) //이동중
        {
            transform.position += Movetransform * Time.deltaTime;
            yield return null;
        }
        transform.position = Targettransform; //이동 완료

        StartCoroutine(Attacking(false, nowAttackCount_I, 0.2f, 0.2f)); //첫번째 공격 실행
    }

    IEnumerator Attacking(bool isLastAttack, int nowAttackCount_I, float delayTime, float linkedAttacksLimitTime) //3연공 재귀로 구현
    {
        bool isComplete = false;
        bool isFail = false;
        float nowdelayTime = 0;
        float nowattacktime_f = 0;

        while (nowdelayTime < delayTime) //연타 방지용 (기본공격 애니메이션 시작 및 타격 지점까지 딜레이)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isFail = true;
            }
            nowdelayTime += Time.deltaTime;
            yield return null;
        }

        if (rangeInEnemy.Count != 0) //기본공격 실행 함수 및 공격 애니메이션 타격 지점
        {
            switch (nowAttackCount_I)
            {
                case 1:
                    CamShake.CamShakeMod(true, 2f);
                    break;
                case 3:
                    CamShake.CamShakeMod(false, 2f); //대각선 떨림 코드로 변경
                    break;
            }
            for (int nowIndex = 0; nowIndex < rangeInEnemy.Count; nowIndex++)
            {
                if (rangeInEnemy[nowIndex] != null)
                {
                    bool isDefence = rangeInEnemy[nowIndex].GetComponent<BasicUnitScript>().nowDefensivePosition == DefensePos.Left ? true : false;
                    rangeInEnemy[nowIndex].GetComponent<BasicUnitScript>().Hit(Damage_I, isDefence);
                }
            }
        }

        while (linkedAttacksLimitTime > nowattacktime_f) //연공 타이밍 계산
        {
            nowattacktime_f += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isComplete = true;
            }
            yield return null;
        }

        if (isLastAttack == false && isFail == false && isComplete)
        {
            nowAttackCount_I++;
            switch (nowAttackCount_I) //공격 실행 애니메이션 시작
            {
                case 2:
                    StartCoroutine(Attacking(false, nowAttackCount_I, 0.2f, 0.25f)); //두번째 공격
                    break;
                case 3:
                    StartCoroutine(Attacking(true, nowAttackCount_I, 0.35f, 0)); //세번째 공격
                    break;
            }
        }
        else //돌아가기
        {
            if (isLastAttack == true)
            {
                yield return new WaitForSeconds(0.5f);
            }
            StartCoroutine(Return());
        }
    }

    IEnumerator Return() //근접공격 후 돌아오기
    {
        Vector3 Movetransform = new Vector3(Speed_F, 0, 0);
        transform.rotation = Quaternion.Euler(0, 180, 0);
        while (transform.position.x > startPos_Vector.x)
        {
            transform.position -= Movetransform * Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.identity;
        transform.position = startPos_Vector;
        nowAttackCount_I = 1;
        isAttacking = false;
        WaitingTimeStart();
    }

    public void SkillUse(int nowUseSkillIndex, int nowUseSkillNeedEnergy)
    {
        if (isDefensing == false && Energy_F >= nowUseSkillNeedEnergy)
        {
            isAttacking = true;
            Energy_F -= nowUseSkillNeedEnergy;
            BBM.ActionButtonsSetActive(false, false, false);
            switch (nowUseSkillIndex)
            {
                case 1:
                    StartCoroutine(SwordAuraSkill());
                    break;
            }
        }
    }

    private IEnumerator SwordAuraSkill()
    {
        float nowDelayTime = 0;
        float maxDelayTime = 1f;
        bool isFailEnchant = true;

        while (nowDelayTime < maxDelayTime)
        {
            if (Input.GetKeyDown(KeyCode.Space) && nowDelayTime > 0.65f && nowDelayTime < 0.85f)
            {
                isFailEnchant = false;
            }
            nowDelayTime += Time.deltaTime;
            yield return null;
        }

        var enchantedSwordAuraObj = ObjectPool.Instance.GetObject(0);
        var enchantedSwordAuraObjComponent = enchantedSwordAuraObj.GetComponent<SwordAura>();
        enchantedSwordAuraObj.transform.position = transform.position + (Vector3)new Vector2(2.5f, 0);
        if (isFailEnchant == false)
        {
            enchantedSwordAuraObjComponent.IsEnchanted = true;
        }

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
        if (isFaintingReady == false)
        {
            WaitingTimeStart();
        }
    }

    protected override void Dead()
    {
        print("사망");
        //사망 애니 및 이벤트
    }

    protected override IEnumerator Fainting()
    {
        isFainting = true;
        nowDefensivePosition = DefensePos.None;
        isDefensing = false;
        BBM.ActionButtonsSetActive(false, false, false);
        yield return new WaitForSeconds(5); //나중에 매개변수로 레벨에 따라서 기절 시간 넣기
        BBM.ActionButtonsSetActive(true, false, false);
        Energy_F += 8; //나중에 매개변수로 레벨에 따라서 기력 차는 양 증가
        isFainting = false;
        nowActionCoolTime = maxActionCoolTime;
        WaitingTimeStart();
    }
}