using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : BasicUnitScript
{
    public bool isSkillButtonPage;

    [Header("공격에 필요한 오브젝트 모음")]
    [SerializeField]
    private GameObject swordAuraObj;

    private void Awake()
    {
        StartSetting();
    }

    private void Update()
    {
        Defense();
        Deflect();
        Jump();
        Faint();
    }

    void FixedUpdate()
    {
        UISetting();
        if (Input.GetKey(KeyCode.Q))
        {
            Energy_F -= 1;
        }
    }

    protected override void StartSetting() //초기 세팅 (일부 공통)
    {
        Cam = Camera.main;
        rigid = gameObject.GetComponent<Rigidbody2D>();
        startPos_Vector = transform.position;
        maxActionCoolTime -= GameManager.Instance.ReduceCoolTimeLevel * 0.5f;
        nowActionCoolTime = 0;
        MaxHp_F += GameManager.Instance.MaxHpUpgradeLevel * 5;
        Hp_F = MaxHp_F;
        MaxEnergy_F += GameManager.Instance.MaxEnergyUpgradeLevel * 5;
        Energy_F = MaxEnergy_F;
        Damage_I += GameManager.Instance.DamageUpgradeLevel;
        nowAttackCount_I = 1;
        BattleSceneManager.Instance.PlayerCharacterPos = transform.position;
    }

    protected override void Defense()
    {
        if (isDefensing == false && isDeflecting == false && isJumping == false && isAttacking == false && isResting == false && isFainting == false)
        {
            if (Input.GetKey(KeyCode.A))
            {
                SetDefensing((int)NowDefensePos.Left, 180);
                //방어 애니메이션
            }
            else if (Input.GetKey(KeyCode.D))
            {
                SetDefensing((int)NowDefensePos.Right, 0);
                //방어 애니메이션
            }
            else if (Input.GetKey(KeyCode.W))
            {
                SetDefensing((int)NowDefensePos.Up, 0);
                //방어 애니메이션
            }
            if (nowDefensivePosition_B[(int)NowDefensePos.Left] == false)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
        else if (isDefensing && isDeflecting == false)
        {
            bool isInputDefenseKey = false;
            if (Input.GetKeyUp(KeyCode.W))
            {
                nowDefensivePosition_B[(int)NowDefensePos.Up] = false;
                for (int nowDefensePosIndex = 0; nowDefensePosIndex < (int)NowDefensePos.DefensePosCount; nowDefensePosIndex++)
                {
                    if (nowDefensivePosition_B[nowDefensePosIndex] == true)
                    {
                        isInputDefenseKey = true;
                    }
                }
                if (isInputDefenseKey == false)
                {
                    isDefensing = false;
                }
            }
            else if (Input.GetKeyUp(KeyCode.D))
            {
                nowDefensivePosition_B[(int)NowDefensePos.Right] = false;
                for (int nowDefensePosIndex = 0; nowDefensePosIndex < (int)NowDefensePos.DefensePosCount; nowDefensePosIndex++)
                {
                    if (nowDefensivePosition_B[nowDefensePosIndex] == true)
                    {
                        isInputDefenseKey = true;
                    }
                }
                if (isInputDefenseKey == false)
                {
                    isDefensing = false;
                }
            }
            else if (Input.GetKeyUp(KeyCode.A))
            {
                nowDefensivePosition_B[(int)NowDefensePos.Left] = false;
                for (int nowDefensePosIndex = 0; nowDefensePosIndex < (int)NowDefensePos.DefensePosCount; nowDefensePosIndex++)
                {
                    if (nowDefensivePosition_B[nowDefensePosIndex] == true)
                    {
                        isInputDefenseKey = true;
                    }
                }
                if (isInputDefenseKey == false)
                {
                    isDefensing = false;
                }
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

    protected override void SetDefensing(int defensingDirectionIndex, float setRotation)
    {
        isDefensing = true;
        nowDefensivePosition_B[defensingDirectionIndex] = true;
        transform.rotation = Quaternion.Euler(0, setRotation, 0);
    }

    private void Deflect()
    {
        if (isDefensing)
        {
            if (nowDefensivePosition_B[(int)NowDefensePos.Right] == true && isDeflecting == false && Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(Deflecting((int)NowDefensePos.Right, 0));
            }
            else if (nowDefensivePosition_B[(int)NowDefensePos.Left] == true && isDeflecting == false && Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(Deflecting((int)NowDefensePos.Left, 180));
            }
        }
    }

    IEnumerator Deflecting(int nowDefensePosIndex, int setRotation)
    {
        var attackRangeObjComponent = attackRangeObj.GetComponent<BoxCollider2D>();
        isDeflecting = true;
        nowDefensivePosition_B[nowDefensePosIndex] = false;
        ActionButtonsSetActive(false);
        transform.rotation = Quaternion.Euler(0, setRotation, 0);
        attackRangeObjComponent.size = new Vector2(0.55f, 2.1f);
        attackRangeObjComponent.offset = new Vector2(-0.1f, 0f);
        //애니 실행
        yield return new WaitForSeconds(0.3f); //치기 전까지 기다림
        //범위 내의 반사 가능한 오브젝트 상호작용
        yield return new WaitForSeconds(0.5f); //애니메이션 종료까지 기다림
        attackRangeObjComponent.size = new Vector2(0.8f, 2.1f);
        attackRangeObjComponent.offset = new Vector2(0f, 0f);
        if (isWaiting == false)
        {
            ActionButtonsSetActive(true);
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
                ActionButtonsSetActive(true);
            }
        }
        if (isWaiting == false && isJumping == false && isAttacking == false && isDeflecting == false && isResting == false && isFainting == false && isSkillButtonPage == false)
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
        ActionButtonsSetActive(false);
    }

    private void Jump()
    {
        if (isJumping == false && isResting == false && isAttacking == false && isDefensing == false && isDeflecting == false && isFainting == false && Input.GetKey(KeyCode.Space))
        {
            isJumping = true;
            ActionButtonsSetActive(false);
            rigid.AddForce(Vector2.up * jumpPower_F, ForceMode2D.Impulse);
            rigid.gravityScale = setJumpGravityScale_F - 0.5f;
            StartCoroutine(JumpDelay());
        }
        else if (isJumping && transform.position.y < startPos_Vector.y)
        {
            isJumping = false;
            if (isWaiting == false)
            {
                ActionButtonsSetActive(true);
            }
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
            ActionButtonsSetActive(false);
            StartCoroutine(GoToAttack());
        }
    }

    public void RestStart()
    {
        if (isDefensing == false)
        {
            isResting = true;
            ActionButtonsSetActive(false);
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
        ActionButtonsSetActive(true);
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
                    CamShake.NowCamShakeStart(0.3f, 0.5f);
                    break;
                case 3:
                    CamShake.NowCamShakeStart(0.3f, 1);
                    break;
            }
            for (int nowIndex = 0; nowIndex < rangeInEnemy.Count; nowIndex++)
            {
                if (rangeInEnemy[nowIndex] != null)
                {
                    rangeInEnemy[nowIndex].GetComponent<BasicUnitScript>().Hp_F -= Damage_I;
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
            ActionButtonsSetActive(false);
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

        if (isFailEnchant == false)
        {
            var enchantedSwordAuraObjs = Instantiate(swordAuraObj, transform.position + (Vector3)new Vector2(2.5f, 0), Quaternion.identity).GetComponent<SwordAura>();
            enchantedSwordAuraObjs.IsEnchanted = true;
        }
        else 
        {
            Instantiate(swordAuraObj, transform.position + (Vector3)new Vector2(2.5f, 0), Quaternion.identity).GetComponent<SwordAura>();
        }

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
        if (isFaintingReady == false)
        {
            BattleButtonManager.Instance.ButtonsPageChange(false, true);
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
        ActionButtonsSetActive(false);
        yield return new WaitForSeconds(5); //나중에 매개변수로 레벨에 따라서 기절 시간 넣기
        ActionButtonsSetActive(true);
        Energy_F += 8; //나중에 매개변수로 레벨에 따라서 기력 차는 양 증가
        isFainting = false;
        nowActionCoolTime = maxActionCoolTime;
        BattleButtonManager.Instance.ButtonsPageChange(false, true);
        WaitingTimeStart();
    }

    private void ActionButtonsSetActive(bool setActive)
    {
        var battleButtonManagerInstance = BattleButtonManager.Instance;

        if (setActive)
        {
            isSkillButtonPage = false;
        }
        else
        {
            isSkillButtonPage = true;
        }

        for (int nowButtonPage = battleButtonManagerInstance.minPage;
        nowButtonPage <= battleButtonManagerInstance.maxPage; nowButtonPage++)
        {
            if (setActive && battleButtonManagerInstance.nowPage == nowButtonPage)
            {
                battleButtonManagerInstance.ButtonPageObjs[nowButtonPage].SetActive(true);
            }
            else
            {
                battleButtonManagerInstance.ButtonPageObjs[nowButtonPage].SetActive(false);
            }
        }
    }
}