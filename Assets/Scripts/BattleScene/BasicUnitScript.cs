using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum DefensePos
{
    Left,
    Right,
    Up,
    None
}

public enum NowState
{
    Standingby,
    Defensing,
    Deflecting,
    Jumping,
    Resting,
    Fainting,
    Attacking,
    ChangingProperties,
    Resurrection,
    ChangePhase,
    Dead
}

public enum StateColor
{
    BasicColor,
    HitColor,
    BurningColor
}

public enum NowEnemyProperty
{
    Mutant,
    Guardian,
    Rot,
    EvilSpirit,
    Angel,
    PropertyTotalNumber
}

public enum BossPhase
{
    PhaseOne = 1,
    PhaseTwo = 2,
    PhaseThree = 3
}

public abstract class BasicUnitScript : MonoBehaviour
{
    #region 공격 쿨타임 관련 변수 (공통)
    [Header("공격 쿨타임 관련 변수")]
    [Tooltip("쿨타임 바들 오브젝트")]
    [SerializeField]
    protected GameObject actionCoolTimeObj;

    [Tooltip("쿨타임 바 이미지(배경)")]
    [SerializeField]
    protected Image nullActionCoolTimeImage;

    [Tooltip("쿨타임 바 이미지")]
    [SerializeField]
    protected Image actionCoolTimeImage;

    [Tooltip("쿨타임 바 위치 조절")]
    [SerializeField]
    protected Vector2 actionCoolTimeObjPlusPos;

    protected float nowActionCoolTime; //현재 쿨타임

    public float maxActionCoolTime;

    #endregion

    #region 행동 관련 변수
    [Header("현재 행동 관련 변수")]
    [Tooltip("중력값")]
    [SerializeField]
    protected float setJumpGravityScale_F;

    protected IEnumerator nowCoroutine; //현재 실행중인 코루틴 동작

    [Tooltip("점프 파워")]
    [SerializeField]
    protected float jumpPower_F;

    protected Vector3 movetransform; //이동을 위해 더해줄 Vector값

    protected float restWaitTime;

    [HideInInspector]
    public DefensePos nowDefensivePosition; //현재 방어 위치

    [HideInInspector]
    public NowState nowState; //현재 행동

    protected bool isWaiting; //대기중

    private bool isHpDiminishedProduction;

    private bool isInvincibility; //현재 무적인지 판별

    public bool IsInvincibility //현재 무적인지 판별 변수 프로퍼티
    {
        get
        {
            return isInvincibility;
        }
        set
        {
            InvincibilityEvent(value);
            isInvincibility = value;
        }
    }

    [HideInInspector]
    public List<GameObject> rangeInEnemy = new List<GameObject>(); //공격 범위 내의 적 리스트

    [HideInInspector]
    public List<GameObject> rangeInDeflectAbleObj = new List<GameObject>(); //튕겨내기 범위 내의 오브젝트 리스트

    protected int nowAttackCount_I; //연속공격 시 현재 공격 횟수
    #endregion

    #region 스탯 (공통)
    [Header("스탯 관련 변수")]
    [Tooltip("체력")]
    [SerializeField]
    private float hp;
    public float Hp
    {
        get { return hp; }
        set
        {
            if (value >= MaxHp)
            {
                hp = MaxHp;
            }
            else
            {
                if (value > lightHp)
                {
                    lightHp = Hp;
                }
                if (value <= 0 && isResurrectionReady == false)
                {
                    hp = 0;
                    StartCoroutine(Dead());
                }
                else
                {
                    hp = value;
                }
            }

            if (isHpDiminishedProduction == false)
            {
                StartCoroutine(HpDiminishedProduction());
            }

            hpText.text = $"{(Hp):N0}/{(MaxHp):N0}";
            unitHpBar.fillAmount = Hp / MaxHp;
        }
    }

    protected float lightHp;

    [Tooltip("최대 체력")]
    [SerializeField]
    protected float maxHp;

    public float MaxHp
    {
        get { return maxHp; }
        set { maxHp = value; }
    }

