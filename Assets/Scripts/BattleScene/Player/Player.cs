using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum NowPlayerProperty
{
    BasicProperty,
    NatureProperty,
    ForceProperty,
    FlameProperty,
    TheHolySpiritProperty,
    AngelProperty,
    PropertyTotalNumber
}

public class Player : BasicUnitScript
{
    #region 쉴드 관련 변수 / 오브젝트
    private float shieldHp_F;
    public float ShieldHp_F
    {
        get { return shieldHp_F; }
        set
        {
            if (value <= 0)
            {
                shieldHp_F = 0;
                if (nowProperty == NowPlayerProperty.TheHolySpiritProperty)
                {
                    hpText.color = basicHpTextColor;
                    TheHolySpiritPropertyBuff(false);
                    TheHolySpiritPropertyDeBuff(true);
                    hpText.text = $"{(Hp_F):N0}/{(MaxHp_F):N0}";
                }
            }
            else
            {
                shieldHp_F = value;
            }
            
            unitShieldHpBars.fillAmount = ShieldHp_F / maxShieldHp_F;
            
            if (value > 0 && nowProperty == NowPlayerProperty.TheHolySpiritProperty)
            {
                hpText.text = $"{ShieldHp_F}/{maxShieldHp_F}";
            }
        }
    }

    private float maxShieldHp_F;

    [SerializeField]
    [Tooltip("유닛 실드 체력바 이미지")]
    private Image unitShieldHpBars;
    #endregion

    private float maxChangePropertyCoolTime = 35; //최대 속성 변경 시간

    private float nowChangePropertyCoolTime; //현재 속성 변경 시간

    public float NowChangePropertyCoolTime
    {
        get
        {
            return nowChangePropertyCoolTime;
        }
        set
        {
            if (value >= maxChangePropertyCoolTime)
            {
                StartCoroutine(ChangeProperty(false));
                StartCoroutine(PropertyPassiveAbilityStart());
            }
            else
            {
                nowChangePropertyCoolTime = value;
            }
        }
    }

    private float maxPropertyTimeLimit = 25; //최대 속성 지속시간

    private float nowPropertyTimeLimit; // 현재 속성 남은 지속시간

    public float NowPropertyTimeLimit
    {
        get { return nowPropertyTimeLimit; }
        set
        {
            if (value >= maxPropertyTimeLimit)
            {
                StartCoroutine(ChangeProperty(true));
            }
            else
            {
                nowPropertyTimeLimit = value;
            }
        }
    }

    [HideInInspector]
    public NowPlayerProperty nowProperty;

    private int nextPropertyIndex;

    private bool isChangePropertyReady;

    private bool isResurrectionOpportunityExists;

    private bool angelPropertyBuffing;

    private bool isToBurn;

    private float maxNaturePassiveCount;

    private float nowNaturePassiveCount;
    public float NowNaturePassiveCount
    {
        get { return nowNaturePassiveCount; }
        set
        {
            if (value >= maxNaturePassiveCount)
            {
                isSpawnNatureBead = true;
                oP.GetObject((int)PoolObjKind.PlayerHpRecoveryBead);
            }
            else
            {
                nowNaturePassiveCount = value;
            }
        }
    }

    [HideInInspector]
    public bool isSpawnNatureBead;

    private BattleButtonManager bBM;

    private ObjectPool oP;

    [SerializeField]
    [Tooltip("현재 플레이어 속성 아이콘")]
    private Image nowPropertyImage;

    [SerializeField]
    [Tooltip("플레이어 속성 아이콘 스프라이트 모음")]
    private Sprite[] nowPropertyIconImages;

    [SerializeField]
    [Tooltip("플레이어 애니메이션")]
    private Animator playerAnimator;
    
    #region 체력 텍스트 색 값들
    [Header("체력 텍스트 색 값들")]
    [SerializeField]
    [Tooltip("기본 체력 텍스트 색")]
    private Color basicHpTextColor;

