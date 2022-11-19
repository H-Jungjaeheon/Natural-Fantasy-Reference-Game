using System.Collections;
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

public enum PropertyColor
{
    Gray,
    Green,
    Brown,
    Red,
    Yellow,
    White
}

public enum NowStatUIState
{
    Basic,
    Invincibility
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
                    hpText.text = $"{(Hp):N0}/{(MaxHp):N0}";
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

    private const float maxShieldHp_F = 2;

    [SerializeField]
    [Tooltip("유닛 실드 체력바 이미지")]
    private Image unitShieldHpBars;
    #endregion

    #region 속성 관련 변수
    private float maxChangePropertyCoolTime = 35; //최대 속성 변경 시간

    public float nowChangePropertyCoolTime; //현재 속성 변경 시간
    
    public float NowChangePropertyCoolTime
    {
        get
        {
            return nowChangePropertyCoolTime;
        }
        set
        {
            if (value > maxChangePropertyCoolTime)
            {
                nowChangePropertyCoolTime = maxChangePropertyCoolTime;
                isChangeProperty = true;
                StartCoroutine(ChangeProperty(false, false));
            }
            else
            {
                if (nowProperty == NowPlayerProperty.BasicProperty)
                {
                    nowPropertyLimitTimeImage.fillAmount = (maxChangePropertyCoolTime - NowChangePropertyCoolTime) / maxChangePropertyCoolTime;
                }
                nowChangePropertyCoolTime = value;
            }
        }
    }

    private float maxPropertyTimeLimit = 25; //최대 속성 지속시간

    public float nowPropertyTimeLimit; // 현재 속성 남은 지속시간

    public float NowPropertyTimeLimit
    {
        get { return nowPropertyTimeLimit; }
        set
        {
            if (value > maxPropertyTimeLimit && isResurrectionReady == false && nowState != NowState.Resurrection)
            {
                nowPropertyTimeLimit = maxPropertyTimeLimit;
                isChangeProperty = true;
                StartCoroutine(ChangeProperty(true, false));
            }
            else
            {
                nowPropertyTimeLimit = value;
                if (isChangePropertyReady || nowProperty != NowPlayerProperty.BasicProperty)
                {
                    nowPropertyLimitTimeImage.fillAmount = (maxPropertyTimeLimit - NowPropertyTimeLimit) / maxPropertyTimeLimit;
                }
            }
        }
    }

    private int nextPropertyIndex; //다음 바뀔 속성의 인덱스

    private bool isChangePropertyReady; //속성 변경 준비 판별

    private IEnumerator propertyTimeCount; //속성 지속시간 세는 코루틴

    [HideInInspector]
    public NowPlayerProperty nowProperty; //현재 속성 상태
    #endregion

    private bool isResurrectionOpportunityExists; //부활 조건

    private bool angelPropertyBuffing;

    private bool isToBurn; 

    private bool isGetGood; //재화 획득 여부 판별

    private bool isChangeProperty;

    private float maxNaturePassiveCount; //자연 속성 최대 구슬 개수

    private float nowNaturePassiveCount; //자연 속성 현재 구슬 개수
    public float NowNaturePassiveCount
    {
        get { return nowNaturePassiveCount; }
        set
        {
            if (value >= maxNaturePassiveCount)
            {
                isSpawnNatureBead = true;
                objectPoolInstance.GetObject((int)PoolObjKind.PlayerHpRecoveryBead);
            }
            else
            {
                nowNaturePassiveCount = value;
            }
        }
    }

    [HideInInspector]
    public bool isSpawnNatureBead;

    [SerializeField]
    [Tooltip("현재 플레이어 속성 아이콘")]
    private Image nowPropertyImage;

    [SerializeField]
    [Tooltip("현재 플레이어 속성 지속시간 이미지")]
    private Image nowPropertyLimitTimeImage;

    [SerializeField]
    [Tooltip("플레이어 속성 아이콘 스프라이트 모음")]
    private Sprite[] nowPropertyIconImages;

