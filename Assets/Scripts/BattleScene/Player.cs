using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : BasicUnitScript
{
    [Header("행동 버튼 관련 변수")]
    [Tooltip("행동 버튼들 오브젝트")]
    [SerializeField]
    protected GameObject actionButtonsObj;

    private void Awake()
    {
        StartSetting();
    }

    
    void FixedUpdate()
    {
        UISetting();
        Jump();
        Defense();
        Deflect();
    }

    protected override void StartSetting() //초기 세팅 (일부 공통)
    {
        Cam = Camera.main;
        rigid = gameObject.GetComponent<Rigidbody2D>();
        startPos_Vector = transform.position;
        maxActionCoolTime -= GameManager.Instance.ReduceCoolTimeLevel * 0.5f;
        MaxHp_F += GameManager.Instance.MaxHpUpgradeLevel * 5;
        Hp_F = MaxHp_F;
        MaxEnergy_F += GameManager.Instance.MaxEnergyUpgradeLevel * 5;
        Energy_F = MaxEnergy_F;
        Damage_I += GameManager.Instance.DamageUpgradeLevel;
        nowAttackCount_I = 1;
        BattleSceneManager.Instance.PlayerCharacterPos = transform.position;
    }

    protected override void Defense()
    {
        //print($"현재 방어중? {isDefensing}");
        //print($"현재 튕겨내기중? {isDeflecting}");
        if (isDefensing == false && isDeflecting == false)
        {
            //print("방어 조건 충족");
            if (isJumping == false && isAttacking == false && Input.GetKey(KeyCode.A))
            {
                SetDefensing((int)NowDefensePos.Left, 180);
                //방어 애니메이션
            }
            else if (isJumping == false && isAttacking == false && Input.GetKey(KeyCode.D))
            {
                SetDefensing((int)NowDefensePos.Right, 0);
                //방어 애니메이션
            }
            else if (isJumping == false && isAttacking == false && Input.GetKey(KeyCode.W))
            {
                SetDefensing((int)NowDefensePos.Up, 0);
                //방어 애니메이션
            }
        }
        else if (isDefensing && isDeflecting == false && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            isDefensing = false;
            if (isDeflecting == false)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            for (int nowDefensePosIndex = 0; nowDefensePosIndex < (int)NowDefensePos.DefensePosCount; nowDefensePosIndex++)
            {
                nowDefensivePosition_B[nowDefensePosIndex] = false;
            }
        }
    }

    private void Deflect()
    {
        if (isDefensing)
        {
            if (nowDefensivePosition_B[(int)NowDefensePos.Right] == true && isDeflecting == false && Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(Deflecting((int)NowDefensePos.Right, 0));
            }
            else if (nowDefensivePosition_B[(int)NowDefensePos.Left] == true && isDeflecting == false && Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(Deflecting((int)NowDefensePos.Left, 180));
            }
        }
    }

    IEnumerator Deflecting(int nowDefensePosIndex, int setRotation)
    {
        var attackRangeObjComponent = attackRangeObj.GetComponent<BoxCollider2D>();
        isDeflecting = true;
        nowDefensivePosition_B[nowDefensePosIndex] = false;
        ActionButtonsSetActive(false);
        transform.rotation = Quaternion.Euler(0, setRotation, 0);
        attackRangeObjComponent.size = new Vector2(0.55f, 2.1f);
        attackRangeObjComponent.offset = new Vector2(-0.1f, 0f);
        //애니 실행
        yield return new WaitForSeconds(2); //치기 전까지 기다림
        //범위 내의 반사 가능한 오브젝트 상호작용
        print("상호작용 타이밍");
        yield return new WaitForSeconds(2); //애니메이션 종료까지 기다림
        attackRangeObjComponent.size = new Vector2(0.8f, 2.1f);
        attackRangeObjComponent.offset = new Vector2(0f, 0f);
        ActionButtonsSetActive(true);
        isDeflecting = false;
        if (nowActionCoolTime < maxActionCoolTime)
        {
            ActionCoolTimeBarSetActive(true);
        }
        if (!Input.GetKey(KeyCode.A))
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        isDefensing = false;
        yield return null;
    }

    protected override void SetDefensing(int defensingDirectionIndex, float setRotation)
    {
        isDefensing = true;
        nowDefensivePosition_B[defensingDirectionIndex] = true;
        transform.rotation = Quaternion.Euler(0, setRotation, 0);
    }

    protected override void UISetting() //대기시간 및 UI세팅 (일부 공통)
    {
        if (isWaiting && isDeflecting == false)
        {
            actionCoolTimeImage.fillAmount = nowActionCoolTime / maxActionCoolTime;
            nowActionCoolTime += Time.deltaTime;
            if (nowActionCoolTime >= maxActionCoolTime)
            {
                isWaiting = false;
                ActionCoolTimeBarSetActive(false);
            }
        }
        if (isWaiting == false && isJumping == false && isAttacking == false && isDeflecting == false)
        {
            ActionCoolTimeBarSetActive(false);
            ActionButtonsSetActive(true);
        }
        else
        {
            if (isDeflecting == true)
            {
                ActionCoolTimeBarSetActive(false);
            }
            ActionButtonsSetActive(false);
        }
        nullActionCoolTimeImage.transform.position = Cam.WorldToScreenPoint(transform.position + new Vector3(0, actionCoolTimeImageYPos_F, 0));
    }

    private void WaitingTimeStart() //공격 후의 세팅 (일부 공통) 
    {
        nowActionCoolTime = 0;
        isWaiting = true;
        ActionCoolTimeBarSetActive(true);
        ActionButtonsSetActive(false);
    }

    private void Jump() 
    {
        if (isJumping == false && isAttacking == false && isDefensing == false && isDeflecting == false && Input.GetKey(KeyCode.Space))
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
        Vector3 Movetransform = new Vector3(Speed_F, 0, 0); //이동을 위해 더해줄 연산
        Vector3 Targettransform = new Vector3(BattleSceneManager.Instance.EnemyCharacterPos.x - 5, transform.position.y); //목표 위치

        while (transform.position.x < Targettransform.x) //이동중
        {
            transform.position += Movetransform * Time.deltaTime;
            yield return null;
        }
        transform.position = Targettransform; //이동 완료

        StartCoroutine(Attacking(false, nowAttackCount_I, 0.2f)); //첫번째 공격 실행
    }

    IEnumerator Attacking(bool isLastAttack, int nowAttackCount_I, float delayTime) //3연공 재귀로 구현
    {
        bool isComplete = false;
        bool isFail = false;
        var camComponent = Cam.GetComponent<CamShake>();
        float nowdelayTime = 0;
        float nowattacktime_f = 0;
        
        while (nowdelayTime < delayTime) //연타 방지용
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isFail = true;
            }
            nowdelayTime += Time.deltaTime;
            yield return null;
        }

        if (rangeInEnemy[0] != null) //기본공격 실행 함수 및 기본공격 애니메이션 시작
        {
            switch (nowAttackCount_I) 
            {
                case 1:
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
                    rangeInEnemy[nowIndex].GetComponent<BasicUnitScript>().Hp_F -= Damage_I;
                }
            }
        }

        while (0.2f > nowattacktime_f) //연공 타이밍 계산
        {
            nowattacktime_f += Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isComplete = true;
            }
            yield return null;
        }

        if (isLastAttack == false && isFail == false && isComplete) 
        {
            nowAttackCount_I++;
            switch (nowAttackCount_I) //공격 실행 애니메이션 시작
            {
                case 2:
                    StartCoroutine(Attacking(false, nowAttackCount_I, 0.17f)); //두번째 공격
                    break;
                case 3:
                    StartCoroutine(Attacking(true, nowAttackCount_I, 0.5f)); //세번째 공격
                    break;
            }
        }
        else //돌아가기
        {
            if (isLastAttack == true)
            {
                yield return new WaitForSeconds(0.5f);
            }
            StartCoroutine(Return());
        }
    }

    IEnumerator Return() //근접공격 후 돌아오기
    {
        Vector3 Movetransform = new Vector3(Speed_F, 0, 0);
        transform.rotation = Quaternion.Euler(0, 180, 0);
        while (transform.position.x > startPos_Vector.x)
        {
            transform.position -= Movetransform * Time.deltaTime;
            yield return null;
        }
        transform.rotation = Quaternion.identity;
        transform.position = startPos_Vector;
        nowAttackCount_I = 1;
        isAttacking = false;
        WaitingTimeStart();
    }

    private void ActionButtonsSetActive(bool SetActive) => actionButtonsObj.SetActive(SetActive);

    private void ActionCoolTimeBarSetActive(bool SetActive) => actionCoolTimeObj.SetActive(SetActive);
}