    [SerializeField]
    [Tooltip("성령 속성 보호막 체력 텍스트 색")]
    private Color shieldHpTextColor;
    #endregion

    protected override void Update()
    {
        base.Update();
        CountDownPropertyTime();
        Deflect();
        Defense();
        Jump();
    }

    protected override void StartSetting() //초기 세팅 (일부 공통)
    {
        var gameManager_Ins = GameManager.Instance;
        int plusMultiplicationMaxHpPerLevel = 5;
        int plusMultiplicationMaxEnergyPerLevel = 5;
        float plusMultiplicationMaxActionCoolTimePerLevel = 0.5f;
        float basicMaxActionCoolTime = 3.5f;

        maxActionCoolTime = basicMaxActionCoolTime - (gameManager_Ins.ReduceCoolTimeLevel * plusMultiplicationMaxActionCoolTimePerLevel);
        MaxHp_F += gameManager_Ins.MaxHpUpgradeLevel * plusMultiplicationMaxHpPerLevel;
        MaxEnergy_F += gameManager_Ins.MaxEnergyUpgradeLevel * plusMultiplicationMaxEnergyPerLevel;
        Damage_I += gameManager_Ins.DamageUpgradeLevel;
        restWaitTime = 1.25f;
        maxShieldHp_F = 2;
        maxDreamyFigure_F = 20;
        maxNaturePassiveCount = 5;

        nowState = NowState.Standingby;
        nowProperty = NowPlayerProperty.BasicProperty;
        bBM = BattleButtonManager.Instance;
        oP = ObjectPool.Instance;
        isResurrectionOpportunityExists = true;

        BattleSceneManager.Instance.playerCharacterPos = transform.position;
        nextPropertyIndex = Random.Range((int)NowPlayerProperty.NatureProperty, (int)NowPlayerProperty.PropertyTotalNumber);
        nowPropertyImage.GetComponent<Image>().sprite = nowPropertyIconImages[(int)nowProperty];
        Energy_F = MaxEnergy_F;
        Hp_F = MaxHp_F;
    }

    public override void Hit(float damage, bool isDefending)
    {
        if (isInvincibility == false)
        {
            if (ShieldHp_F > 0 && !isDefending)
            {
                ShieldHp_F -= 1;
            }
            else if (isDefending)
            {
                Energy_F -= 1;
                DreamyFigure_F += 1;
                playerAnimator.SetTrigger("DefenceIntermediateMotion");
            }
            else
            {
                Hp_F -= nowProperty == NowPlayerProperty.ForceProperty ? damage * 2f : damage;
                DreamyFigure_F += 2;
            }
        }
    }

    protected override void Defense()
    {
        if (nowState == NowState.Standingby && Hp_F > 0 && isChangePropertyReady == false)
        {
            if (Input.GetKey(KeyCode.A))
            {
                SetDefensing(DefensePos.Left, 180);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                SetDefensing(DefensePos.Right, 0);
            }
            else if (Input.GetKey(KeyCode.W))
            {
                SetDefensing(DefensePos.Up, 0);
            }

            if (nowDefensivePosition != DefensePos.Left)
            {
                transform.rotation = Quaternion.identity;
            }
        }
        else if (nowState == NowState.Defensing)
        {
            if (nowDefensivePosition == DefensePos.Left && !Input.GetKey(KeyCode.A) || nowDefensivePosition == DefensePos.Right && !Input.GetKey(KeyCode.D)
                || nowDefensivePosition == DefensePos.Up && !Input.GetKey(KeyCode.W))
            {
                string nowDefenceAnimName = (nowDefensivePosition == DefensePos.Up) ? "Defence(Top)" : "Defence(Left&Right)";

                playerAnimator.SetBool(nowDefenceAnimName, false);
                ReleaseDefense();
            }
        }
    }

    protected override void SetDefensing(DefensePos nowDefensePos, float setRotation)
    {
        string nowDefenceAnimName = (nowDefensePos == DefensePos.Up) ? "Defence(Top)" : "Defence(Left&Right)";

        playerAnimator.SetBool(nowDefenceAnimName, true);
        nowState = NowState.Defensing;
        nowDefensivePosition = nowDefensePos;
        transform.rotation = Quaternion.Euler(0, setRotation, 0);
    }