    [Tooltip("기력")]
    [SerializeField]
    protected float energy;
    public float Energy
    {
        get { return energy; }
        set
        {
            energy = (value < 0) ? energy = 0 : energy = value;
            
            energyText.text = $"{(Energy):N0}/{(MaxEnergy):N0}";
            
            unitEnergyBars.fillAmount = Energy / MaxEnergy;
            
            if (value <= 0)
            {
                StartCoroutine(WaitForFaint());
            }
        }
    }

    [Tooltip("최대 기력")]
    [SerializeField]
    protected float maxEnergy;
    public float MaxEnergy
    {
        get { return maxEnergy; }
        set { maxEnergy = value; }
    }

    [Tooltip("몽환 게이지 유무 판별")]
    [SerializeField]
    protected bool isHaveDreamyFigure;

    [Tooltip("몽환 게이지")]
    [SerializeField]
    protected float dreamyFigure;
    public float DreamyFigure
    {
        get { return dreamyFigure; }
        set
        {
            dreamyFigure = (value >= maxDreamyFigure) ? dreamyFigure = maxDreamyFigure : dreamyFigure = value;
            dreamyFigureText.text = $"{DreamyFigure}/{maxDreamyFigure}";
            unitDreamyFigureBars.fillAmount = DreamyFigure / maxDreamyFigure;
        }
    }

    private const float maxDreamyFigure = 20; //최대 몽환 게이지

    [Tooltip("공격력")]
    [SerializeField]
    protected int damage;
    public int Damage
    {
        get { return damage; }
        set { damage = value; }
    }

    [Tooltip("이동속도")]
    [SerializeField]
    private float speed;

    public float Speed 
    {
        get { return speed; }
        set 
        {
            speed = value;
            movetransform.x = value;
        }
    }
    #endregion

    #region 스탯 원본 수치 (버프/디버프에 사용)
    protected int originalDamage; //현재 플레이어 기본 데미지 수치

    protected float originalMaxActionCoolTime; //현재 플레이어 기본 행동 쿨타임 수치

    protected float originalRestWaitTime; //현재 플레이어 기본 휴식 시간 수치

    protected float originalSpeed; //현재 플레이어 기본 이동속도 수치
    #endregion

    #region 화상 관련 변수
    protected bool isBurning;

    protected int nowBurnDamageStack; //현재 중첩된 스택

    protected int additionalLimitTime; //스택 중첩 추가시간

    protected float nowBurnDamageLimitTime; //현재 화상 지속시간

    protected float maxBurnDamageLimitTime; //최대 화상 지속시간

    protected float maxStackableOverlapTime; //스택 중첩 가능 시간

    protected float nowGiveBurnDamageTime;

    protected float maxGiveBurnDamageTime;

    [SerializeField]
    [Tooltip("화상 이펙트 오브젝트")]
    private GameObject burnEffect;
    #endregion

    #region 이동속도 감소 디버프 관련 변수 모음
    [HideInInspector]
    public int hitSlowCount; //현재 충돌중인 이동속도 감소 효과 장애물 개수
    #endregion

    #region 스탯 UI 이미지 모음
    [Tooltip("유닛 체력바 이미지")]
    public Image unitHpBar;

    [Tooltip("유닛 체력바 배경 이미지")]
    public Image unitHpBarBg;

    [SerializeField]
    [Tooltip("유닛 체력바 연출 이미지")]
    private Image unitLightHpBars;

    [SerializeField]
    [Tooltip("유닛 기력바 이미지")]
    private Image unitEnergyBars;

    [SerializeField]
    [Tooltip("유닛 몽환 게이지 이미지")]
    private Image unitDreamyFigureBars;

    [SerializeField]
    [Tooltip("체력 표시 텍스트")]
    protected TextMeshProUGUI hpText;

    [SerializeField]
    [Tooltip("기력 표시 텍스트")]
    protected TextMeshProUGUI energyText;

