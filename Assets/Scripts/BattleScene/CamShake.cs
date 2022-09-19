using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
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

    [SerializeField]
    [Tooltip("부모 오브젝트의 리지드바디(점프 속도 제어용)")]
    private Rigidbody2D rigid;

    [SerializeField]
    private PostProcessVolume PPV;

    private Bloom bloom;
    private Vignette vignette;

    Vector3 initialPosition;
    Vector3 objStartPosition;
    Vector3 camStartposition;
    Vector3 zeroPosition = new Vector3(0,0,0);
    WaitForSeconds shakeDelay = new WaitForSeconds(0.03f);

    void Awake()
    {
        StartSetting();
        PPV.profile.TryGetSettings(out bloom);
        PPV.profile.TryGetSettings(out vignette);
    }

    void Update()
    {
        RepetitionSetting();
        if (Input.GetKeyDown(KeyCode.F1))
        {
            bloom.intensity.value += 10;
        }
    }

    public void StartSetting()
    {
        initialPosition = rigid.transform.position;
        objStartPosition = initialPosition;
        camStartposition = transform.position;
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
        initialPosition.y = rigid.transform.position.y;
    }

    private void CallingStartJump()
    {
        StartCoroutine(StartJump());
    }

    private void StopJump()
    {
        rigid.transform.position = objStartPosition;
        transform.position = camStartposition;
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
        int multiplication = -1;
        Vector3 nowCamPos = initialPosition;
        for (int nowShakeCount = 0; nowShakeCount < 11; nowShakeCount++)
        {
            nowCamPos.x = shakeAmount * multiplication;
            nowCamPos.y = (BattleSceneManager.Instance.Player.nowState == NowState.Jumping) ? initialPosition.y : objStartPosition.y;

            rigid.transform.position = nowCamPos;

            shakeAmount = (nowShakeCount == 0) ? shakeAmount -= shakeAmount / 1.25f : shakeAmount -= shakeAmount / 9;
            
            multiplication *= -1;
            yield return shakeDelay;
            rigid.transform.position = initialPosition;
        }
    }

    IEnumerator CamVerticalShake(float shakeAmount)
    {
        int multiplication = -1;
        Vector3 plusPos = zeroPosition;
        for (int nowShakeCount = 0; nowShakeCount < 11; nowShakeCount++)
        {
            plusPos.x = -(shakeAmount * multiplication);
            plusPos.y = shakeAmount * multiplication;

            transform.position += plusPos;

            shakeAmount = (nowShakeCount == 0) ? shakeAmount -= shakeAmount / 1.25f : shakeAmount -= shakeAmount / 9;

            multiplication *= -1;
            yield return shakeDelay;
            transform.position -= plusPos;
            plusPos = zeroPosition;
        }
        plusPos = initialPosition;
        transform.position = plusPos;
    }
}