    IEnumerator ChangeProperty(bool isChangeBasicProperty)
    {
        NowPropertyTimeLimit = 0;
        NowChangePropertyCoolTime = 0;
        isChangePropertyReady = true;
        nowActionCoolTime = 0;

        while (true)
        {
            if (nowState == NowState.Standingby && angelPropertyBuffing == false)
            {
                break;
            }
            yield return null;
        }

        nowState = NowState.ChangingProperties;
        Invincibility(true);
        bBM.ActionButtonsSetActive(false, false, false);
        transform.rotation = Quaternion.identity;

        if (isChangeBasicProperty)
        {
            nowProperty = NowPlayerProperty.BasicProperty;
            if(nowProperty == NowPlayerProperty.TheHolySpiritProperty)
            {
                hpText.text = $"{(Hp_F):N0}/{(MaxHp_F):N0}";
            }
        }
        else
        {
            nowProperty = (NowPlayerProperty)nextPropertyIndex;
            switch (nowProperty)
            {
                case NowPlayerProperty.NatureProperty:
                    break;
                case NowPlayerProperty.ForceProperty:
                    break;
                case NowPlayerProperty.FlameProperty:
                    break;
                case NowPlayerProperty.TheHolySpiritProperty:
                    break;
                case NowPlayerProperty.AngelProperty:
                    break;
            }
            nextPropertyIndex = ((NowPlayerProperty)nextPropertyIndex == NowPlayerProperty.AngelProperty) ? (int)NowPlayerProperty.NatureProperty : nextPropertyIndex + 1;
        }
        StartCoroutine(EndingPropertyChanges());
        nowPropertyImage.GetComponent<Image>().sprite = nowPropertyIconImages[(int)nowProperty];
    }

    IEnumerator EndingPropertyChanges() //나중에 애니메이션 나오면 일반함수로 전환, 그리고 속성 변경 애니메이션 끝날때쯤 변경한 이 함수 실행
    {
        yield return new WaitForSeconds(2);
        isChangePropertyReady = false;
        nowState = NowState.Standingby;
        Invincibility(false);

        if (BattleButtonManager.Instance.nowButtonPage == ButtonPage.FirstPage)
        {
            bBM.ActionButtonsSetActive(true, false, true);
        }
        else
        {
            bBM.ActionButtonsSetActive(false, true, false);
        }
    }

    public void PropertyChangeStart()
    {
        if (nowState == NowState.Standingby && DreamyFigure_F >= 10)
        {
            DreamyFigure_F -= 10;
            StartCoroutine(ChangeProperty(false));
        }
    }

    private void CountDownPropertyTime()
    {
        if (isChangePropertyReady == false)
        {
            if (nowProperty != NowPlayerProperty.BasicProperty)
            {
                NowPropertyTimeLimit += Time.deltaTime;
            }
            else
            {
                NowChangePropertyCoolTime += Time.deltaTime;
            }
        }
    }

