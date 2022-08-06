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

    [Tooltip("현재 공격 범위에 존재하는 적")]
    public List<GameObject> rangeInEnemy = new List<GameObject>();

    private bool isComplete;
    private int nowAttackCount_I;

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

    [Tooltip("최대 체력")]
    [SerializeField]
    private float maxHp_F;

    public float MaxHp_F
    {
        get { return maxHp_F; }
        set { maxHp_F = value; }
    }

    [Tooltip("기력")]
    [SerializeField]
    private float energy_F;
    public float Energy_F
    {
        get { return energy_F; }
        set { energy_F = value; }
    }

    [Tooltip("최대 기력")]
    [SerializeField]
    private float maxEnergy_F; 
    public float MaxEnergy_F
    {
        get { return maxEnergy_F; }
        set { maxEnergy_F = value; }
    }


    [Tooltip("몽환 게이지")]
    [SerializeField]
    private float dreamyFigure_F;
    public float DreamyFigure_F
    {
        get { return dreamyFigure_F; }
        set { dreamyFigure_F = value; }
    }

    [Tooltip("최대 몽환 게이지")]
    [SerializeField]
    private float maxDreamyFigure_F;
    public float MaxDreamyFigure_F
    {
        get { return maxDreamyFigure_F; }
        set { maxDreamyFigure_F = value; }
    }


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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var camComponent = Cam.GetComponent<CamShake>();
            camComponent.CamShakeStart(0.3f, 0.5f);
        }
    }
    private void StartSetting() //초기 세팅 (일부 공통)
    {
        Cam = Camera.main;
        rigid = gameObject.GetComponent<Rigidbody2D>();
        startPos_Vector = transform.position;
        maxActionCoolTime = 3.5f - GameManager.Instance.ReduceCoolTimeLevel * 0.5f;
        MaxHp_F = 25 + GameManager.Instance.MaxHpUpgradeLevel * 5;
        Hp_F = MaxHp_F;
        MaxEnergy_F = 15 + GameManager.Instance.MaxEnergyUpgradeLevel * 5;
        Energy_F = MaxEnergy_F;
        MaxDreamyFigure_F = 20;
        Damage_I = 1 + GameManager.Instance.DamageUpgradeLevel;
        nowAttackCount_I = 1;
        BattleSceneManager.Instance.PlayerCharacterPos = transform.position;
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
        StartCoroutine(GoToAttack());
    }

    IEnumerator GoToAttack()
    {
        Vector3 Movetransform = new Vector3(Speed_F * Time.deltaTime, 0, 0); //이동을 위해 더해줄 연산
        Vector3 Targettransform = new Vector3(BattleSceneManager.Instance.EnemyCharacterPos.x - 5, transform.position.y); //목표 위치

        while (transform.position.x < Targettransform.x) //이동중
        {
            transform.position += Movetransform;
            yield return null;
        }
        transform.position = Targettransform; //이동 완료

        StartCoroutine(EnemyAttack(false, nowAttackCount_I, 0.3f)); //첫번째 공격
    }

    IEnumerator EnemyAttack(bool isLastAttack, int nowAttackCount_I, float attacktimelimit_f) //3연공 재귀로 구현
    {
        isComplete = false;
        var camComponent = Cam.GetComponent<CamShake>();
        if (rangeInEnemy[0] != null)
        {
            print($"공격 {nowAttackCount_I}"); //공격 실행(애니메이션)
            switch (nowAttackCount_I)
            {
                case 1 :
                    camComponent.CamShakeStart(0.3f, 0.5f);
                    break;
                case 3:
                    camComponent.CamShakeStart(0.3f, 1);
                    break;
            }
            for (int nowIndex = 0; nowIndex < rangeInEnemy.Count; nowIndex++)
            {
                if (rangeInEnemy[nowIndex] != null)
                {
                    rangeInEnemy[nowIndex].GetComponent<Enemy>().hp -= Damage_I;
                }
            }
        }

        if (isLastAttack == false)
        {
            float nowattacktime_f = 0;
            print("눌러");
            while (attacktimelimit_f > nowattacktime_f) //기본공격 타이밍 계산
            {
                nowattacktime_f += Time.deltaTime;
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    isComplete = true;
                }
                yield return null;
            }

            if (isComplete)
            {
                switch (nowAttackCount_I)
                {
                    case 1:
                        yield return new WaitForSeconds(0.1f);
                        nowAttackCount_I++;
                        StartCoroutine(EnemyAttack(false, nowAttackCount_I, 0.3f)); //두번째 공격
                        break;
                    case 2:
                        yield return new WaitForSeconds(0.5f);
                        nowAttackCount_I++;
                        StartCoroutine(EnemyAttack(true, nowAttackCount_I, 0.3f)); //세번째 공격
                        break;
                }
            }
            else //돌아가기
            {
                StartCoroutine(Return());
            }
        }
        else //돌아가기
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(Return());
        }
    }

    IEnumerator Return()
    {
        Vector3 Movetransform = new Vector3(Speed_F * Time.deltaTime, 0, 0); //이동을 위해 더해줄 연산
        transform.rotation = Quaternion.Euler(0, 180, 0);
        while (transform.position.x > startPos_Vector.x)
        {
            transform.position -= Movetransform;
            yield return null;
        }
        transform.rotation = Quaternion.identity;
        transform.position = startPos_Vector;
        nowAttackCount_I = 1;
        isAttacking = false;
        WaitingTimeStart();
    }

    private void ActionButtonsSetActive(bool SetActive) => actionButtonsObj.SetActive(SetActive);
}
