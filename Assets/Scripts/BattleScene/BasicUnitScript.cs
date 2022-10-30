using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [Tooltip("쿨타임 바 Y축 위치 조절")]
    [SerializeField]
    protected float actionCoolTimeImageYPos_F;

    protected float nowActionCoolTime; //현재 쿨타임

    public float maxActionCoolTime;

    #endregion

    #region 행동 관련 변수
    [Header("현재 행동 관련 변수")]
    [Tooltip("중력값")]
    [SerializeField]
    protected float setJumpGravityScale_F;

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

    protected bool isInvincibility;

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
    private float hp_F;
    public float Hp_F
    {
        get { return hp_F; }
        set
        {
            if (value >= MaxHp_F)
            {
                hp_F = MaxHp_F;
            }
            else
            {
                if (value > lightHp_F)
                {
                    lightHp_F = Hp_F;
                }
                if (value <= 0)
                {
                    hp_F = 0;
                    StartCoroutine(Dead());
                }
                else
                {
                    hp_F = value;
                }
            }

            if (isHpDiminishedProduction == false)
            {
                StartCoroutine(HpDiminishedProduction());
            }

            hpText.text = $"{(Hp_F):N0}/{(MaxHp_F):N0}";
            unitHpBars.fillAmount = Hp_F / MaxHp_F;
        }
    }

    protected float lightHp_F;

    [Tooltip("최대 체력")]
    [SerializeField]
    protected float maxHp_F;

    public float MaxHp_F
    {
        get { return maxHp_F; }
        set { maxHp_F = value; }
    }

    [Tooltip("기력")]
    [SerializeField]
    protected float energy_F;
    public float Energy_F
    {
        get { return energy_F; }
        set
        {
            energy_F = (value < 0) ? energy_F = 0 : energy_F = value;
            
            energyText.text = $"{(Energy_F):N0}/{(MaxEnergy_F):N0}";
            
            unitEnergyBars.fillAmount = Energy_F / MaxEnergy_F;
            
            if (value <= 0)
            {
                StartCoroutine(Fainting());
            }
        }
    }

    [Tooltip("최대 기력")]
    [SerializeField]
    protected float maxEnergy_F;
    public float MaxEnergy_F
    {
        get { return maxEnergy_F; }
        set { maxEnergy_F = value; }
    }

    [Tooltip("몽환 게이지 유무 판별")]
    [SerializeField]
    protected bool isHaveDreamyFigure;

    [Tooltip("몽환 게이지")]
    [SerializeField]
    protected float dreamyFigure_F;
    public float DreamyFigure_F
    {
        get { return dreamyFigure_F; }
        set
        {
            dreamyFigure_F = (value >= maxDreamyFigure_F) ? dreamyFigure_F = maxDreamyFigure_F : dreamyFigure_F = value;
            dreamyFigureText.text = $"{DreamyFigure_F}/{maxDreamyFigure_F}";
            unitDreamyFigureBars.fillAmount = DreamyFigure_F / maxDreamyFigure_F;
        }
    }

    [HideInInspector]
    public float maxDreamyFigure_F; //최대 몽환 게이지

    [Tooltip("공격력")]
    [SerializeField]
    protected float damage_I;
    public float Damage_I
    {
        get { return damage_I; }
        set { damage_I = value; }
    }

    [Tooltip("이동속도")]
    [SerializeField]
    protected float Speed_F;
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
    [SerializeField]
    [Tooltip("유닛 체력바 이미지")]
    private Image unitHpBars;

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
    protected Text hpText;

    [SerializeField]
    [Tooltip("기력 표시 텍스트")]
    protected Text energyText;

    [SerializeField]
    [Tooltip("몽환 게이지 표시 텍스트")]
    protected Text dreamyFigureText;
    #endregion

    [HideInInspector]
    public Vector2 startPos_Vector;

    protected Camera Cam;

    [SerializeField]
    [Tooltip("자신의 리지드바디")]
    protected Rigidbody2D rigid;

    [SerializeField]
    [Tooltip("자신의 공격 범위 콜라이더")]
    protected BoxCollider2D attackRangeObjComponent;

    protected Vector2 InitializationAttackRangeSize = new Vector2(1.1f, 2.6f);

    protected Vector2 InitializationAttackRangeOffset = new Vector2(0.2f, 0);

    [SerializeField]
    [Tooltip("현재 상태 표시해주는 UI 오브젝트 스크립트")]
    protected BattleUIObj battleUIObjScript;

    [SerializeField]
    [Tooltip("현재 상태 표시해주는 UI 애니메이션")]
    protected Animator battleUIAnimator;

    [SerializeField]
    [Tooltip("자신의 스프라이트 렌더러")]
    protected SpriteRenderer spriteRenderer;

    [Tooltip("타격 시 잠깐동안 캐릭터가 바뀔 색")]
    public Color hitColor;

    [Tooltip("타격 색 변경 후 원래 색으로 되돌림")]
    public Color returnBasicColor;

    protected WaitForSeconds changeToBasicColorDelay;

    protected virtual void Awake()
    {
        StartSameSetting();
        StartSetting();
    }

    protected virtual void Update()
    {
        UISetting();
        Burning();
    }

    protected void StartSameSetting()
    {
        Cam = Camera.main;
        startPos_Vector = transform.position;
        nowAttackCount_I = 1;
        nowActionCoolTime = 0;
        maxGiveBurnDamageTime = 3;
        maxStackableOverlapTime = 10; //화상 효과 중첩 가능 제한시간 초기화
        maxBurnDamageLimitTime = 15; //화상 효과 지속시간 증가 초기화

        hpText.text = $"{(Hp_F):N0}/{(MaxHp_F):N0}";
        energyText.text = $"{(Energy_F):N0}/{(MaxEnergy_F):N0}";
        dreamyFigureText.text = $"{DreamyFigure_F}/{maxDreamyFigure_F}";

        unitHpBars.fillAmount = Hp_F / MaxHp_F;
        unitEnergyBars.fillAmount = Energy_F / MaxEnergy_F;
        unitDreamyFigureBars.fillAmount = DreamyFigure_F / maxDreamyFigure_F;
        unitLightHpBars.fillAmount = lightHp_F / MaxHp_F;

        changeToBasicColorDelay = new WaitForSeconds(0.1f);
    }

    protected abstract void StartSetting();

    public virtual void Hit(float damage, bool isDefending)
    {
        if (isInvincibility == false)
        {
            if (isDefending)
            {
                Energy_F -= 1;
                DreamyFigure_F += 1;
            }
            else
            {
                spriteRenderer.color = hitColor;
                Hp_F -= damage;
                DreamyFigure_F += 2;
                StartCoroutine(ChangeToBasicColor());
            }
        }
    }

    protected IEnumerator ChangeToBasicColor()
    {
        yield return changeToBasicColorDelay;
        spriteRenderer.color = returnBasicColor;
    }

    protected IEnumerator HpDiminishedProduction()
    {
        float nowReductionSpeed = MaxHp_F / 12;
        isHpDiminishedProduction = true;
        yield return new WaitForSeconds(0.7f);
        while (lightHp_F > Hp_F)
        {
            lightHp_F -= Time.deltaTime * nowReductionSpeed;
            nowReductionSpeed += Time.deltaTime * 2;
            unitLightHpBars.fillAmount = lightHp_F / MaxHp_F;
            yield return null;
        }
        lightHp_F = Hp_F;
        isHpDiminishedProduction = false;
        unitLightHpBars.fillAmount = lightHp_F / MaxHp_F;
    }

    protected void ReleaseDefense()
    {
        nowDefensivePosition = DefensePos.None;
        nowState = NowState.Standingby;
    }

    protected abstract void UISetting();

    protected virtual void WaitingTimeEnd()
    {
        isWaiting = false;
        nowActionCoolTime = 0;
    }

    protected abstract void Defense();

    protected abstract void SetDefensing(DefensePos nowDefensePos, float setRotation);

    protected void ActionCoolTimeBarSetActive(bool SetActive) => actionCoolTimeObj.SetActive(SetActive);

    protected abstract IEnumerator Dead();

    protected abstract IEnumerator Fainting();

    protected abstract IEnumerator Resting();

    protected void ChangeAttackRange(Vector2 attackRangeColliderSize, Vector2 attackRangeColliderOffset)
    {
        attackRangeObjComponent.size = attackRangeColliderSize;
        attackRangeObjComponent.offset = attackRangeColliderOffset;
    }

    protected void Invincibility(bool isInvincibilityOn) => isInvincibility = isInvincibilityOn; //무적 ON or OFF
    protected void InitializationAttackRange()
    {
        attackRangeObjComponent.size = InitializationAttackRangeSize;
        attackRangeObjComponent.offset = InitializationAttackRangeOffset;
    }

    protected void Burning()
    {
        if (isBurning)
        {
            nowBurnDamageLimitTime += Time.deltaTime;
            nowGiveBurnDamageTime += Time.deltaTime;

            if (nowGiveBurnDamageTime >= maxGiveBurnDamageTime)
            {
                Hp_F -= 1;
                nowGiveBurnDamageTime = 0;
            }

            if (nowBurnDamageLimitTime >= maxBurnDamageLimitTime)
            {
                isBurning = false;
                nowBurnDamageStack = 0;
                nowBurnDamageLimitTime = 0;
                nowGiveBurnDamageTime = 0;
            }
        }
    }

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
        }
    }
}