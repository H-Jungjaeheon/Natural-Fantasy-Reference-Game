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
    Dead
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

    protected float restWaitTime;

    [HideInInspector]
    public DefensePos nowDefensivePosition; //현재 방어 위치

    [HideInInspector]
    public NowState nowState; //현재 행동

    protected bool isWaiting; //대기중

    private bool isHpDiminishedProduction;

    private bool isInvincibility; //현재 무적인지 판별

    public bool IsInvincibility
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
                nowCoroutine = Fainting();
                StartCoroutine(nowCoroutine);
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

    [HideInInspector]
    public const float maxDreamyFigure = 20; //최대 몽환 게이지

    [Tooltip("공격력")]
    [SerializeField]
    protected float damage;
    public float Damage
    {
        get { return damage; }
        set { damage = value; }
    }

    [Tooltip("이동속도")]
    [SerializeField]
    protected float Speed;
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

    protected const int maxGoodGetCount = 15;

    protected int nowGoodGetCount;

    [HideInInspector]
    public Vector2 startPos_Vector;

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

    #region 맞았을 때의 요소 모음
    [Header("맞았을 때의 요소 모음")]
    [SerializeField]
    [Tooltip("자신의 스프라이트 렌더러")]
    protected SpriteRenderer spriteRenderer;

    [Tooltip("타격 시 잠깐동안 캐릭터가 바뀔 색")]
    public Color hitColor;

    [Tooltip("타격 색 변경 후 원래 색으로 되돌림")]
    public Color returnBasicColor;

    [SerializeField]
    [Tooltip("데미지 텍스트 스폰 위치 정밀 조정(자신 위치에서 더해줌)")]
    protected Vector3 plusVector;

    protected bool isResurrectionReady; //부활 준비 여부 판별
    #endregion

    protected ObjectPool objectPoolInstance; //오브젝트 풀 싱글톤 인스턴스

    protected BattleSceneManager bsm; //배틀 씬 매니저 싱글톤 인스턴스

    protected BattleButtonManager battleButtonManagerInstance; //배틀 버튼 매니저 싱글톤 인스턴스

    protected WaitForSeconds changeToBasicColorDelay;

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

        startPos_Vector = transform.position;

        nowAttackCount_I = 1;
        nowActionCoolTime = 0;
        maxGiveBurnDamageTime = 4; //화상 효과 최대 데미지 횟수
        maxStackableOverlapTime = 10; //화상 효과 중첩 가능 제한시간 초기화
        maxBurnDamageLimitTime = 15; //화상 효과 지속시간 증가 초기화

        changeToBasicColorDelay = new WaitForSeconds(0.1f);
    }

    protected abstract void StartSetting();

    /// <summary>
    /// 데미지가 들어왔을 때 실행하는 함수
    /// </summary>
    /// <param name="damage"> 들어온 데미지 </param>
    /// <param name="isDefending"> 현재 들어온 공격의 방어 성공 유무 </param>
    public virtual void Hit(float damage, bool isDefending)
    {
        var damageText = objectPoolInstance.GetObject((int)PoolObjKind.DamageText); //데미지 텍스트 소환(오브젝트 풀)
        float calculatedDamage = damage; //추가 연산을 끝낸 최종 데미지값

        if (isInvincibility == false)
        {
            if (isDefending)
            {
                Energy -= 1;
                DreamyFigure += 1;
                damageText.GetComponent<DamageText>().TextCustom(TextState.Blocking, transform.position + plusVector, calculatedDamage);
            }
            else
            {
                spriteRenderer.color = hitColor; //맞았을 때의 효과 : 색 변경
                Hp -= damage;
                DreamyFigure += 2;
                StartCoroutine(ChangeToBasicColor());
                damageText.GetComponent<DamageText>().TextCustom(TextState.BasicDamage, transform.position + plusVector, calculatedDamage);
            }
        }
        else
        {
            damageText.GetComponent<DamageText>().TextCustom(TextState.Blocking, transform.position + plusVector, calculatedDamage);
        }
    }

    /// <summary>
    /// 맞았을 때의 효과 : 원래 색으로 변경
    /// </summary>
    /// <returns></returns>
    protected IEnumerator ChangeToBasicColor()
    {
        yield return changeToBasicColorDelay;
        spriteRenderer.color = returnBasicColor;
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
                    Hp -= 1;
                }
                nowGiveBurnDamageTime = 0;
            }

            if (nowBurnDamageLimitTime >= maxBurnDamageLimitTime || Hp <= 0)
            {
                isBurning = false;
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
            StartCoroutine(Burning());
        }
    }

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