    private void Deflect()
    {
        if (Input.GetKeyDown(KeyCode.Space) && nowState == NowState.Defensing)
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
        bool isAlreadyShake = false;

        nowState = NowState.Deflecting;
        nowDefensivePosition = DefensePos.None;
        bBM.ActionButtonsSetActive(false, false, false);
        transform.rotation = Quaternion.Euler(0, setRotation, 0);
        ChangeAttackRange(new Vector2(0.7f, 2.6f), new Vector2(0, 0));

        playerAnimator.SetBool("Paring", true);
        playerAnimator.SetBool("Defence(Left&Right)", false);

        yield return new WaitForSeconds(0.15f); //치기 전까지 기다림
        
        for (int nowIndex = 0; nowIndex < rangeInDeflectAbleObj.Count; nowIndex++)
        {
            if (rangeInDeflectAbleObj[nowIndex].GetComponent<EnemysBullet>().isDeflectAble)
            {
                if (isAlreadyShake == false)
                {
                    isAlreadyShake = true;
                    CamShake.CamShakeMod(true, 1.1f);
                }
                rangeInDeflectAbleObj[nowIndex].GetComponent<EnemysBullet>().Reflex(BulletState.Deflecting);
            }
        }
        yield return new WaitForSeconds(0.25f); //애니메이션 종료까지 기다림
        
        playerAnimator.SetBool("Paring", false);
        
        InitializationAttackRange();

        if (nowState != NowState.Defensing)
        {
            nowState = NowState.Standingby;
        }

        if (isWaiting == false && isChangePropertyReady == false)
        {
            bBM.ActionButtonsSetActive(true, false, false);
        }
        if (nowActionCoolTime != 0)
        {
            ActionCoolTimeBarSetActive(true);
        }
        if (!Input.GetKey(KeyCode.A))
        {
            transform.rotation = Quaternion.identity;
        }
        yield return null;
    }


    protected override void UISetting() //대기시간 및 UI세팅 (일부 공통)
    {
        if (isWaiting && (nowState == NowState.Standingby || nowState == NowState.Jumping || nowState == NowState.Defensing)) //isFainting == false && isWaiting && isDeflecting == false
        {
            actionCoolTimeImage.fillAmount = nowActionCoolTime / maxActionCoolTime;
            nowActionCoolTime += Time.deltaTime;
            if (nowActionCoolTime >= maxActionCoolTime)
            {
                WaitingTimeEnd();
                ActionCoolTimeBarSetActive(false);
                bBM.ActionButtonsSetActive(true, false, false);
            }
        }
        else if (nowState == NowState.Deflecting || nowState == NowState.Fainting || nowState == NowState.ChangingProperties)
        {
            ActionCoolTimeBarSetActive(false);
        }
        nullActionCoolTimeImage.transform.position = Cam.WorldToScreenPoint(transform.position + new Vector3(0, actionCoolTimeImageYPos_F, 0));
    }

    private void WaitingTimeStart() //공격 후의 세팅 (일부 공통, 한번만 실행) 
    {
        nowState = NowState.Standingby;

        if (isChangePropertyReady == false && Hp_F > 0)
        {
            isWaiting = true;
            if (nowActionCoolTime < maxActionCoolTime)
            {
                ActionCoolTimeBarSetActive(true);
            }
            bBM.ActionButtonsSetActive(false, false, false);
        }
    }

