using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    #region 공격 쿨타임 관련 변수 (공통)
    [Header("공격 쿨타임 관련 변수")]
    [Tooltip("쿨타임 바들 오브젝트")]
    [SerializeField]
    private GameObject actionCoolTimeObj;

    [Tooltip("쿨타임 바 이미지(배경)")]
    [SerializeField]
    private Image nullActionCoolTimeImage;

    [Tooltip("쿨타임 바 이미지")]
    [SerializeField]
    private Image actionCoolTimeImage;

    [Tooltip("쿨타임 바 Y축 위치 조절")]
    [SerializeField]
    private float actionCoolTimeImageYPos_F;

    private float nowActionCoolTime; //현재 쿨타임
    private float maxActionCoolTime; //최대 쿨타임
    private bool isWaiting; //대기중
    #endregion

    #region 행동 관련 변수 (일부 공통)
    [Header("행동 버튼 관련 변수")]
    [Tooltip("행동 버튼들 오브젝트")]
    [SerializeField]
    private GameObject actionButtonsObj;

    [Header("현재 행동 관련 변수")]
    [Tooltip("중력값")]
    [SerializeField]
    private float setJumpGravityScale_F;

    [Tooltip("점프 파워")]
    [SerializeField]
    private float jumpPower_F;

    [HideInInspector]
    public bool isJumping;
    #endregion

    #region 스탯 (공통)
    [Header("스탯 관련 변수")]
    [Tooltip("체력")]
    [SerializeField]
    private float hp_F;
    public float Hp_F
    {
        get { return hp_F; }
        set { hp_F = value; }
    }

    private float MaxHp_F; //최대 체력

    [Tooltip("기력")]
    [SerializeField]
    private int energy_I;
    public int Energy_I
    {
        get { return energy_I; }
        set { energy_I = value; }
    }

    private int MaxEnergy_I; //최대 기력

    [Tooltip("몽환 게이지")]
    [SerializeField]
    private int dreamyFigure_I;
    public int DreamyFigure_I
    {
        get { return dreamyFigure_I; }
        set { dreamyFigure_I = value; }
    }

    [Tooltip("최대 몽환 게이지")]
    [SerializeField]
    private int MaxDreamyFigure_I;

    [Tooltip("공격력")]
    [SerializeField]
    private int damage_I;
    public int Damage_I
    {
        get { return damage_I; }
        set { damage_I = value; }
    }

    [Tooltip("이동속도")]
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
    private void StartSetting() //초기 세팅 (공통)
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
    private void UISetting() //대기시간 및 UI세팅 (공통)
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
    private void WaitingTimeStart() //공격 후의 세팅 (공통) 
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
