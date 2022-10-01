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
    AttackCoolTimeWaiting,
    Defensing,
    Deflecting,
    Jumping,
    Resting,
    Fainting,
    Attacking,
    ChangingProperties
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

    [Tooltip("최대 쿨타임")]
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

    [HideInInspector]
    public DefensePos nowDefensivePosition; //현재 방어 위치

    [HideInInspector]
    public NowState nowState; //현재 행동

    protected bool isWaiting; //대기중

    private bool isHpDiminishedProduction;

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
    protected float hp_F;
    public float Hp_F
    {
        get { return hp_F; }
        set
        {
            if (value > lightHp_F)
            {
                lightHp_F = Hp_F;
            }
            if (value <= 0)
            {
                hp_F = 0;
                Dead();
            }
            else
            {
                hp_F = value;
            }
            if (isHpDiminishedProduction == false)
            {
                StartCoroutine(HpDiminishedProduction());
            }
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
            energy_F = (value <= 0) ? energy_F = 0 : energy_F = value;
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
        }
    }

    protected float maxDreamyFigure_F; //최대 몽환 게이지

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

    protected virtual void Awake()
    {
        StartSameSetting();
        StartSetting();
    }

    protected virtual void Update()
    {
        UISetting();
        Faint();
        UnitBarsUpdate();
    }

    protected void StartSameSetting()
    {
        Cam = Camera.main;
        startPos_Vector = transform.position;
        nowAttackCount_I = 1;
        nowActionCoolTime = 0;
    }

    protected abstract void StartSetting();

    protected void UnitBarsUpdate()
    {
        unitHpBars.fillAmount = Hp_F / MaxHp_F;
        unitEnergyBars.fillAmount = Energy_F / MaxEnergy_F;
        unitDreamyFigureBars.fillAmount = DreamyFigure_F / maxDreamyFigure_F;
        unitLightHpBars.fillAmount = lightHp_F / MaxHp_F;
    }

    public virtual void Hit(float damage, bool isDefending)
    {
        if (isDefending)
        {
            Energy_F -= 1;
            DreamyFigure_F += 1;
        }
        else
        {
            Hp_F -= damage;
            DreamyFigure_F += 2;
        }
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
            yield return null;
        }
        lightHp_F = Hp_F;
        isHpDiminishedProduction = false;
    }

    protected void ReleaseDefense()
    {
        nowDefensivePosition = DefensePos.None;
        nowState = NowState.Standingby;
    }

    protected abstract void Faint();

    protected abstract void UISetting();

    protected abstract void Defense();

    protected abstract void SetDefensing(DefensePos nowDefensePos, float setRotation);

    protected void ActionCoolTimeBarSetActive(bool SetActive) => actionCoolTimeObj.SetActive(SetActive);

    protected abstract void Dead();

    protected abstract IEnumerator PropertyPassiveAbilityStart();

    protected abstract IEnumerator Fainting();

    protected void ChangeAttackRange(Vector2 attackRangeColliderSize, Vector2 attackRangeColliderOffset)
    {
        attackRangeObjComponent.size = attackRangeColliderSize;
        attackRangeObjComponent.offset = attackRangeColliderOffset;
    }

    protected void InitializationAttackRange()
    {
        attackRangeObjComponent.size = InitializationAttackRangeSize;
        attackRangeObjComponent.offset = InitializationAttackRangeOffset;
    }
}