    [SerializeField]
    [Tooltip("상태에 따른 체력바 스프라이트 모음")]
    private Sprite[] nowStateHpUi;

    [SerializeField]
    [Tooltip("상태에 따른 체력바 배경 스프라이트 모음")]
    private Sprite[] nowStatHpUiBg;

    #region 체력 텍스트 색 값들
    [Header("체력 텍스트 색 값들")]
    [SerializeField]
    [Tooltip("기본 체력 텍스트 색")]
    private Color basicHpTextColor;

    [SerializeField]
    [Tooltip("성령 속성 보호막 체력 텍스트 색")]
    private Color shieldHpTextColor;
    #endregion

    #region 속성 상징 색
    [Header("속성 상징 색 모음")]
    [SerializeField]
    [Tooltip("속성 상징 색 모음")]
    private Color[] propertyColors;
    #endregion

    #region 스탯 원본 수치 (버프/디버프에 사용)
    private float originalDamage; //현재 플레이어 기본 데미지 수치

    private float originalMaxActionCoolTime; //현재 플레이어 기본 행동 쿨타임 수치

    private float originalRestWaitTime; //현재 플레이어 기본 휴식 시간 수치

    private float originalSpeed; //현재 플레이어 기본 이동속도 수치
    #endregion

    private void Update()
    {
        Deflect();
        Defense();
        Jump();
    }

    /// <summary>
    /// 초기 세팅
    /// </summary>
    protected override void StartSetting()
    {
        var gameManager_Ins = GameManager.Instance;
        int energyPerLevel = gameManager_Ins.statLevels[(int)UpgradeableStatKind.Energy] * 3; //레벨당 기력 증가식 (최대 30 증가)
        int maxHpPerLevel = (int)MaxHp / 10 * (gameManager_Ins.statLevels[(int)UpgradeableStatKind.Hp]); //레벨당 체력 증가식 (최대 100% 증가)
        float damagePerLevel = (Damage * 10 / 100) * gameManager_Ins.statLevels[(int)UpgradeableStatKind.Damage]; //레벨당 공격력 증가식 (최대 100% 증가)
        float maxActionCoolTimePerLevel = (gameManager_Ins.ReduceCoolTimeLevel * 0.1f); //레벨당 최대 쿨타임 차감식 (임시)

        maxActionCoolTime -= maxActionCoolTimePerLevel;
        MaxHp += maxHpPerLevel;
        MaxEnergy += energyPerLevel;
        Damage += damagePerLevel;

        restWaitTime = 1.25f;
        maxNaturePassiveCount = 5;

        nowState = NowState.Standingby;
        nowProperty = NowPlayerProperty.BasicProperty;
        isResurrectionOpportunityExists = true;

        bsm.playerCharacterPos = transform.position;
        nextPropertyIndex = Random.Range((int)NowPlayerProperty.NatureProperty, (int)NowPlayerProperty.PropertyTotalNumber);
        nowPropertyImage.sprite = nowPropertyIconImages[(int)nowProperty];

        Energy = MaxEnergy;
        Hp = MaxHp;

        originalDamage = Damage;
        originalMaxActionCoolTime = maxActionCoolTime;
        originalRestWaitTime = restWaitTime;
        originalSpeed = Speed;

        propertyTimeCount = CountDownPropertyTimes();
        StartCoroutine(propertyTimeCount);
    }

    public override void Hit(float damage, bool isDefending)
    {
        if (IsInvincibility == false)
        {
            if (ShieldHp_F > 0 && !isDefending)
            {
                ShieldHp_F -= 1;
            }
            else if (isDefending)
            {
                Energy -= 1;
                DreamyFigure += 1;
                animator.SetTrigger("DefenceIntermediateMotion");
            }
            else
            {
                spriteRenderer.color = hitColor;
                Hp -= nowProperty == NowPlayerProperty.ForceProperty ? damage * 2f : damage;
                DreamyFigure += 2;
                StartCoroutine(ChangeToBasicColor());
            }
        }
    }

