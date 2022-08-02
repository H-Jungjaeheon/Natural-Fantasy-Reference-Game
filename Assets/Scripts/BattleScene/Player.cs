using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    #region 공격 쿨타임 관련 변수
    [Header("공격 쿨타임 관련 변수")]
    [Tooltip("쿨타임 바들 오브젝트")]
    [SerializeField] private GameObject actionCoolTimeObj;

    [Tooltip("쿨타임 바 이미지(배경)")]
    [SerializeField] private Image nullActionCoolTimeImage;

    [Tooltip("쿨타임 바 이미지")]
    [SerializeField] private Image actionCoolTimeImage;

    [Tooltip("쿨타임 바 Y축 위치 조절")]
    [SerializeField] private float actionCoolTimeImageYPos;

    private float nowActionCoolTime; //현재 쿨타임
    private float maxActionCoolTime; //최대 쿨타임
    private bool isWaiting; //대기중
    #endregion

    #region 행동 관련 변수
    [Header("행동 버튼 관련 변수")]
    [Tooltip("행동 버튼들 오브젝트")]
    [SerializeField] private GameObject ActionButtonsObj;

    [Header("현재 행동 관련 변수")]
    [Tooltip("중력값")]
    [SerializeField] private float SetJumpGravityScale;

    [Tooltip("점프 파워")]
    [SerializeField] private float JumpPower;

    private bool IsJumping;
    #endregion

    [HideInInspector]
    public Vector2 StartPos;

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
        StartPos = (Vector2)transform.position;
        maxActionCoolTime = 3.5f - GameManager.Instance.ReduceCoolTimeLevel * 0.5f;
        BattleSceneManager.Instance.PlayerCharacterCloseRangeAttackPos = new Vector2(-9, -1.4f);
    }
    private void UISetting() //대기시간 및 UI세팅 (공통)
    {
        if (isWaiting)
        {
            nullActionCoolTimeImage.transform.position = Cam.WorldToScreenPoint(transform.position + new Vector3(0, actionCoolTimeImageYPos, 0));
            actionCoolTimeImage.fillAmount = nowActionCoolTime / maxActionCoolTime;
            nowActionCoolTime += Time.deltaTime;
            if (nowActionCoolTime >= maxActionCoolTime)
            {
                isWaiting = false;
                actionCoolTimeObj.SetActive(false);
            }
        }
        if (isWaiting == false && IsJumping == false)
        {
            ActionButtonsOn();
        }
    }
    private void WaitingTimeStart() //공격 후의 세팅 (공통) 
    {
        nowActionCoolTime = 0;
        isWaiting = true;
        actionCoolTimeObj.SetActive(true);
        ActionButtonsOff();
    }
    private void Jump() 
    {
        if (IsJumping == false && Input.GetKey(KeyCode.Space))
        {
            IsJumping = true;
            ActionButtonsOff();
            rigid.AddForce(Vector2.up * JumpPower, ForceMode2D.Impulse);
            rigid.gravityScale = SetJumpGravityScale - 0.5f;
            StartCoroutine(JumpDelay());
        }
        else if (IsJumping && transform.position.y < StartPos.y)
        {
            IsJumping = false;
            transform.position = StartPos;
            rigid.velocity = Vector2.zero;
            rigid.gravityScale = 0;
        }
    }
    IEnumerator JumpDelay()
    {
        yield return new WaitForSeconds(0.3f);
        while (rigid.gravityScale >= 0.2f)
        {
            rigid.gravityScale -= Time.deltaTime * 2.5f;
            yield return null;
        }
        rigid.gravityScale = SetJumpGravityScale * 1.5f;
    }
    private void ActionButtonsOff() => ActionButtonsObj.SetActive(false);
    private void ActionButtonsOn() => ActionButtonsObj.SetActive(true);
}
