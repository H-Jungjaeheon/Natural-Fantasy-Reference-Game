using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPlayer : BasicUnitScript, IDefense
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

    public float NowChangePropertyCoolTime //현재 속성 변경 시간 프로퍼티
    {
        get
        {
            return nowChangePropertyCoolTime;
        }
        protected set
        {
            if (value > maxChangePropertyCoolTime)
            {
                StartCoroutine(ChangeProperty(false));
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

    public float NowPropertyTimeLimit //현재 속성 남은 지속시간 프로퍼티
    {
        get
        {
            return nowPropertyTimeLimit;
        }
        protected set
        {
            if (value > maxPropertyTimeLimit && isResurrectionReady == false)
            {
                nowPropertyTimeLimit = maxPropertyTimeLimit;

                StartCoroutine(ChangeProperty(true));
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

    private bool isResurrectionOpportunityExists; //부활 조건 판별(부활 전적 유무)

    private bool angelPropertyBuffing; //천사 속성 버프 지속중 판별

    private bool isToBurn; //화상 효과 입혔는지 판별(기본공격)

    private float maxNaturePassiveCount; //회복구슬 생성 쿨타임

    private float nowNaturePassiveCount; //현재 회복구슬 생성 쿨타임

    public float NowNaturePassiveCount //현재 회복구슬 생성 쿨타임 프로퍼티
    {
        get { return nowNaturePassiveCount; }
        set
        {
            if (value >= maxNaturePassiveCount)
            {
                objectPoolInstance.GetObject((int)PoolObjKind.PlayerHpRecoveryBead);
                nowNaturePassiveCount = 0;
            }
            else
            {
                nowNaturePassiveCount = value;
            }
        }
    }

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
    [Tooltip("히트 액션 타이밍 표시 오브젝트")]
    private HitActionTiming timingObj;

    #region 속성 상징 색
    [Header("속성 상징 색 모음")]
    [SerializeField]
    [Tooltip("속성 상징 색 모음")]
    private Color[] propertyColors;
    #endregion

    #region 플레이어 애니메이션 이름 모음
    protected const string BASIC_THIRDATTACK_HITACTION_COMPLEAT = "BasicThirdAttackHitActionCompleat";

    protected const string BASIC_SECONDATTACK_HITACTION_COMPLEAT = "BasicSecondAttackHitActionCompleat";

    protected const string JUMP_INTERMEDIATE_MOTION = "JumpIntermediateMotion";

    protected const string PARING = "Paring";

    protected const string DEFENCE_INTERMEDIATE_MOTION = "DefenceIntermediateMotion";

    protected const string DEFENCE_T = "Defence(Top)";
    #endregion

    void Update()
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
        attackRangeSize = new Vector2(1.1f, 2.68f);
        attackRangeOffset = new Vector2(0.18f, -0.08f);

        restWaitTime = 1.25f;
        maxNaturePassiveCount = 5;

        isResurrectionOpportunityExists = true;

        nextPropertyIndex = Random.Range((int)NowPlayerProperty.NatureProperty, (int)NowPlayerProperty.PropertyTotalNumber);
        nowPropertyImage.sprite = nowPropertyIconImages[(int)nowProperty];

        Energy = MaxEnergy;
        Hp = MaxHp;

        originalDamage = Damage;
        originalMaxActionCoolTime = maxActionCoolTime;
        originalRestWaitTime = restWaitTime;
        originalSpeed = Speed;

        plusVector = new Vector3(0f, 0f, 0f);

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
    /// 방어 해제 시 실행 함수
    /// </summary>
    public void ReleaseDefense()
    {
        nowDefensivePos = DefensePos.None;
        nowState = NowState.Standingby;
    }

    /// <summary>
    /// 공격에 맞았을 때
    /// </summary>
    /// <param name="damage"> 받을 데미지 </param>
    /// <param name="isDefending"> 방어중인지 판별 </param>
    public override void Hit(float damage, bool isDefending, EffectType effectType)
    {
        if (Hp > 0)
        {
            var damageText = objectPoolInstance.GetObject((int)PoolObjKind.DamageText); //데미지 텍스트 소환(오브젝트 풀)
            TextState nowTextState = TextState.Blocking; //현재 데미지 텍스트 상태

            var hitEffect = objectPoolInstance.GetObject((int)PoolObjKind.HitEffects); //타격 이펙트 소환(오브젝트 풀)
            HitEffect nowHitEffect = hitEffect.GetComponent<HitEffect>();

            nowHitEffect.effectType = effectType;

            if (IsInvincibility == false)
            {
                if (ShieldHp_F > 0 && !isDefending)
                {
                    nowHitEffect.effectType = EffectType.Defense;

                    ShieldHp_F -= 1;
                }
                else if (isDefending)
                {
                    nowHitEffect.effectType = EffectType.Defense;

                    Energy -= 1;
                    DreamyFigure += 1;
                    animator.SetTrigger(DEFENCE_INTERMEDIATE_MOTION);
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

            hitEffect.transform.position = transform.position + particlePos;

            damageText.GetComponent<DamageText>().TextCustom(nowTextState, transform.position + plusVector, damage);
        }
    }

    /// <summary>
    /// 방어 실행
    /// </summary>
    public void Defense()
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

                if (nowDefensivePos != DefensePos.Left)
                {
                    transform.rotation = Quaternion.identity;
                }
            }
            else if (nowState == NowState.Defensing)
            {
                if (nowDefensivePos == DefensePos.Left && !Input.GetKey(KeyCode.A) || nowDefensivePos == DefensePos.Right && !Input.GetKey(KeyCode.D)
                    || nowDefensivePos == DefensePos.Up && !Input.GetKey(KeyCode.W))
                {
                    string nowDefenceAnimName = (nowDefensivePos == DefensePos.Up) ? DEFENCE_T : DEFENCE_LR;

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
    public void SetDefensing(DefensePos nowDefensePos, float setRotation)
    {
        string nowDefenceAnimName = (nowDefensePos == DefensePos.Up) ? DEFENCE_T : DEFENCE_LR;

        animator.SetBool(nowDefenceAnimName, true);
        nowState = NowState.Defensing;
        nowDefensivePos = nowDefensePos;
        transform.rotation = Quaternion.Euler(0, setRotation, 0);
    }

    /// <summary>
    /// 속성 변경하는 구간 (무적시간, 속성 변경 애니메이션 실행)
    /// </summary>
    /// <param name="isChangeBasicProperty"> 기본 속성으로 변경 유무 </param>
    /// <returns></returns>
    IEnumerator ChangeProperty(bool isChangeBasicProperty)
    {
        isChangePropertyReady = true;

        while (true)
        {
            if (nowState == NowState.Standingby && angelPropertyBuffing == false)
            {
                break;
            }
            yield return null;
        }

        nowState = NowState.ChangingProperties;

        WaitingTimeEnd();

        NowChangePropertyCoolTime = 0;
        NowPropertyTimeLimit = 0;

        Invincibility(true);

        battleButtonManagerInstance.ActionButtonSetActive(false);
        transform.rotation = Quaternion.identity;

        if (isChangeBasicProperty)
        {
            nowProperty = NowPlayerProperty.BasicProperty;

            if (nowProperty == NowPlayerProperty.TheHolySpiritProperty)
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

        hpText.color = (nowProperty == NowPlayerProperty.TheHolySpiritProperty) ?
            hpTextColors[(int)NowStatUIState.Shield] : hpTextColors[(int)NowStatUIState.Basic]; //현재 바뀐 속성이 성령 속성이면 체력 텍스트 색 방어막 상태 색으로 변경

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
            StartCoroutine(ChangeProperty(false));
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

                if (isChangePropertyReady)
                {
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
    void Deflect()
    {
        if (Input.GetKeyDown(KeyCode.Space) && nowState == NowState.Defensing)
        {
            if (nowDefensivePos == DefensePos.Right)
            {
                StartCoroutine(Deflecting(0));
            }
            else if (nowDefensivePos == DefensePos.Left)
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
        nowDefensivePos = DefensePos.None;
        transform.rotation = Quaternion.Euler(0, setRotation, 0);

        battleButtonManagerInstance.ActionButtonSetActive(false);

        ChangeAttackRange(new Vector2(0.85f, 2.68f), new Vector2(0.06f, -0.08f));

        animator.SetBool(PARING, true);
        animator.SetBool(DEFENCE_LR, false);

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

        animator.SetBool(PARING, false);

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
    }

    protected override IEnumerator CoolTimeRunning()
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

                    if (isChangePropertyReady == false && (nowState == NowState.Standingby || nowState == NowState.Defensing))
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
    protected override void WaitingTimeStart()
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

            StartCoroutine(CoolTimeRunning());
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
            rigid.gravityScale = setJumpGravityScale - 0.5f;
            animator.SetTrigger(JUMPING);
            StartCoroutine(JumpDelay());
        }
        else if (nowState == NowState.Jumping && transform.position.y < startPos.y)
        {
            nowState = NowState.Standingby;

            if (isWaiting == false && isChangePropertyReady == false)
            {
                battleButtonManagerInstance.ActionButtonSetActive(true);
            }

            animator.SetBool(JUMP_INTERMEDIATE_MOTION, false);

            CamShake.JumpStop(false);

            transform.position = startPos;
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

        animator.SetBool(JUMP_INTERMEDIATE_MOTION, true);
        rigid.gravityScale = setJumpGravityScale * 1.5f;
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
        battleUIAnimator.SetBool(NOW_RESTING, true);

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

        battleUIAnimator.SetBool(NOW_RESTING, false);

        if (isSlowing)
        {
            battleUIAnimator.SetBool(NOW_SLOWING, true);
        }
        else
        {
            battleUIObjScript.BattleUIObjSetActiveFalse();
        }

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
        Vector3 targettransform = new Vector3(bsm.enemyCharacterPos.x - 3, transform.position.y); //목표 위치

        animator.SetBool(MOVING, true);

        while (transform.position.x < targettransform.x) //이동중
        {
            transform.position += movetransform * Time.deltaTime;
            yield return null;
        }

        transform.position = targettransform; //이동 완료

        animator.SetBool(MOVING, false);
        StartCoroutine(Attacking(false, 1, 0.2f, 0.2f)); //첫번째 공격 실행
        animator.SetTrigger(BASIC_ATTACK);
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

        if (nowAttackCount == 3)
        {
            ChangeAttackRange(new Vector2(2.3f, 3), new Vector2(0.8f, 0.13f));
        }

        switch (nowAttackCount)
        {
            case 1:
                timingObj.HitActionTimingStart(10);
                break;
            case 2:
                timingObj.HitActionTimingStart(11);
                break;
        }

        while (nowdelayTime < maxDelayTime)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(timingObj.pressSpace(true));
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

            bool isDefence = false;

            maxEnemyIndex = rangeInEnemy.Count - 1;

            for (int nowIndex = 0; nowIndex < rangeInEnemy.Count; nowIndex++)
            {
                if (rangeInEnemy[nowIndex] != null)
                {
                    var unitScriptComponenet = rangeInEnemy[nowIndex].GetComponent<BasicUnitScript>();

                    if (unitScriptComponenet.nowState == NowState.Defensing && unitScriptComponenet.nowDefensivePos == DefensePos.Left)
                    {
                        isDefence = true;
                    }

                    unitScriptComponenet.Hit(CurrentRandomDamage(Damage), isDefence, EffectType.Slash);

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
            if (Input.GetKeyDown(KeyCode.Space) && isFail == false)
            {
                StartCoroutine(timingObj.pressSpace(false));
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
                    animator.SetTrigger(BASIC_SECONDATTACK_HITACTION_COMPLEAT);
                    break;
                case 3:
                    StartCoroutine(Attacking(true, nowAttackCount, 0.35f, 0));
                    animator.SetTrigger(BASIC_THIRDATTACK_HITACTION_COMPLEAT);
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
                ChangeAttackRange(new Vector2(2.4f, 0.8f), new Vector2(0.8f, 0.2f));
                yield return new WaitForSeconds(0.5f);
            }

            StartCoroutine(Return());
        }
    }

    /// <summary>
    /// 현재 적에게 줄 랜덤 데미지 뽑는 함수
    /// </summary>
    /// <param name="nowPlayerDamage"> 현재 플레이어 데미지 </param>
    /// <returns></returns>
    float CurrentRandomDamage(float nowPlayerDamage)
    {
        int randDamage = Random.Range(-1, 2); //랜덤 데미지 증감

        nowPlayerDamage += randDamage;

        if (nowPlayerDamage < 1)
        {
            nowPlayerDamage = 1;
        }

        return nowPlayerDamage;
    }

    /// <summary>
    /// 근접 공격(적에게 이동해야하는 공격) 후 복귀 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator Return()
    {
        transform.rotation = Quaternion.Euler(0, 180, 0);

        animator.SetBool(MOVING, true);

        while (transform.position.x > startPos.x)
        {
            transform.position -= movetransform * Time.deltaTime;
            yield return null;
        }

        transform.rotation = Quaternion.identity;
        transform.position = startPos;

        animator.SetBool(MOVING, false);

        WaitingTimeStart();
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
            bsm.StartGameEndPanelAnim(true, new Vector3(0f, 0f, 0f));
        }
    }

    /// <summary>
    /// 부활 실행 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator Resurrection()
    {
        int recoveryFixedValue = 20;
        int ResurrectionStatsValueSharingValue = 5;

        Invincibility(true); //현재 무적 상태 : 참

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
    }

    /// <summary>
    /// 천사 속성 버프 or 버프 해제
    /// </summary>
    /// <param name="isBuffing"> 버프 걸기 판별 (버프를 걸어주는가?) </param>
    void AngelPropertyBuff(bool isBuffing)
    {
        angelPropertyBuffing = isBuffing;

        Damage = isBuffing ? Damage * 2 : originalDamage;
        maxActionCoolTime = isBuffing ? maxActionCoolTime - 1 : originalMaxActionCoolTime;
    }

    /// <summary>
    /// 성령 속성 버프 or 버프 해제
    /// </summary>
    /// <param name="isBuffing"> 버프 걸기 판별 (버프를 걸어주는가?) </param>
    void TheHolySpiritPropertyBuff(bool isBuffing)
    {
        maxActionCoolTime = isBuffing ? maxActionCoolTime - (int)(maxActionCoolTime * 0.25f) : originalMaxActionCoolTime;
        Damage = isBuffing ? (int)(originalDamage * 1.5f) : originalDamage;
    }

    /// <summary>
    /// 성령 속성 디버프 or 디버프 해제
    /// </summary>
    /// <param name="isDeBuffing"> 디버프 걸기 판별 (디버프를 걸어주는가?) </param>
    void TheHolySpiritPropertyDeBuff(bool isDeBuffing)
    {
        maxActionCoolTime = isDeBuffing ? maxActionCoolTime + (int)(maxActionCoolTime * 0.25f) : originalMaxActionCoolTime;
        Damage = isDeBuffing ? (int)(Damage * 0.5f) : originalDamage;
        Speed = isDeBuffing ? originalSpeed * 0.7f : originalSpeed;
    }

    /// <summary>
    /// 기절 함수
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator Fainting()
    {
        nowState = NowState.Fainting;

        if (nowDefensivePos == DefensePos.Left || nowDefensivePos == DefensePos.Right)
        {
            animator.SetBool(DEFENCE_LR, false);
        }
        else if (nowDefensivePos == DefensePos.Up)
        {
            animator.SetBool(DEFENCE_T, false);
        }

        animator.SetBool(FAINTING, true);
        nowDefensivePos = DefensePos.None;
        battleButtonManagerInstance.ActionButtonSetActive(false);

        yield return new WaitForSeconds(0.2f);

        battleUIObjScript.BattleUIObjSetActiveTrue(ChangeBattleUIAnim.Faint);
        battleUIAnimator.SetBool(NOW_FAINTING, true);

        yield return new WaitForSeconds(5); //나중에 매개변수로 레벨에 따라서 기절 시간 넣기

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
    /// 속성 패시브 효과 실행 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator PropertyPassiveAbilityStart()
    {
        NowChangePropertyCoolTime = 0;

        while (nowState != NowState.ChangingProperties)
        {
            yield return null;
        }

        switch (nowProperty)
        {
            case NowPlayerProperty.NatureProperty:

                NowNaturePassiveCount = 0;

                while (nowProperty == NowPlayerProperty.NatureProperty)
                {
                    NowNaturePassiveCount += Time.deltaTime;
                    yield return null;
                }

                break;

            case NowPlayerProperty.ForceProperty:

                int enhancedDamage = (int)(Damage * 0.5f);
                float reducedMaxActionCoolTime = maxActionCoolTime * 0.2f;

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