    [SerializeField]
    [Tooltip("몽환 게이지 표시 텍스트")]
    protected TextMeshProUGUI dreamyFigureText;
    #endregion

    protected bool isSlowing; //디버프 : 이동속도 감소 효과 판별

    protected bool isImmunity; //현재 디버프 면역 상태인지 판별

    protected Vector3 particlePos; //타격 시 파티클(이펙트) 생성 위치

    protected const int maxGoodGetCount = 15; //최대 재화 획득 가능 횟수

    protected int nowGoodGetCount; //현재 재화 획득 횟수

    [HideInInspector]
    public Vector2 startPos;

    protected Camera Cam;

    [SerializeField]
    [Tooltip("자신의 리지드바디")]
    protected Rigidbody2D rigid;

    [SerializeField]
    [Tooltip("자신의 공격 범위 콜라이더")]
    protected BoxCollider2D attackRangeObjComponent;

    protected Vector2 InitializationAttackRangeSize;

    protected Vector2 InitializationAttackRangeOffset;

    [SerializeField]
    [Tooltip("현재 상태 표시해주는 UI 오브젝트 스크립트")]
    protected BattleUIObj battleUIObjScript;

    #region 애니메이터 모음
    [Header("애니메이터 모음")]
    [Tooltip("유닛 애니메이션")]
    public Animator animator;

    [SerializeField]
    [Tooltip("현재 상태 표시해주는 UI 애니메이션")]
    protected Animator battleUIAnimator;
    #endregion

    #region 유닛 기본 애니메이션 이름 모음
    protected const string moving = "Moving";

    protected const string resting = "Resting";

    protected const string basicAttack = "BasicAttack";

    protected const string jumping = "Jumping";

    protected const string dead = "Dead";

    protected const string fainting = "Fainting";

    protected const string defenceLR = "Defence(Left&Right)";
    #endregion

    #region 상태 UI 애니메이션 이름 모음
    protected const string nowFainting = "NowFainting";

    protected const string nowResting = "NowResting";

    protected const string nowSlowing = "NowSlowing";
    #endregion

    #region 피해 요소 모음
    [Header("피해 요소 모음")]
    [SerializeField]
    [Tooltip("자신의 스프라이트 렌더러")]
    protected SpriteRenderer spriteRenderer;

    [Tooltip("상태에 따른 유닛 색 모음")]
    public Color[] stateColors;

    protected Vector3 plusVector; //데미지 텍스트 스폰 위치 정밀 조정(자신 위치에서 더해줌)

    protected WaitForSeconds changeToBasicColorDelay = new WaitForSeconds(0.1f); //공격에 맞았을 시 색 변경 딜레이

    protected bool isResurrectionReady; //부활 준비 여부 판별
    #endregion

    protected ObjectPool objectPoolInstance; //오브젝트 풀 싱글톤 인스턴스

    protected BattleSceneManager bsm; //배틀 씬 매니저 싱글톤 인스턴스

    protected BattleButtonManager battleButtonManagerInstance; //배틀 버튼 매니저 싱글톤 인스턴스


    protected virtual void Awake()
    {
        StartSameSetting();
    }

    protected virtual void Start()
    {
        StartSetting();
    }

    protected void StartSameSetting()
    {
        Cam = Camera.main;

        bsm = BattleSceneManager.Instance;
        objectPoolInstance = ObjectPool.Instance;
        battleButtonManagerInstance = BattleButtonManager.Instance;

        startPos = transform.position; //시작 위치 저장
        movetransform.x = Speed; //시작 이동속도로 움직임 벡터 X값 저장

        nowAttackCount_I = 1;
        nowActionCoolTime = 0;
        maxGiveBurnDamageTime = 4; //화상 효과 최대 데미지 횟수
        maxStackableOverlapTime = 10; //화상 효과 중첩 가능 제한시간 초기화
        maxBurnDamageLimitTime = 15; //화상 효과 지속시간 증가 초기화
    }

    protected abstract void StartSetting();

