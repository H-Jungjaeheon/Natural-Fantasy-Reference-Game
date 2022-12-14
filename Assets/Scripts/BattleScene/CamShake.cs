using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CamShake : MonoBehaviour
{
    public static Action<bool, float> CamShakeMod;
    public static Action<bool> JumpStop;
    public static Action JumpStart;

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

    private IEnumerator startJumpCoroutine; //점프 시작할 때 카메라에게 물리 효과를 주는 코루틴

    private IEnumerator nowShakeCoroutine; //카메라에게 흔들림 효과를 주는 코루틴

    Vector3 initialPosition;

    Vector3 objStartPosition;

    Vector3 camStartposition;

    Vector3 zeroPosition = new Vector3(0, 0, 0);

    WaitForSeconds shakeDelay = new WaitForSeconds(0.03f);

    private bool isGameClear;

    void Awake()
    {
        StartSetting();
        PPV.profile.TryGetSettings(out bloom);
        PPV.profile.TryGetSettings(out vignette);
    }

    /// <summary>
    /// 시작 세팅
    /// </summary>
    public void StartSetting()
    {
        initialPosition = rigid.transform.position;
        objStartPosition = initialPosition;
        camStartposition = transform.position;
        CamShakeMod = CamShakeStart;
        JumpStart = CallingStartJump;
        JumpStop = StopJump;

        StartCoroutine(RepetitionSetting());

        nowShakeCoroutine = CamHorizontalShake(0); //초기화
    }

    /// <summary>
    /// 게임 종료 세팅
    /// </summary>
    public void GameEndSetting()
    {
        StartCoroutine(GameEndCamAnim());
        isGameClear = true;
    }

    /// <summary>
    /// 게임 종료 카메라 움직임 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator GameEndCamAnim()
    {
        Vector3 bossPos = BattleSceneManager.Instance.enemy.transform.position;

        bossPos.y -= 4.6f;
        bossPos.z = -10;

        initialPosition = bossPos;

        while (true)
        {
            transform.position = Vector3.Lerp(transform.position, bossPos, 5 * Time.deltaTime);
            yield return null;
        }
    }

    /// <summary>
    /// 카메라 흔들림 효과 함수
    /// </summary>
    /// <param name="isHorizontalShake"> 가로로 흔들림 판별(false이면 세로로 흔들림) </param>
    /// <param name="timeInput"> 흔들림 효과 시간 </param>
    private void CamShakeStart(bool isHorizontalShake, float timeInput)
    {
        if (isGameClear == false)
        {
            StopCoroutine(nowShakeCoroutine);
            transform.position = initialPosition;

            if (isHorizontalShake)
            {
                nowShakeCoroutine = CamHorizontalShake(timeInput);
            }
            else
            {
                nowShakeCoroutine = CamVerticalShake(timeInput);
            }

            StartCoroutine(nowShakeCoroutine);
        }
    }

    /// <summary>
    /// 실시간 카메라 Y좌표 업데이트 함수
    /// </summary>
    /// <returns></returns>
    private IEnumerator RepetitionSetting()
    {
        while (true)
        {
            initialPosition.y = rigid.transform.position.y;
            if (isGameClear)
            {
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 점프 시작 시 카메라 움직임 호출 함수
    /// </summary>
    private void CallingStartJump()
    {
        startJumpCoroutine = StartJump();
        StartCoroutine(startJumpCoroutine);
    }

    /// <summary>
    /// 점프 종료 시 카메라 움직임
    /// </summary>
    /// <param name="isStopJumpCoroutine"> 점프 시작 효과 코루틴 종료 요청 </param>
    private void StopJump(bool isStopJumpCoroutine)
    {
        if (isStopJumpCoroutine && startJumpCoroutine != null)
        {
            StopCoroutine(startJumpCoroutine);
        }

        rigid.transform.position = objStartPosition;
        transform.position = camStartposition;
        rigid.velocity = Vector2.zero;
        rigid.gravityScale = 0;
    }

    /// <summary>
    /// 점프 시작 시 카메라 움직임
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 카메라 가로 흔들림 효과 함수
    /// </summary>
    /// <param name="shakeAmount"> 흔들림 강도 </param>
    /// <returns></returns>
    IEnumerator CamHorizontalShake(float shakeAmount) 
    {
        int multiplication = -1;
        Vector3 nowCamPos = initialPosition;

        for (int nowShakeCount = 0; nowShakeCount < 11; nowShakeCount++)
        {
            nowCamPos.x = shakeAmount * multiplication;
            nowCamPos.y = (BattleSceneManager.Instance.player.nowState == NowState.Jumping) ? initialPosition.y : objStartPosition.y;

            rigid.transform.position = nowCamPos;

            shakeAmount = (nowShakeCount == 0) ? shakeAmount -= shakeAmount / 1.25f : shakeAmount -= shakeAmount / 9;
            
            multiplication *= -1;
            yield return shakeDelay;
            rigid.transform.position = initialPosition;
        }

        transform.position = initialPosition;
    }

    /// <summary>
    /// 카메라 세로 흔들림 효과 함수
    /// </summary>
    /// <param name="shakeAmount"> 흔들림 강도 </param>
    /// <returns></returns>
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

        transform.position = initialPosition;
    }

    //if (Input.GetKeyDown(KeyCode.F1))
    //{
    //    bloom.intensity.value += 10;
    //}
}
