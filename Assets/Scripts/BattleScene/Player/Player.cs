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
    Invincibility,
    Shield
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
                    hpText.color = hpTextColors[(int)NowStatUIState.Basic];
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

    private float nowChangePropertyCoolTime; //현재 속성 변경 시간

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

    private float nowPropertyTimeLimit; // 현재 속성 남은 지속시간

    public float NowPropertyTimeLimit
    {
        get { return nowPropertyTimeLimit; }
        set
        {
            if (value > maxPropertyTimeLimit && isResurrectionReady == false && nowState != NowState.Resurrection && nowState != NowState.Attacking && nowState != NowState.Jumping)
            {
                nowPropertyTimeLimit = maxPropertyTimeLimit;

                if (isResurrectionReady == false)
                {
                    isChangeProperty = true;
                    StartCoroutine(ChangeProperty(true, false));
                }
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

    [SerializeField]
    [Tooltip("히트 액션 타이밍 표시 오브젝트")]
    private HitActionTiming timingObj;

    #region 체력 텍스트 색 값들
    [Header("체력 텍스트 색 값들")]
    [SerializeField]
    [Tooltip("체력 텍스트 색 모음")]
    private Color[] hpTextColors;
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
    /// 초기 세팅 함수
    /// </summary>
    protected override void StartSetting()
    {
        var gameManagerIns = GameManager.Instance;
        int energyPerLevel = gameManagerIns.statLevels[(int)UpgradeableStatKind.Energy] * 3; //레벨당 기력 증가식 (최대 30 증가)
        int maxHpPerLevel = (int)MaxHp / 10 * (gameManagerIns.statLevels[(int)UpgradeableStatKind.Hp]); //레벨당 체력 증가식 (최대 100% 증가)
        float damagePerLevel = (Damage * 10 / 100) * gameManagerIns.statLevels[(int)UpgradeableStatKind.Damage]; //레벨당 공격력 증가식 (최대 100% 증가)
        float maxActionCoolTimePerLevel = (gameManagerIns.ReduceCoolTimeLevel * 0.1f); //레벨당 최대 쿨타임 차감식 (임시)

        InitializationAttackRangeSize = new Vector2(1.1f, 2.68f);
        InitializationAttackRangeOffset = new Vector2(0.18f, -0.08f);

        maxActionCoolTime -= maxActionCoolTimePerLevel;
        MaxHp += maxHpPerLevel;
        MaxEnergy += energyPerLevel;
        Damage += damagePerLevel;

        restWaitTime = 1.25f;
        maxNaturePassiveCount = 5;

        isResurrectionOpportunityExists = true;

        nextPropertyIndex = (int)NowPlayerProperty.FlameProperty;//Random.Range((int)NowPlayerProperty.NatureProperty, (int)NowPlayerProperty.PropertyTotalNumber);
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

    /// <summary>
    /// 게임 클리어 시 세팅 함수
    /// </summary>
    public void GameClearSetting()
    {
        StopAllCoroutines();
        
        CamShake.JumpStop(true);

        animator.speed = 0;

        rigid.gravityScale = 0;
        rigid.velocity = Vector2.zero;

        if (nowState == NowState.Fainting || nowState == NowState.Resting)
        {
            battleUIObjScript.BattleUIObjSetActiveFalse();
        }
        else if (isWaiting)
        {
            ActionCoolTimeBarSetActive(false);
        }
    }

    /// <summary>
    /// 공격에 맞았을 때
    /// </summary>
    /// <param name="damage"> 받을 데미지 </param>
    /// <param name="isDefending"> 방어중인지 판별 </param>
    public override void Hit(float damage, bool isDefending)
    {
        var damageText = objectPoolInstance.GetObject((int)PoolObjKind.DamageText); //데미지 텍스트 소환(오브젝트 풀)
        TextState nowTextState = TextState.Blocking; //현재 데미지 텍스트 상태

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
                spriteRenderer.color = stateColors[(int)StateColor.HitColor]; //맞았을 때의 효과 : 색 변경

                if (nowProperty == NowPlayerProperty.ForceProperty)
                {
                    damage *= 2;
                }
                nowTextState = TextState.BasicDamage; //나중에 치명타 추가되면 치명타 조건 구분해서 넣기

                Hp -= damage;

                DreamyFigure += 2;
                StartCoroutine(ChangeToBasicColor());
            }
        }

        damageText.GetComponent<DamageText>().TextCustom(nowTextState, transform.position + plusVector, damage);
    }

    /// <summary>
    /// 방어 실행
    /// </summary>
    protected override void Defense()
    {
        if (bsm.nowGameState == NowGameState.Playing)
        {
            if (nowState == NowState.Standingby && Hp > 0 && isChangePropertyReady == false)
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
    }

    /// <summary>
    /// 현재 방어 설정
    /// </summary>
    /// <param name="nowDefensePos"> 현재 방어 위치 상태 </param>
    /// <param name="setRotation"> 현재 방어 캐릭터 로테이션 값 </param>
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

        WaitingTimeEnd();
        ActionCoolTimeBarSetActive(false);

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
        hpText.color = hpTextColors[(int)NowStatUIState.Invincibility];

        battleButtonManagerInstance.ActionButtonSetActive(false);
        transform.rotation = Quaternion.identity;

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

    /// <summary>
    /// 속성 변경 마무리 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator EndingPropertyChanges() //나중에 애니메이션 나오면 일반함수로 전환, 그리고 속성 변경 애니메이션 끝날때쯤 변경한 이 함수 실행
    {
        yield return new WaitForSeconds(2);

        isChangePropertyReady = false;
        nowState = NowState.Standingby;

        Invincibility(false);

        if (nowProperty == NowPlayerProperty.TheHolySpiritProperty)
        {
            hpText.color = hpTextColors[(int)NowStatUIState.Shield];
        }
        else
        {
            hpText.color = hpTextColors[(int)NowStatUIState.Basic];
        }

        battleButtonManagerInstance.ActionButtonSetActive(true);

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

    /// <summary>
    /// 속성 변경에 필요한 카운트 다운
    /// </summary>
    /// <returns></returns>
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
            else if (isResurrectionReady)
            {
                break;
            }

            yield return null;
        }
    }

    /// <summary>
    /// 패링 실행 함수
    /// </summary>
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

    /// <summary>
    /// 패링 실행 함수
    /// </summary>
    /// <param name="setRotation"> 패링 방향 </param>
    /// <returns></returns>
    IEnumerator Deflecting(int setRotation)
    {
        bool isAlreadyShake = false;

        nowState = NowState.Deflecting;
        nowDefensivePosition = DefensePos.None;
        transform.rotation = Quaternion.Euler(0, setRotation, 0);

        battleButtonManagerInstance.ActionButtonSetActive(false);

        ChangeAttackRange(new Vector2(0.85f, 2.68f), new Vector2(0.06f, -0.08f));

        animator.SetBool("Paring", true);
        animator.SetBool("Defence(Left&Right)", false);

        yield return new WaitForSeconds(0.15f);
        
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
        yield return new WaitForSeconds(0.25f);
        
        animator.SetBool("Paring", false);
        
        InitializationAttackRange();

        if (nowState != NowState.Defensing)
        {
            nowState = NowState.Standingby;
        }

        if (isWaiting == false && isChangePropertyReady == false)
        {
            battleButtonManagerInstance.ActionButtonSetActive(true);
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

    /// <summary>
    /// 대기시간 및 UI세팅
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator UISetting() 
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
                        battleButtonManagerInstance.ActionButtonSetActive(true);
                        battleButtonManagerInstance.ButtonsPageChange(true, false);
                    }
                    break;
                }
            }
            else if (nowState == NowState.Deflecting || nowState == NowState.Fainting || nowState == NowState.ChangingProperties)
            {
                ActionCoolTimeBarSetActive(false);
                if (nowState == NowState.Fainting || nowState == NowState.ChangingProperties)
                {
                    break;
                }
            }

            yield return null;
        }
    }
     
    /// <summary>
    /// 공격 후 공격 쿨타임 실행 함수
    /// </summary>
    private void WaitingTimeStart()
    {
        nowState = NowState.Standingby;

        if (isChangePropertyReady == false && Hp > 0)
        {
            isWaiting = true;
            
            if (nowActionCoolTime < maxActionCoolTime)
            {
                ActionCoolTimeBarSetActive(true);
                battleButtonManagerInstance.ActionButtonSetActive(false);
            }

            StartCoroutine(UISetting());
        }
    }

    /// <summary>
    /// 점프 실행 함수
    /// </summary>
    private void Jump()
    {
        if (bsm.nowGameState == NowGameState.Playing && isChangePropertyReady == false && nowState == NowState.Standingby && Input.GetKey(KeyCode.Space) && Hp > 0)
        {
            nowState = NowState.Jumping;

            transform.rotation = Quaternion.Euler(0, 0, 0);

            battleButtonManagerInstance.ActionButtonSetActive(false);

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
                battleButtonManagerInstance.ActionButtonSetActive(true);
            }

            animator.SetBool("JumpIntermediateMotion", false);
            CamShake.JumpStop(false);
            transform.position = startPos_Vector;
            rigid.velocity = Vector2.zero;
            rigid.gravityScale = 0;
        }
    }

    /// <summary>
    /// 점프 후 중력값 조절하는 함수
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 근접 공격(적에게 이동해야하는 공격)실행 함수
    /// </summary>
    public void CloseAttackStart()
    {
        if (nowState == NowState.Standingby)
        {
            nowState = NowState.Attacking;
            battleButtonManagerInstance.ActionButtonSetActive(false);
            StartCoroutine(GoToAttack());
        }
    }
    
    /// <summary>
    /// 휴식 실행 함수
    /// </summary>
    public void RestStart()
    {
        if (nowState == NowState.Standingby)
        {
            nowState = NowState.Resting;
            battleButtonManagerInstance.ActionButtonSetActive(false);
            StartCoroutine(Resting());
        }
    }

    /// <summary>
    /// 휴식 함수
    /// </summary>
    /// <returns></returns>
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
            battleButtonManagerInstance.ActionButtonSetActive(true);
            battleButtonManagerInstance.ButtonsPageChange(true, false);
        }
    }

    /// <summary>
    /// 근접공격시 적 위치까지 이동
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 현재 적에게 줄 랜덤 데미지 뽑는 함수
    /// </summary>
    /// <param name="nowPlayerDamage"> 현재 플레이어 데미지 </param>
    /// <returns></returns>
    private float CurrentRandomDamage(float nowPlayerDamage)
    {
        int randDamage = Random.Range(-2, 2); //랜덤 데미지 증감

        nowPlayerDamage += randDamage;

        return nowPlayerDamage;
    }

    /// <summary>
    /// 근접 공격(적에게 이동해야하는 공격) 후 복귀 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator Return()
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

    /// <summary>
    /// 현재 선택한 스킬 실행 함수
    /// </summary>
    /// <param name="nowUseSkillIndex"> 현재 실행할 스킬 인덱스 </param>
    /// <param name="nowUseSkillNeedEnergy"> 현재 실행할 스킬 필요 기력 </param>
    public void SkillUse(int nowUseSkillIndex, int nowUseSkillNeedEnergy)
    {
        if (nowState == NowState.Standingby && Energy >= nowUseSkillNeedEnergy)
        {
            nowState = NowState.Attacking;
            Energy -= nowUseSkillNeedEnergy;

            battleButtonManagerInstance.ButtonsPageChange(true, false);
            battleButtonManagerInstance.ActionButtonSetActive(false);

            switch (nowUseSkillIndex)
            {
                case 1:
                    StartCoroutine(SwordAuraSkill());
                    animator.SetBool("FirstSkill", true);
                    break;
            }
        }
    }

    /// <summary>
    /// 검기 발사 스킬 함수
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 사망시 실행하는 함수
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator Dead()
    {
        if (nowProperty == NowPlayerProperty.AngelProperty && isResurrectionOpportunityExists) //부활 가능할 때 (가능하며 천사 속성일 때)
        {
            isResurrectionOpportunityExists = false;
            isResurrectionReady = true; //부활 준비 : 참
            StartCoroutine(Resurrection());
            yield return null;
        }
        else //부활이 불가능할 때 (가능 하지만, 천사 속성이 아닐 때 or 부활을 이미 했을 때)
        {
            nowState = NowState.Dead;
            bsm.StartGameEndPanelAnim(true);
        }
    }

    /// <summary>
    /// 부활 실행 함수
    /// </summary>
    /// <returns></returns>
    private IEnumerator Resurrection()
    {
        int recoveryFixedValue = 20;
        int ResurrectionStatsValueSharingValue = 5;

        Invincibility(true); //현재 무적 상태 : 참
        hpText.color = hpTextColors[(int)NowStatUIState.Invincibility];

        while (nowState != NowState.Standingby) //플레이어 상태가 대기 상태일 때까지 대기
        {
            yield return null;
        }

        nowState = NowState.Resurrection;
        AngelPropertyBuff(true);
        ActionCoolTimeBarSetActive(false);

        battleButtonManagerInstance.ActionButtonSetActive(false);

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

        propertyTimeCount = CountDownPropertyTimes();
        StartCoroutine(propertyTimeCount);

        nowActionCoolTime = maxActionCoolTime;
        WaitingTimeStart();

        battleButtonManagerInstance.ActionButtonSetActive(true);
        battleButtonManagerInstance.ButtonsPageChange(true, false);

        while (maxPropertyTimeLimit > NowPropertyTimeLimit)
        {
            yield return null;
        }

        AngelPropertyBuff(false);
        Invincibility(false);
        hpText.color = hpTextColors[(int)NowStatUIState.Basic];
    }

    /// <summary>
    /// 천사 속성 버프 or 버프 해제
    /// </summary>
    /// <param name="isBuffing"> 버프 걸기 판별 (버프를 걸어주는가?) </param>
    private void AngelPropertyBuff(bool isBuffing)
    {
        angelPropertyBuffing = isBuffing;

        Damage = isBuffing ? Damage * 2 : originalDamage;
        maxActionCoolTime = isBuffing ? maxActionCoolTime - 1 : originalMaxActionCoolTime;
    }

    /// <summary>
    /// 성령 속성 버프 or 버프 해제
    /// </summary>
    /// <param name="isBuffing"> 버프 걸기 판별 (버프를 걸어주는가?) </param>
    private void TheHolySpiritPropertyBuff(bool isBuffing)
    {
        maxActionCoolTime = isBuffing ? maxActionCoolTime - (maxActionCoolTime / 4) : originalMaxActionCoolTime;
        restWaitTime = isBuffing ? restWaitTime - (originalRestWaitTime / 5) : originalRestWaitTime;
        Damage = isBuffing ? Damage * 1.5f : originalDamage;
    }

    /// <summary>
    /// 성령 속성 디버프 or 디버프 해제
    /// </summary>
    /// <param name="isDeBuffing"> 디버프 걸기 판별 (디버프를 걸어주는가?) </param>
    private void TheHolySpiritPropertyDeBuff(bool isDeBuffing)
    {
        maxActionCoolTime = isDeBuffing ? maxActionCoolTime + (maxActionCoolTime / 4) : originalMaxActionCoolTime;
        restWaitTime = isDeBuffing ? restWaitTime + (originalRestWaitTime / 5) : originalRestWaitTime;
        Damage = isDeBuffing ? Damage / 1.5f : originalDamage;
        Speed = isDeBuffing ? Speed / 1.25f : originalSpeed;
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
        battleButtonManagerInstance.ActionButtonSetActive(false);

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
            battleButtonManagerInstance.ActionButtonSetActive(true);
            battleButtonManagerInstance.ButtonsPageChange(true, false);
        }

        WaitingTimeStart();
    }

    /// <summary>
    /// 무적 상태일 때 UI 표시 효과
    /// </summary>
    /// <param name="isInvincibilityTrue"> 무적 상태에 돌입하는가? </param>
    protected override void InvincibilityEvent(bool isInvincibilityTrue)
    {
        unitHpBarBg.sprite = (isInvincibilityTrue) ? nowStatHpUiBg[(int)NowStatUIState.Invincibility] : nowStatHpUiBg[(int)NowStatUIState.Basic];
        unitHpBar.sprite = (isInvincibilityTrue) ? nowStateHpUi[(int)NowStatUIState.Invincibility] : nowStateHpUi[(int)NowStatUIState.Basic];
    }

    /// <summary>
    /// 속성 패시브 효과 실행 함수
    /// </summary>
    /// <returns></returns>
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
                hpText.color = hpTextColors[(int)NowStatUIState.Invincibility];
                hpText.text = $"{(Hp):N0}/{(MaxHp):N0}";
                break;
        }
    }
}