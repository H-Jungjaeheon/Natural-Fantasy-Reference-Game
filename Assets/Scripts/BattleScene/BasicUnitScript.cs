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
    #region ���� ��Ÿ�� ���� ���� (����)
    [Header("���� ��Ÿ�� ���� ����")]
    [Tooltip("��Ÿ�� �ٵ� ������Ʈ")]
    [SerializeField]
    protected GameObject actionCoolTimeObj;

    [Tooltip("��Ÿ�� �� �̹���(���)")]
    [SerializeField]
    protected Image nullActionCoolTimeImage;

    [Tooltip("��Ÿ�� �� �̹���")]
    [SerializeField]
    protected Image actionCoolTimeImage;

    [Tooltip("��Ÿ�� �� Y�� ��ġ ����")]
    [SerializeField]
    protected float actionCoolTimeImageYPos_F;

    protected float nowActionCoolTime; //���� ��Ÿ��

    [Tooltip("�ִ� ��Ÿ��")]
    public float maxActionCoolTime;

    protected bool isWaiting; //�����
    #endregion

    #region �ൿ ���� ����
    [Header("���� �ൿ ���� ����")]
    [Tooltip("�߷°�")]
    [SerializeField]
    protected float setJumpGravityScale_F;

    [Tooltip("���� �Ŀ�")]
    [SerializeField]
    protected float jumpPower_F;

    [Header("��� / ��� ��ġ ���� ����")]
    [Tooltip("���� ��� ��ġ")]
    public bool[] nowDefensivePosition_B;

    protected bool isDefensing;

    protected bool isDeflecting;

    [Header("���� ���� ���� �� ����Ʈ")]
    public List<GameObject> rangeInEnemy = new List<GameObject>();

    [Header("ƨ�ܳ��� ���� ���� ������Ʈ ����Ʈ")]
    public List<GameObject> rangeInDeflectAbleObj = new List<GameObject>();

    protected int nowAttackCount_I;

    [HideInInspector]
    public bool isJumping;
    #endregion

    #region ���� (����)
    [Header("���� ���� ����")]
    [Tooltip("ü��")]
    [SerializeField]
    protected float hp_F;
    public float Hp_F
    {
        get { return hp_F; }
        set { hp_F = value; }
    }

    [Tooltip("�ִ� ü��")]
    [SerializeField]
    protected float maxHp_F;

    public float MaxHp_F
    {
        get { return maxHp_F; }
        set { maxHp_F = value; }
    }

    [Tooltip("���")]
    [SerializeField]
    protected float energy_F;
    public float Energy_F
    {
        get { return energy_F; }
        set { energy_F = value; }
    }

    [Tooltip("�ִ� ���")]
    [SerializeField]
    protected float maxEnergy_F;
    public float MaxEnergy_F
    {
        get { return maxEnergy_F; }
        set { maxEnergy_F = value; }
    }

    [Tooltip("��ȯ ������ ���� �Ǻ�")]
    [SerializeField]
    protected bool isHaveDreamyFigure;

    [Tooltip("��ȯ ������")]
    [SerializeField]
    protected float dreamyFigure_F;
    public float DreamyFigure_F
    {
        get { return dreamyFigure_F; }
        set { dreamyFigure_F = value; }
    }

    [Tooltip("�ִ� ��ȯ ������")]
    [SerializeField]
    protected float maxDreamyFigure_F;
    public float MaxDreamyFigure_F
    {
        get { return maxDreamyFigure_F; }
        set { maxDreamyFigure_F = value; }
    }


    [Tooltip("���ݷ�")]
    [SerializeField]
    protected int damage_I;
    public int Damage_I
    {
        get { return damage_I; }
        set { damage_I = value; }
    }

    [Tooltip("�̵��ӵ�")]
    [SerializeField]
    protected float Speed_F;

    [HideInInspector]
    public bool isAttacking;
    #endregion

    [SerializeField]
    [Header("���� ���� �ݶ��̴�")]
    [Tooltip("�ش� ������Ʈ ���� �ݶ��̴�")]
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
