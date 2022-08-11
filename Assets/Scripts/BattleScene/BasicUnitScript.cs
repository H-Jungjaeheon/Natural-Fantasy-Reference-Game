using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum NowDefensePos
{
    Left,
    Right,
    Up,
    DefensePosCount
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

    protected bool isWaiting; //대기중
    #endregion

    #region 행동 관련 변수
    [Header("현재 행동 관련 변수")]
    [Tooltip("중력값")]
    [SerializeField]
    protected float setJumpGravityScale_F;

    [Tooltip("점프 파워")]
    [SerializeField]
    protected float jumpPower_F;

    [Header("방어 / 방어 위치 관련 변수")]
    [Tooltip("현재 방어 위치")]
    public bool[] nowDefensivePosition_B;

    protected bool isDefensing;

    protected bool isDeflecting;

    [Header("공격 범위 내의 적 리스트")]
    public List<GameObject> rangeInEnemy = new List<GameObject>();

    [Header("튕겨내기 범위 내의 오브젝트 리스트")]
    public List<GameObject> rangeInDeflectAbleObj = new List<GameObject>();

    protected int nowAttackCount_I;

    [HideInInspector]
    public bool isJumping;
    #endregion

    #region 스탯 (공통)
    [Header("스탯 관련 변수")]
    [Tooltip("체력")]
    [SerializeField]
    protected float hp_F;
    public float Hp_F
    {
        get { return hp_F; }
        set { hp_F = value; }
    }

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
        set { energy_F = value; }
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
        set { dreamyFigure_F = value; }
    }

    [Tooltip("최대 몽환 게이지")]
    [SerializeField]
    protected float maxDreamyFigure_F;
    public float MaxDreamyFigure_F
    {
        get { return maxDreamyFigure_F; }
        set { maxDreamyFigure_F = value; }
    }


    [Tooltip("공격력")]
    [SerializeField]
    protected int damage_I;
    public int Damage_I
    {
        get { return damage_I; }
        set { damage_I = value; }
    }

    [Tooltip("이동속도")]
    [SerializeField]
    protected float Speed_F;

    [HideInInspector]
    public bool isAttacking;
    #endregion

    [SerializeField]
    [Header("공격 범위 콜라이더")]
    [Tooltip("해당 오브젝트 공격 콜라이더")]
    protected GameObject attackRangeObj;

    [HideInInspector]
    public Vector2 startPos_Vector;

    protected Camera Cam;

    protected Rigidbody2D rigid;

    protected abstract void StartSetting();

    protected abstract void UISetting();

    protected abstract void Defense();

    protected abstract void SetDefensing(int defensingDirectionIndex, float setRotation);
}