    private void Jump()
    {
        if (nowState == NowState.Standingby && Input.GetKey(KeyCode.Space) && Hp_F > 0 && isChangePropertyReady == false)
        {
            nowState = NowState.Jumping;
            bBM.ActionButtonsSetActive(false, false, false);
            CamShake.JumpStart();
            rigid.AddForce(Vector2.up * jumpPower_F, ForceMode2D.Impulse);
            rigid.gravityScale = setJumpGravityScale_F - 0.5f;
            playerAnimator.SetTrigger("Jumping");
            StartCoroutine(JumpDelay());
        }
        else if (nowState == NowState.Jumping && transform.position.y < startPos_Vector.y)
        {
            nowState = NowState.Standingby;
            if (isWaiting == false && isChangePropertyReady == false)
            {
                if (bBM.nowButtonPage == ButtonPage.SecondPage)
                {
                    bBM.ActionButtonsSetActive(false, true, false);
                }
                else
                {
                    bBM.ActionButtonsSetActive(true, false, false);
                }
            }

            playerAnimator.SetBool("JumpIntermediateMotion", false);
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
        playerAnimator.SetBool("JumpIntermediateMotion", true);
        rigid.gravityScale = setJumpGravityScale_F * 1.5f;
    }

    public void CloseAttackStart()
    {
        if (nowState == NowState.Standingby)
        {
            nowState = NowState.Attacking;
            bBM.ActionButtonsSetActive(false, false, false);
            StartCoroutine(GoToAttack());
        }
    }

    public void RestStart()
    {
        if (nowState == NowState.Standingby)
        {
            nowState = NowState.Resting;
            bBM.ActionButtonsSetActive(false, false, false);
            StartCoroutine(Resting());
        }
    }

    protected override IEnumerator Resting()
    {
        int nowRestingCount = 0;
        WaitForSeconds RestWaitTime = new WaitForSeconds(restWaitTime);

        battleUIObjScript.ChangeRestAnimObjScale();
        battleUIAnimator.SetBool("NowResting", true);

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

        battleUIAnimator.SetBool("NowResting", false);
        nowState = NowState.Standingby;
        if (isChangePropertyReady == false)
        {
            bBM.ActionButtonsSetActive(true, false, false);
        }
    }

    IEnumerator GoToAttack()
    {
        Vector3 Movetransform = new Vector3(Speed_F, 0, 0); //이동을 위해 더해줄 연산
        Vector3 Targettransform = new Vector3(BattleSceneManager.Instance.enemyCharacterPos.x - 5.5f, transform.position.y); //목표 위치

        playerAnimator.SetBool("Moving", true);

        while (transform.position.x < Targettransform.x) //이동중
        {
            transform.position += Movetransform * Time.deltaTime;
            yield return null;
        }
        transform.position = Targettransform; //이동 완료

        playerAnimator.SetBool("Moving", false);
        StartCoroutine(Attacking(false, nowAttackCount_I, 0.2f, 0.2f)); //첫번째 공격 실행
        playerAnimator.SetTrigger("BasicAttack");
    }

    IEnumerator Attacking(bool isLastAttack, int nowAttackCount_I, float delayTime, float linkedAttacksLimitTime) //3연공 재귀로 구현
    {
        bool isComplete = false;
        bool isFail = false;

        float nowdelayTime = 0;
        float nowattacktime_f = 0;

        int maxEnemyIndex;

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
                maxEnemyIndex = rangeInEnemy.Count - 1;

                if (rangeInEnemy[nowIndex] != null)
                {
                    var unitScriptComponenet = rangeInEnemy[nowIndex].GetComponent<BasicUnitScript>();

                    bool isDefence = (unitScriptComponenet.nowState == NowState.Defensing && unitScriptComponenet.nowDefensivePosition == DefensePos.Left) ? true : false;

                    unitScriptComponenet.Hit(Damage_I, isDefence);
                    if (isToBurn == false && nowProperty == NowPlayerProperty.FlameProperty)
                    {
                        unitScriptComponenet.BurnDamageStart();
                        if (nowIndex == maxEnemyIndex)
                        {
                            isToBurn = true;
                        }
                    }
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
                    StartCoroutine(Attacking(false, nowAttackCount_I, 0.2f, 0.25f));
                    playerAnimator.SetTrigger("BasicSecondAttackHitActionCompleat");
                    break;
                case 3:
                    StartCoroutine(Attacking(true, nowAttackCount_I, 0.35f, 0));
                    playerAnimator.SetTrigger("BasicThirdAttackHitActionCompleat");
                    break;
            }
        }
        else //돌아가기
        {
            if (isToBurn)
            {
                isToBurn = false;
            }

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

        playerAnimator.SetBool("Moving", true);
        while (transform.position.x > startPos_Vector.x)
        {
            transform.position -= Movetransform * Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.identity;
        transform.position = startPos_Vector;
        nowAttackCount_I = 1;

        playerAnimator.SetBool("Moving", false);
        WaitingTimeStart();
    }

    public void SkillUse(int nowUseSkillIndex, int nowUseSkillNeedEnergy)
    {
        if (nowState == NowState.Standingby && Energy_F >= nowUseSkillNeedEnergy)
        {
            Energy_F -= nowUseSkillNeedEnergy;
            bBM.ActionButtonsSetActive(false, false, false);
            switch (nowUseSkillIndex)
            {
                case 1:
                    StartCoroutine(SwordAuraSkill());
                    playerAnimator.SetBool("FirstSkill", true);
                    break;
            }
        }
    }

    private IEnumerator SwordAuraSkill()
    {
        float nowDelayTime = 0;
        float maxDelayTime = 0.6f;
        bool isFailEnchant = true;

        nowState = NowState.Attacking;

        while (nowDelayTime < maxDelayTime)
        {
            if (Input.GetKeyDown(KeyCode.Space) && nowDelayTime > 0.4f && nowDelayTime < 0.55f)
            {
                isFailEnchant = false;
            }
            nowDelayTime += Time.deltaTime;
            yield return null;
        }

        var enchantedSwordAuraObj = oP.GetObject((int)PoolObjKind.PlayerSwordAura);
        var enchantedSwordAuraObjComponent = enchantedSwordAuraObj.GetComponent<SwordAura>();

        if (isFailEnchant == false)
        {
            enchantedSwordAuraObjComponent.IsEnchanted = true;
        }

        yield return new WaitForSeconds(0.3f);

        playerAnimator.SetBool("FirstSkill", false);

        if (Energy_F > 0)
        {
            WaitingTimeStart();
        }
        else
        {
            nowState = NowState.Standingby;
        }
    }

    protected override IEnumerator Dead()
    {
        if (nowProperty == NowPlayerProperty.AngelProperty && isResurrectionOpportunityExists)
        {
            isResurrectionOpportunityExists = false;
            while (nowState != NowState.Standingby)
            {
                yield return null;
            }
            StartCoroutine(Resurrection());
        }
        else
        {
            //사망 애니 및 이벤트
        }
    }

    private IEnumerator Resurrection()
    {
        int recoveryFixedValue = 20;
        int ResurrectionStatsValueSharingValue = 5;

        Invincibility(true);
        AngelPropertyBuff(true);
        nowState = NowState.Resurrection;
        WaitingTimeEnd();
        bBM.ActionButtonsSetActive(false, false, false);

        while (true)
        {
            Hp_F += Time.deltaTime * (MaxHp_F / recoveryFixedValue);
            Energy_F += (Energy_F < maxEnergy_F / ResurrectionStatsValueSharingValue) ? Time.deltaTime * (maxEnergy_F / recoveryFixedValue) : 0;
            if (Hp_F >= MaxHp_F / ResurrectionStatsValueSharingValue && Energy_F >= maxEnergy_F / ResurrectionStatsValueSharingValue)
            {
                Hp_F = MaxHp_F / ResurrectionStatsValueSharingValue;
                if (Energy_F <= maxEnergy_F / ResurrectionStatsValueSharingValue)
                {
                    Energy_F = maxEnergy_F / ResurrectionStatsValueSharingValue;
                }
                break;
            }
            yield return null;
        }

        bBM.ActionButtonsSetActive(true, false, false);
        nowState = NowState.Standingby; 
        yield return new WaitForSeconds(15f);
        AngelPropertyBuff(false);
        Invincibility(false);
    }

    private void AngelPropertyBuff(bool isBuffing)
    {
        angelPropertyBuffing = isBuffing;

        Damage_I = isBuffing ? Damage_I * 2 : Damage_I / 2;
        maxActionCoolTime = isBuffing ? maxActionCoolTime - 1 : maxActionCoolTime + 1;
        Speed_F = isBuffing ? Speed_F * 1.5f : Speed_F / 1.5f;
    }

    private void TheHolySpiritPropertyBuff(bool isBuffing)
    {
        float originalRestWaitTime = 1.25f;
        float reducedMaxActionCoolTime = maxActionCoolTime / 4;
        float reducedRestWaitTime = originalRestWaitTime / 5;

        maxActionCoolTime = isBuffing ? maxActionCoolTime - reducedMaxActionCoolTime : maxActionCoolTime + reducedMaxActionCoolTime;
        restWaitTime = isBuffing ? restWaitTime - reducedRestWaitTime : restWaitTime + reducedRestWaitTime;
        Damage_I = isBuffing ? Damage_I * 1.5f : Damage_I / 1.5f;
        Speed_F = isBuffing ? Speed_F * 1.25f : Speed_F / 1.25f;
    }

    private void TheHolySpiritPropertyDeBuff(bool isDeBuffing)
    {
        float originalRestWaitTime = 1.25f;
        float reducedMaxActionCoolTime = maxActionCoolTime / 4;
        float reducedRestWaitTime = originalRestWaitTime / 5;

        maxActionCoolTime = isDeBuffing ? maxActionCoolTime + reducedMaxActionCoolTime : maxActionCoolTime - reducedMaxActionCoolTime;
        restWaitTime = isDeBuffing ? restWaitTime + reducedRestWaitTime : restWaitTime - reducedRestWaitTime;
        Damage_I = isDeBuffing ? Damage_I / 1.5f : Damage_I * 1.5f;
        Speed_F = isDeBuffing ? Speed_F / 1.25f : Speed_F * 1.25f;
    }

    protected override IEnumerator Fainting()
    {
        if (nowDefensivePosition == DefensePos.Left || nowDefensivePosition == DefensePos.Right)
        {
            playerAnimator.SetBool("Defence(Left&Right)", false);
        }
        else if (nowDefensivePosition == DefensePos.Up)
        {
            playerAnimator.SetBool("Defence(Top)", false);
        }

        battleUIObjScript.ChangeFaintAnimObjScale();
        playerAnimator.SetBool("Stuning", true);
        battleUIAnimator.SetBool("NowFainting", true);

        nowState = NowState.Fainting;
        nowDefensivePosition = DefensePos.None;
        bBM.ActionButtonsSetActive(false, false, false);
        yield return new WaitForSeconds(5); //나중에 매개변수로 레벨에 따라서 기절 시간 넣기

        playerAnimator.SetBool("Stuning", false);
        battleUIAnimator.SetBool("NowFainting", false);

        Energy_F += 8; //나중에 매개변수로 레벨에 따라서 기력 차는 양 증가
        nowActionCoolTime = maxActionCoolTime;


        if (isChangePropertyReady == false && Hp_F > 0)
        {
            bBM.ActionButtonsSetActive(true, false, false);
        }

        WaitingTimeStart();
    }

    private IEnumerator PropertyPassiveAbilityStart()
    {
        NowChangePropertyCoolTime = 0;

        switch (nowProperty)
        {
            case NowPlayerProperty.NatureProperty:
                while (nowProperty == NowPlayerProperty.NatureProperty)
                {
                    if (isSpawnNatureBead == false && nowProperty == NowPlayerProperty.NatureProperty)
                    {
                        NowNaturePassiveCount += Time.deltaTime;
                    }
                    yield return null;
                }
                NowNaturePassiveCount = 0;
                break;

            case NowPlayerProperty.ForceProperty:
                float enhancedDamage = Damage_I / 2f;
                float reducedMaxActionCoolTime = maxActionCoolTime / 5;

                maxActionCoolTime -= reducedMaxActionCoolTime;
                Damage_I += enhancedDamage;
                while (nowProperty == NowPlayerProperty.ForceProperty)
                {
                    yield return null;
                }
                Damage_I -= enhancedDamage;
                maxActionCoolTime += reducedMaxActionCoolTime;
                break;

            case NowPlayerProperty.TheHolySpiritProperty:

                ShieldHp_F = 2;
                TheHolySpiritPropertyBuff(true);
                hpText.color = shieldHpTextColor;
                unitShieldHpBars.fillAmount = ShieldHp_F / maxShieldHp_F;

                while (nowProperty == NowPlayerProperty.TheHolySpiritProperty)
                {
                    yield return null;
                }

                if (ShieldHp_F > 0)
                {
                    TheHolySpiritPropertyBuff(false);
                }
                else
                {
                    TheHolySpiritPropertyDeBuff(false);
                }

                hpText.color = basicHpTextColor;
                shieldHp_F = 0;
                break;
        }
    }
}