    protected override void Defense()
    {
        if (bsm.nowGameState == NowGameState.Playing && nowState == NowState.Standingby && Hp > 0 && isChangePropertyReady == false)
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

                animator.SetBool(nowDefenceAnimName, false);
                ReleaseDefense();
            }
        }
    }

    protected override void SetDefensing(DefensePos nowDefensePos, float setRotation)
    {
        string nowDefenceAnimName = (nowDefensePos == DefensePos.Up) ? "Defence(Top)" : "Defence(Left&Right)";

        animator.SetBool(nowDefenceAnimName, true);
        nowState = NowState.Defensing;
        nowDefensivePosition = nowDefensePos;
        transform.rotation = Quaternion.Euler(0, setRotation, 0);
    }

    /// <summary>
    /// 속성 변경하는 구간 (무적시간, 속성 변경 애니메이션 실행)
    /// </summary>
    /// <param name="isChangeBasicProperty"> 기본 속성으로 변경 유무 </param>
    /// <param name="isForcedChange"> 속성 강제 변경 유무 </param>
    /// <returns></returns>
    IEnumerator ChangeProperty(bool isChangeBasicProperty, bool isForcedChange)
    {
        isChangePropertyReady = true;

        NowChangePropertyCoolTime = 0;

        if (isForcedChange)
        {
            NowPropertyTimeLimit = 0;
        }

        while (true)
        {
            if (nowState == NowState.Standingby && angelPropertyBuffing == false)
            {
                break;
            }
            yield return null;
        }

        if (isForcedChange == false)
        {
            NowPropertyTimeLimit = 0;
        }

        nowState = NowState.ChangingProperties;
        Invincibility(true);

        battleButtonManagerInstance.ActionButtonsSetActive(false, false, false);
        transform.rotation = Quaternion.identity;
        nowActionCoolTime = 0;

        if (isChangeBasicProperty)
        {
            nowProperty = NowPlayerProperty.BasicProperty;
            if(nowProperty == NowPlayerProperty.TheHolySpiritProperty)
            {
                hpText.text = $"{(Hp):N0}/{(MaxHp):N0}";
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
            StartCoroutine(PropertyPassiveAbilityStart());
            nextPropertyIndex = ((NowPlayerProperty)nextPropertyIndex == NowPlayerProperty.AngelProperty) ? (int)NowPlayerProperty.NatureProperty : nextPropertyIndex + 1;
        }

        StartCoroutine(EndingPropertyChanges());
        nowPropertyLimitTimeImage.color = propertyColors[(int)nowProperty];
        nowPropertyImage.sprite = nowPropertyIconImages[(int)nowProperty];
    }

    IEnumerator EndingPropertyChanges() //나중에 애니메이션 나오면 일반함수로 전환, 그리고 속성 변경 애니메이션 끝날때쯤 변경한 이 함수 실행
    {
        yield return new WaitForSeconds(2);
        isChangePropertyReady = false;
        nowState = NowState.Standingby;

        Invincibility(false);

        if (battleButtonManagerInstance.nowButtonPage == ButtonPage.FirstPage)
        {
            battleButtonManagerInstance.ActionButtonsSetActive(true, false, true);
        }
        else
        {
            battleButtonManagerInstance.ActionButtonsSetActive(false, true, false);
        }

        propertyTimeCount = CountDownPropertyTimes();
        StartCoroutine(propertyTimeCount); 
    }

    public void PropertyChangeStart()
    {
        if (nowState == NowState.Standingby && DreamyFigure >= 10)
        {
            DreamyFigure -= 10;
            StopCoroutine(propertyTimeCount); //실행중인 속성 지속시간 세는 코루틴 중지 (중복 실행 방지)
            StartCoroutine(ChangeProperty(false, true));
        }
    }

    IEnumerator CountDownPropertyTimes()
    {
        while (true)
        {
            if (bsm.nowGameState == NowGameState.Playing && isChangePropertyReady == false && isResurrectionReady == false)
            {
                if (nowProperty != NowPlayerProperty.BasicProperty)
                {
                    NowPropertyTimeLimit += Time.deltaTime;
                }
                else
                {
                    NowChangePropertyCoolTime += Time.deltaTime;
                }

                if (isChangeProperty)
                {
                    isChangeProperty = false;
                    break;
                }
            }
            yield return null;
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
        battleButtonManagerInstance.ActionButtonsSetActive(false, false, false);
        transform.rotation = Quaternion.Euler(0, setRotation, 0);
        ChangeAttackRange(new Vector2(0.7f, 2.6f), new Vector2(0, 0));

        animator.SetBool("Paring", true);
        animator.SetBool("Defence(Left&Right)", false);

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
        
        animator.SetBool("Paring", false);
        
        InitializationAttackRange();

        if (nowState != NowState.Defensing)
        {
            nowState = NowState.Standingby;
        }

        if (isWaiting == false && isChangePropertyReady == false)
        {
            battleButtonManagerInstance.ActionButtonsSetActive(true, false, false);
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


    protected override IEnumerator UISetting() //대기시간 및 UI세팅
    {
        while (true)
        {
            if (isWaiting && (nowState == NowState.Standingby || nowState == NowState.Jumping || nowState == NowState.Defensing))
            {
                actionCoolTimeObj.transform.position = transform.position + (Vector3)actionCoolTimeObjPlusPos;
                actionCoolTimeImage.fillAmount = nowActionCoolTime / maxActionCoolTime;
                nowActionCoolTime += Time.deltaTime;

                if (nowActionCoolTime >= maxActionCoolTime || isChangePropertyReady)
                {
                    WaitingTimeEnd();
                    ActionCoolTimeBarSetActive(false);

                    if (isChangePropertyReady == false)
                    {
                        battleButtonManagerInstance.ActionButtonsSetActive(true, false, false);
                    }
                    break;
                }
            }
            else if (nowState == NowState.Deflecting || nowState == NowState.Fainting || nowState == NowState.ChangingProperties)
            {
                ActionCoolTimeBarSetActive(false);
            }

            yield return null;
        }
    }

    private void WaitingTimeStart() //공격 후의 세팅 (일부 공통, 한번만 실행) 
    {
        nowState = NowState.Standingby;

        if (isChangePropertyReady == false && Hp > 0)
        {
            isWaiting = true;
            
            if (nowActionCoolTime < maxActionCoolTime)
            {
                ActionCoolTimeBarSetActive(true);
                battleButtonManagerInstance.ActionButtonsSetActive(false, false, false);
            }

            StartCoroutine(UISetting());
        }
    }

    private void Jump()
    {
        if (bsm.nowGameState == NowGameState.Playing && nowState == NowState.Standingby && Input.GetKey(KeyCode.Space) && Hp > 0 && isChangePropertyReady == false)
        {
            nowState = NowState.Jumping;
            battleButtonManagerInstance.ActionButtonsSetActive(false, false, false);

            CamShake.JumpStart();

            rigid.AddForce(Vector2.up * jumpPower_F, ForceMode2D.Impulse);
            rigid.gravityScale = setJumpGravityScale_F - 0.5f;
            animator.SetTrigger("Jumping");
            StartCoroutine(JumpDelay());
        }
        else if (nowState == NowState.Jumping && transform.position.y < startPos_Vector.y)
        {
            nowState = NowState.Standingby;
            if (isWaiting == false && isChangePropertyReady == false)
            {
                if (battleButtonManagerInstance.nowButtonPage == ButtonPage.SecondPage)
                {
                    battleButtonManagerInstance.ActionButtonsSetActive(false, true, false);
                }
                else
                {
                    battleButtonManagerInstance.ActionButtonsSetActive(true, false, false);
                }
            }

            animator.SetBool("JumpIntermediateMotion", false);
            CamShake.JumpStop(false);
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

        animator.SetBool("JumpIntermediateMotion", true);
        rigid.gravityScale = setJumpGravityScale_F * 1.5f;
    }

    public void CloseAttackStart()
    {
        if (nowState == NowState.Standingby)
        {
            nowState = NowState.Attacking;
            battleButtonManagerInstance.ActionButtonsSetActive(false, false, false);
            StartCoroutine(GoToAttack());
        }
    }

    public void RestStart()
    {
        if (nowState == NowState.Standingby)
        {
            nowState = NowState.Resting;
            battleButtonManagerInstance.ActionButtonsSetActive(false, false, false);
            StartCoroutine(Resting());
        }
    }

    protected override IEnumerator Resting()
    {
        int nowRestingCount = 0;
        WaitForSeconds RestWaitTime = new WaitForSeconds(restWaitTime);

        battleUIObjScript.BattleUIObjSetActiveTrue(ChangeBattleUIAnim.Rest);
        battleUIAnimator.SetBool("NowResting", true);

        while (3 > nowRestingCount)
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

        battleUIObjScript.BattleUIObjSetActiveFalse();
        battleUIAnimator.SetBool("NowResting", false);

        nowState = NowState.Standingby;

        if (isChangePropertyReady == false)
        {
            battleButtonManagerInstance.ActionButtonsSetActive(true, false, false);
        }
    }

    IEnumerator GoToAttack()
    {
        Vector3 Movetransform = new Vector3(Speed, 0, 0); //이동을 위해 더해줄 연산
        Vector3 Targettransform = new Vector3(bsm.enemyCharacterPos.x - 5.5f, transform.position.y); //목표 위치

        animator.SetBool("Moving", true);

        while (transform.position.x < Targettransform.x) //이동중
        {
            transform.position += Movetransform * Time.deltaTime;
            yield return null;
        }
        transform.position = Targettransform; //이동 완료

        animator.SetBool("Moving", false);
        StartCoroutine(Attacking(false, nowAttackCount_I, 0.2f, 0.2f)); //첫번째 공격 실행
        animator.SetTrigger("BasicAttack");
    }

    /// <summary>
    /// 기본 공격 실행
    /// </summary>
    /// <param name="isLastAttack"> 연속 공격의 마지막(3회차) 공격인지 판별 </param>
    /// <param name="nowAttackCount"> 현재 공격 회차 </param>
    /// <param name="maxDelayTime"> 연타 방지용 시간 (기본공격 애니메이션 시작 및 타격 지점까지 딜레이) </param>
    /// <param name="maxLinkedAttacksLimitTime"> 히트 액션(연속 공격) 성공 시간 </param>
    /// <returns></returns>
    IEnumerator Attacking(bool isLastAttack, int nowAttackCount, float maxDelayTime, float maxLinkedAttacksLimitTime)
    {
        bool isComplete = false;
        bool isFail = false;

        float nowdelayTime = 0;
        float nowattacktime = 0;

        int maxEnemyIndex;

        while (nowdelayTime < maxDelayTime)
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
            switch (nowAttackCount)
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

                    unitScriptComponenet.Hit(CurrentRandomDamage(Damage), isDefence);

                    if (isGetGood == false)
                    {
                        GetBasicGood();
                        isGetGood = true;
                    }

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

        while (maxLinkedAttacksLimitTime > nowattacktime) //연공 타이밍 계산
        {
            nowattacktime += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isComplete = true;
            }
            yield return null;
        }

        if (isLastAttack == false && isFail == false && isComplete)
        {
            nowAttackCount++;
            switch (nowAttackCount) //공격 실행 애니메이션 시작
            {
                case 2:
                    StartCoroutine(Attacking(false, nowAttackCount, 0.2f, 0.25f));
                    animator.SetTrigger("BasicSecondAttackHitActionCompleat");
                    break;
                case 3:
                    StartCoroutine(Attacking(true, nowAttackCount, 0.35f, 0));
                    animator.SetTrigger("BasicThirdAttackHitActionCompleat");
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

            isGetGood = false;
            StartCoroutine(Return());
        }
    }

    private float CurrentRandomDamage(float nowPlayerDamage)
    {
        int randDamage = Random.Range(-2, 2); //랜덤 데미지 증감

        nowPlayerDamage += randDamage;

        return nowPlayerDamage;
    }

    IEnumerator Return() //근접공격 후 돌아오기
    {
        Vector3 Movetransform = new Vector3(Speed, 0, 0);
        transform.rotation = Quaternion.Euler(0, 180, 0);

        animator.SetBool("Moving", true);
        while (transform.position.x > startPos_Vector.x)
        {
            transform.position -= Movetransform * Time.deltaTime;
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

    public void SkillUse(int nowUseSkillIndex, int nowUseSkillNeedEnergy)
    {
        if (nowState == NowState.Standingby && Energy >= nowUseSkillNeedEnergy)
        {
            nowState = NowState.Attacking;
            Energy -= nowUseSkillNeedEnergy;

            battleButtonManagerInstance.ActionButtonsSetActive(false, false, false);

            switch (nowUseSkillIndex)
            {
                case 1:
                    StartCoroutine(SwordAuraSkill());
                    animator.SetBool("FirstSkill", true);
                    break;
            }
        }
    }

    private IEnumerator SwordAuraSkill()
    {
        float nowDelayTime = 0;
        float maxDelayTime = 0.6f;
        bool isFailEnchant = true;

        while (nowDelayTime < maxDelayTime)
        {
            if (Input.GetKeyDown(KeyCode.Space) && nowDelayTime > 0.4f && nowDelayTime < 0.55f)
            {
                isFailEnchant = false;
            }
            nowDelayTime += Time.deltaTime;
            yield return null;
        }

        var enchantedSwordAuraObj = objectPoolInstance.GetObject((int)PoolObjKind.PlayerSwordAura);
        var enchantedSwordAuraObjComponent = enchantedSwordAuraObj.GetComponent<SwordAura>();

        if (isFailEnchant == false)
        {
            enchantedSwordAuraObjComponent.IsEnchanted = true;
        }

        yield return new WaitForSeconds(0.3f);

        animator.SetBool("FirstSkill", false);

        if (Energy > 0)
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
            StartCoroutine(Resurrection());
            yield return null;
        }
        else
        {
            nowState = NowState.Dead;
            bsm.StartGameEndPanelAnim(true);
        }
    }

    private IEnumerator Resurrection()
    {
        int recoveryFixedValue = 20;
        int ResurrectionStatsValueSharingValue = 5;

        isResurrectionReady = true;

        while (nowState != NowState.Standingby)
        {
            yield return null;
        }

        Invincibility(true);
        AngelPropertyBuff(true);
        ActionCoolTimeBarSetActive(false);

        battleButtonManagerInstance.ActionButtonsSetActive(false, false, false);

        while (true)
        {
            Hp += Time.deltaTime * (MaxHp / recoveryFixedValue);
            Energy += (Energy <= maxEnergy / ResurrectionStatsValueSharingValue) ? Time.deltaTime * (maxEnergy / recoveryFixedValue) : 0;

            if (Hp >= MaxHp / ResurrectionStatsValueSharingValue && Energy >= maxEnergy / ResurrectionStatsValueSharingValue)
            {
                Hp = MaxHp / ResurrectionStatsValueSharingValue;
                if (Energy <= maxEnergy / ResurrectionStatsValueSharingValue)
                {
                    Energy = maxEnergy / ResurrectionStatsValueSharingValue;
                }
                break;
            }
            yield return null;
        }

        NowPropertyTimeLimit = 10;

        isResurrectionReady = false;
        nowState = NowState.Resurrection;

        nowActionCoolTime = maxActionCoolTime;
        WaitingTimeStart();

        battleButtonManagerInstance.ActionButtonsSetActive(true, false, false);

        while (maxPropertyTimeLimit > NowPropertyTimeLimit)
        {
            yield return null;
        }

        AngelPropertyBuff(false);
        Invincibility(false);
    }

    private void AngelPropertyBuff(bool isBuffing)
    {
        angelPropertyBuffing = isBuffing;

        Damage = isBuffing ? Damage * 2 : originalDamage;
        maxActionCoolTime = isBuffing ? maxActionCoolTime - 1 : originalMaxActionCoolTime;
    }

    private void TheHolySpiritPropertyBuff(bool isBuffing)
    {
        maxActionCoolTime = isBuffing ? maxActionCoolTime - (maxActionCoolTime / 4) : originalMaxActionCoolTime;
        restWaitTime = isBuffing ? restWaitTime - (originalRestWaitTime / 5) : originalRestWaitTime;
        Damage = isBuffing ? Damage * 1.5f : originalDamage;
    }

    private void TheHolySpiritPropertyDeBuff(bool isDeBuffing)
    {
        maxActionCoolTime = isDeBuffing ? maxActionCoolTime + (maxActionCoolTime / 4) : originalMaxActionCoolTime;
        restWaitTime = isDeBuffing ? restWaitTime + (originalRestWaitTime / 5) : originalRestWaitTime;
        Damage = isDeBuffing ? Damage / 1.5f : originalDamage;
        Speed = isDeBuffing ? Speed / 1.25f : originalSpeed;
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

        if (nowDefensivePosition == DefensePos.Left || nowDefensivePosition == DefensePos.Right)
        {
            animator.SetBool("Defence(Left&Right)", false);
        }
        else if (nowDefensivePosition == DefensePos.Up)
        {
            animator.SetBool("Defence(Top)", false);
        }

        animator.SetBool("Stuning", true);
        nowDefensivePosition = DefensePos.None;
        battleButtonManagerInstance.ActionButtonsSetActive(false, false, false);

        yield return new WaitForSeconds(0.2f);

        battleUIObjScript.BattleUIObjSetActiveTrue(ChangeBattleUIAnim.Faint);
        battleUIAnimator.SetBool("NowFainting", true);

        yield return new WaitForSeconds(5); //나중에 매개변수로 레벨에 따라서 기절 시간 넣기

        battleUIObjScript.BattleUIObjSetActiveFalse();
        animator.SetBool("Stuning", false);
        battleUIAnimator.SetBool("NowFainting", false);

        Energy += 8; //나중에 매개변수로 레벨에 따라서 기력 차는 양 증가
        nowActionCoolTime = maxActionCoolTime;

        if (isChangePropertyReady == false && Hp > 0)
        {
            battleButtonManagerInstance.ActionButtonsSetActive(true, false, false);
        }

        WaitingTimeStart();
    }

    protected override void InvincibilityEvent(bool isInvincibilityTrue)
    {
        unitHpBarBg.sprite = (isInvincibilityTrue) ? nowStatHpUiBg[(int)NowStatUIState.Invincibility] : nowStatHpUiBg[(int)NowStatUIState.Basic];
        unitHpBar.sprite = (isInvincibilityTrue) ? nowStateHpUi[(int)NowStatUIState.Invincibility] : nowStateHpUi[(int)NowStatUIState.Basic];
    }

    private IEnumerator PropertyPassiveAbilityStart()
    {
        NowChangePropertyCoolTime = 0;

        while (nowState != NowState.ChangingProperties)
        {
            yield return null;
        }

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
                float enhancedDamage = Damage / 2f;
                float reducedMaxActionCoolTime = maxActionCoolTime / 5;

                maxActionCoolTime -= reducedMaxActionCoolTime;
                Damage += enhancedDamage;
                while (nowProperty == NowPlayerProperty.ForceProperty)
                {
                    yield return null;
                }
                Damage -= enhancedDamage;
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

                shieldHp_F = 0;
                unitShieldHpBars.fillAmount = ShieldHp_F / maxShieldHp_F;
                hpText.color = basicHpTextColor;
                hpText.text = $"{(Hp):N0}/{(MaxHp):N0}";
                break;
        }
    }
}