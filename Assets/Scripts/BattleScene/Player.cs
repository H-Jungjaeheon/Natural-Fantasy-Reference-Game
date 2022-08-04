using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    #region ���� ��Ÿ�� ���� ���� (����)
    [Header("���� ��Ÿ�� ���� ����")]
    [Tooltip("��Ÿ�� �ٵ� ������Ʈ")]
    [SerializeField]
    private GameObject actionCoolTimeObj;

    [Tooltip("��Ÿ�� �� �̹���(���)")]
    [SerializeField]
    private Image nullActionCoolTimeImage;

    [Tooltip("��Ÿ�� �� �̹���")]
    [SerializeField]
    private Image actionCoolTimeImage;

    [Tooltip("��Ÿ�� �� Y�� ��ġ ����")]
    [SerializeField]
    private float actionCoolTimeImageYPos_F;

    private float nowActionCoolTime; //���� ��Ÿ��
    private float maxActionCoolTime; //�ִ� ��Ÿ��
    private bool isWaiting; //�����
    #endregion

    #region �ൿ ���� ���� (�Ϻ� ����)
    [Header("�ൿ ��ư ���� ����")]
    [Tooltip("�ൿ ��ư�� ������Ʈ")]
    [SerializeField]
    private GameObject actionButtonsObj;

    [Header("���� �ൿ ���� ����")]
    [Tooltip("�߷°�")]
    [SerializeField]
    private float setJumpGravityScale_F;

    [Tooltip("���� �Ŀ�")]
    [SerializeField]
    private float jumpPower_F;

    [HideInInspector]
    public bool isJumping;
    #endregion

    #region ���� (����)
    [Header("���� ���� ����")]
    [Tooltip("ü��")]
    [SerializeField]
    private float hp_F;
    public float Hp_F
    {
        get { return hp_F; }
        set { hp_F = value; }
    }

    private float MaxHp_F; //�ִ� ü��

    [Tooltip("���")]
    [SerializeField]
    private int energy_I;
    public int Energy_I
    {
        get { return energy_I; }
        set { energy_I = value; }
    }

    private int MaxEnergy_I; //�ִ� ���

    [Tooltip("��ȯ ������")]
    [SerializeField]
    private int dreamyFigure_I;
    public int DreamyFigure_I
    {
        get { return dreamyFigure_I; }
        set { dreamyFigure_I = value; }
    }

    [Tooltip("�ִ� ��ȯ ������")]
    [SerializeField]
    private int MaxDreamyFigure_I;

    [Tooltip("���ݷ�")]
    [SerializeField]
    private int damage_I;
    public int Damage_I
    {
        get { return damage_I; }
        set { damage_I = value; }
    }

    [Tooltip("�̵��ӵ�")]
    [SerializeField]
    private float Speed_F;

    [HideInInspector]
    public bool isAttacking;
    #endregion

    [HideInInspector]
    public Vector2 startPos_Vector;

    Camera Cam;
    Rigidbody2D rigid;

    private void Awake()
    {
        StartSetting();
    }

    // Start is called before the first frame update
    void Start()
    {
        nowActionCoolTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        UISetting();
        Jump();
    }
    private void StartSetting() //�ʱ� ���� (����)
    {
        Cam = Camera.main;
        rigid = gameObject.GetComponent<Rigidbody2D>();
        isWaiting = true;
        startPos_Vector = transform.position;
        maxActionCoolTime = 3.5f - GameManager.Instance.ReduceCoolTimeLevel * 0.5f;
        MaxHp_F = 25 + GameManager.Instance.MaxHpUpgradeLevel * 5;
        Hp_F = MaxHp_F;
        MaxEnergy_I = 15 + GameManager.Instance.MaxEnergyUpgradeLevel * 5;
        Energy_I = MaxEnergy_I;
        Damage_I = 2 + GameManager.Instance.DamageUpgradeLevel;
        BattleSceneManager.Instance.PlayerCharacterCloseRangeAttackPos = new Vector2(-9, -1.4f);
    }
    private void UISetting() //���ð� �� UI���� (����)
    {
        if (isWaiting)
        {
            nullActionCoolTimeImage.transform.position = Cam.WorldToScreenPoint(transform.position + new Vector3(0, actionCoolTimeImageYPos_F, 0));
            actionCoolTimeImage.fillAmount = nowActionCoolTime / maxActionCoolTime;
            nowActionCoolTime += Time.deltaTime;
            if (nowActionCoolTime >= maxActionCoolTime)
            {
                isWaiting = false;
                actionCoolTimeObj.SetActive(false);
            }
        }
        if (isWaiting == false && isJumping == false && isAttacking == false)
        {
            ActionButtonsSetActive(true);
        }
        else
        {
            ActionButtonsSetActive(false);
        }
    }
    private void WaitingTimeStart() //���� ���� ���� (����) 
    {
        nowActionCoolTime = 0;
        isWaiting = true;
        actionCoolTimeObj.SetActive(true);
        ActionButtonsSetActive(false);
    }
    private void Jump() 
    {
        if (isJumping == false && isAttacking == false && Input.GetKey(KeyCode.Space))
        {
            isJumping = true;
            ActionButtonsSetActive(false);
            rigid.AddForce(Vector2.up * jumpPower_F, ForceMode2D.Impulse);
            rigid.gravityScale = setJumpGravityScale_F - 0.5f;
            StartCoroutine(JumpDelay());
        }
        else if (isJumping && transform.position.y < startPos_Vector.y)
        {
            isJumping = false;
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
        rigid.gravityScale = setJumpGravityScale_F * 1.5f;
    }

    public void CloseAttackStart()
    {
        isAttacking = true;
        ActionButtonsSetActive(false);
        StartCoroutine(CloseAttack());
    }

    IEnumerator CloseAttack()
    {
        WaitForSeconds WFS = new WaitForSeconds(3);
        while (transform.position.x == BattleSceneManager.Instance.PlayerCharacterCloseRangeAttackPos.x)
        {
            
        }
        yield return WFS;

        isAttacking = false;
        WaitingTimeStart();
    }

    private void ActionButtonsSetActive(bool SetActive) => actionButtonsObj.SetActive(SetActive);
}
