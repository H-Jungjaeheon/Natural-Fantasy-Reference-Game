using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    #region ���� ��Ÿ�� ���� ����
    [Header("���� ��Ÿ�� ���� ����")]
    [Tooltip("��Ÿ�� �ٵ� ������Ʈ")]
    [SerializeField] private GameObject actionCoolTimeObj;

    [Tooltip("��Ÿ�� �� �̹���(���)")]
    [SerializeField] private Image nullActionCoolTimeImage;

    [Tooltip("��Ÿ�� �� �̹���")]
    [SerializeField] private Image actionCoolTimeImage;

    [Tooltip("��Ÿ�� �� Y�� ��ġ ����")]
    [SerializeField] private float actionCoolTimeImageYPos;

    private float nowActionCoolTime; //���� ��Ÿ��
    private float maxActionCoolTime; //�ִ� ��Ÿ��
    private bool isWaiting; //�����
    #endregion

    #region �ൿ ���� ����
    [Header("�ൿ ��ư ���� ����")]
    [Tooltip("�ൿ ��ư�� ������Ʈ")]
    [SerializeField] private GameObject ActionButtonsObj;

    [Header("���� �ൿ ���� ����")]
    [Tooltip("�߷°�")]
    [SerializeField] private float SetJumpGravityScale;

    [Tooltip("���� �Ŀ�")]
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
    private void StartSetting() //�ʱ� ���� (����)
    {
        Cam = Camera.main;
        rigid = gameObject.GetComponent<Rigidbody2D>();
        isWaiting = true;
        StartPos = (Vector2)transform.position;
        maxActionCoolTime = 3.5f - GameManager.Instance.ReduceCoolTimeLevel * 0.5f;
        BattleSceneManager.Instance.PlayerCharacterCloseRangeAttackPos = new Vector2(-9, -1.4f);
    }
    private void UISetting() //���ð� �� UI���� (����)
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
    private void WaitingTimeStart() //���� ���� ���� (����) 
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
