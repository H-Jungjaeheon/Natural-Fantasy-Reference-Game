using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CamShake : MonoBehaviour
{
    public static Action<bool, float> CamShakeMod;
    public static Action JumpStart;
    public static Action JumpStop;

    [SerializeField]
    [Tooltip("플레이어 점프 시 카메라 올라가는 속도")]
    private float jumpCamPower_F;

    [SerializeField]
    [Tooltip("플레이어 착지 중 카메라 떨어지는 속도")]
    private float setJumpCamGravityScale_F;

    private bool isVerticalShaking;

    [SerializeField]
    [Tooltip("부모 오브젝트의 리지드바디(점프 속도 제어용)")]
    private Rigidbody2D rigid;

    Vector3 initialPosition;
    Vector3 startPosition;
    WaitForSeconds shakeDelay = new WaitForSeconds(0.03f);

    void Awake()
    {
        StartSetting();
    }

    void Update()
    {
        RepetitionSetting();
        if (Input.GetKeyDown(KeyCode.H))
        {
            StartCoroutine(CamVerticalShake(1.5f));
        }
    }

    public void StartSetting()
    {
        initialPosition = rigid.transform.position;
        startPosition = rigid.transform.position;
        CamShakeMod = CamShakeStart;
        JumpStart = CallingStartJump;
        JumpStop = StopJump;
    }

    public void CamShakeStart(bool isHorizontalShake, float timeInput)
    {
        if (isHorizontalShake)
        {
            StartCoroutine(CamHorizontalShake(timeInput));
        }
        else
        {
            StartCoroutine(CamVerticalShake(timeInput));
        }
    }

    private void RepetitionSetting()
    {
        if (isVerticalShaking == false)
        {
            initialPosition.y = rigid.transform.position.y;
            rigid.transform.position = initialPosition;
        }
    }

    private void CallingStartJump()
    {
        StartCoroutine(StartJump());
    }

    private void StopJump()
    {
        rigid.transform.position = startPosition;
        rigid.velocity = Vector2.zero;
        rigid.gravityScale = 0;
    }

    private IEnumerator StartJump()
    {
        rigid.AddForce(Vector2.up * jumpCamPower_F, ForceMode2D.Impulse);
        rigid.gravityScale = setJumpCamGravityScale_F; 
        yield return new WaitForSeconds(0.7f);
        while (rigid.gravityScale >= 0.2f)
        {
            rigid.gravityScale -= Time.deltaTime * 3.5f;
            yield return null;
        }
        rigid.gravityScale = 1.5f;
    }

    IEnumerator CamHorizontalShake(float shakeAmount)
    {
        int multiplication = 1;
        Vector3 nowCamPos = initialPosition;
        for (int nowShakeCount = 0; nowShakeCount < 9; nowShakeCount++)
        {
            nowCamPos.x = shakeAmount * multiplication;
            rigid.transform.position = nowCamPos;

            shakeAmount = (nowShakeCount == 1) ? shakeAmount -= shakeAmount / 1.5f : shakeAmount -= shakeAmount / 10;
            
            multiplication *= -1;
            yield return shakeDelay;
        }
    }

    IEnumerator CamVerticalShake(float shakeAmount)
    {
        int multiplication = -1;
        Vector3 nowCamPos = initialPosition;
        Vector3 plusPos = startPosition;
        isVerticalShaking = true;

        for (int nowShakeCount = 0; nowShakeCount < 11; nowShakeCount++)
        {
            plusPos.y = shakeAmount * -multiplication;
            nowCamPos.x = shakeAmount * multiplication;
            rigid.transform.position = nowCamPos;
            rigid.transform.position += plusPos;
            shakeAmount = (nowShakeCount == 1) ? shakeAmount -= shakeAmount / 1.2f : shakeAmount -= shakeAmount / 8.5f;

            yield return shakeDelay;
            multiplication *= -1;
            rigid.transform.position -= plusPos;
        }
        isVerticalShaking = false;
        if (BattleSceneManager.Instance.Player.isJumping == false)
        {
            rigid.transform.position = startPosition;
        }
        else 
        {
            rigid.transform.position = initialPosition;
        }
    }
}