    /// <summary>
    /// 데미지가 들어왔을 때 실행하는 함수
    /// </summary>
    /// <param name="damage"> 들어온 데미지 </param>
    /// <param name="isDefending"> 현재 들어온 공격의 방어 성공 유무 </param>
    public virtual void Hit(float damage, bool isDefending)
    {
        if (hp > 0)
        {
            var damageText = objectPoolInstance.GetObject((int)PoolObjKind.DamageText); //데미지 텍스트 소환(오브젝트 풀)
            TextState nowTextState = TextState.Blocking; //현재 데미지 텍스트 상태

            if (isInvincibility == false)
            {
                if (isDefending)
                {
                    Energy -= 1;
                    DreamyFigure += 1;
                }
                else
                {
                    spriteRenderer.color = stateColors[(int)StateColor.HitColor]; //맞았을 때의 효과 : 색 변경
                    Hp -= damage;
                    DreamyFigure += 2;
                    StartCoroutine(ChangeToBasicColor());
                    nowTextState = TextState.BasicDamage;
                }
            }

            damageText.GetComponent<DamageText>().TextCustom(nowTextState, transform.position + plusVector, damage);
        }
    }

    /// <summary>
    /// 공격 후 공격 쿨타임 실행 함수
    /// </summary>
    protected virtual void WaitingTimeStart()
    {
        nowState = NowState.Standingby;

        if (Hp > 0 && Energy > 0)
        {
            isWaiting = true;

            if (nowActionCoolTime < maxActionCoolTime)
            {
                ActionCoolTimeBarSetActive(true);
            }

            StartCoroutine(UISetting());
        }
    }

    /// <summary>
    /// 맞았을 때의 효과 : 원래 색으로 변경
    /// </summary>
    /// <returns></returns>
    protected IEnumerator ChangeToBasicColor()
    {
        yield return changeToBasicColorDelay;

        spriteRenderer.color = (isBurning) ? stateColors[(int)StateColor.BurningColor] : stateColors[(int)StateColor.BasicColor];
    }

    /// <summary>
    /// 체력 줄어드는 체력바 효과 함수
    /// </summary>
    /// <returns></returns>
    protected IEnumerator HpDiminishedProduction()
    {
        float nowReductionSpeed = MaxHp / 12;
        isHpDiminishedProduction = true;
        yield return new WaitForSeconds(0.6f);
        while (lightHp > Hp)
        {
            lightHp -= Time.deltaTime * nowReductionSpeed;
            nowReductionSpeed += Time.deltaTime * 2;
            unitLightHpBars.fillAmount = lightHp / MaxHp;
            yield return null;
        }
        lightHp = Hp;
        isHpDiminishedProduction = false;
        unitLightHpBars.fillAmount = lightHp / MaxHp;
    }

    protected void ReleaseDefense()
    {
        nowDefensivePosition = DefensePos.None;
        nowState = NowState.Standingby;
    }

    protected abstract IEnumerator UISetting();

    protected virtual void WaitingTimeEnd()
    {
        isWaiting = false;
        nowActionCoolTime = 0;
    }

    protected virtual void Defense()
    {
        
    }

    protected virtual void SetDefensing(DefensePos nowDefensePos, float setRotation)
    {
        
    }

    protected void ActionCoolTimeBarSetActive(bool SetActive) => actionCoolTimeObj.SetActive(SetActive);

    protected virtual void InvincibilityEvent(bool isInvincibilityTrue)
    {
        
    }

    protected abstract IEnumerator Dead();

    protected abstract IEnumerator Fainting();

    protected abstract IEnumerator Resting();

    IEnumerator WaitForFaint()
    {
        while (nowState != NowState.Standingby && nowState != NowState.Defensing)
        {
            yield return null;
        }

        nowCoroutine = Fainting();
        StartCoroutine(nowCoroutine);
    }

    /// <summary>
    /// 공격 범위 변경
    /// </summary>
    /// <param name="attackRangeColliderSize"> 공격 범위 변경(크기) </param>
    /// <param name="attackRangeColliderOffset"> 공격 범위 변경(위치) </param>
    protected void ChangeAttackRange(Vector2 attackRangeColliderSize, Vector2 attackRangeColliderOffset)
    {
        attackRangeObjComponent.size = attackRangeColliderSize;
        attackRangeObjComponent.offset = attackRangeColliderOffset;
    }

    protected void Invincibility(bool isInvincibilityOn) => IsInvincibility = isInvincibilityOn; //무적 ON or OFF

    protected void InitializationAttackRange()
    {
        attackRangeObjComponent.size = InitializationAttackRangeSize;
        attackRangeObjComponent.offset = InitializationAttackRangeOffset;
    }

    /// <summary>
    /// 화상 효과(데미지) 함수
    /// </summary>
    /// <returns></returns>
    protected IEnumerator Burning()
    {
        while (isBurning)
        {
            nowBurnDamageLimitTime += Time.deltaTime;
            nowGiveBurnDamageTime += Time.deltaTime;

            if (nowGiveBurnDamageTime >= maxGiveBurnDamageTime)
            {
                if (isInvincibility == false)
                {
                    Hit(1, false);
                }
                nowGiveBurnDamageTime = 0;
            }

            if (nowBurnDamageLimitTime >= maxBurnDamageLimitTime || Hp <= 0)
            {
                isBurning = false;

                burnEffect.SetActive(isBurning);

                spriteRenderer.color = stateColors[(int)StateColor.BasicColor];
                nowBurnDamageStack = 0;
                nowBurnDamageLimitTime = 0;
                nowGiveBurnDamageTime = 0;
                break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 화상 효과 시작 함수
    /// </summary>
    public void BurnDamageStart()
    {
        if (isImmunity == false)
        {
            if (nowBurnDamageStack == 5 || nowBurnDamageLimitTime >= maxStackableOverlapTime)
            {
                return;
            }

            nowBurnDamageStack++;
            nowBurnDamageLimitTime = 0;

            maxStackableOverlapTime = 10 - nowBurnDamageStack; //현재 스택에 따른 화상 효과 중첩 가능 제한 시간
            maxBurnDamageLimitTime = 15 + nowBurnDamageStack; //스택이 높을 수록 화상 지속시간 증가

            if (nowBurnDamageStack == 1)
            {
                isBurning = true;

                burnEffect.SetActive(isBurning);

                spriteRenderer.color = stateColors[(int)StateColor.BurningColor];
                StartCoroutine(Burning());
            }
        }
    }

    /// <summary>
    /// 이동속도 감소 디버프 활성화 or 비활성화 함수
    /// </summary>
    /// <param name="isDebuffOn"> 디버프 활성화 유무(false시 비활성화) </param>
    /// <param name="percentage"> 이동속도 감소 % 수치 </param>
    public void SlowDebuff(bool isDebuffOn, int percentage)
    {
        if (isImmunity == false)
        {
            if (isDebuffOn)
            {
                isSlowing = true;

                battleUIObjScript.BattleUIObjSetActiveTrue(ChangeBattleUIAnim.SlowDebuff);
                battleUIAnimator.SetBool("NowSlowing", true);

                Speed = (originalSpeed - originalSpeed * percentage / 100);
            }
            else
            {
                isSlowing = false;

                if (nowState != NowState.Fainting && nowState != NowState.Resting)
                {
                    battleUIObjScript.BattleUIObjSetActiveFalse();
                }

                battleUIAnimator.SetBool("NowSlowing", false);

                Speed = originalSpeed;
            }
        }
    }

    /// <summary>
    /// 재화 지급 함수(보스)
    /// </summary>
    public virtual void GetBasicGood() //후에 보스별로 주는 재화량 다르게 하기
    {
        if (nowGoodGetCount < maxGoodGetCount)
        {
            int nowGetRandGood = Random.Range(5, 11);
            bsm.NowGetBasicGood += nowGetRandGood;
            nowGoodGetCount++;
        }
    }
}