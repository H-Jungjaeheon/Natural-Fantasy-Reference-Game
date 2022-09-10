using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CamShake : MonoBehaviour
{
    public static Action<float, float> NowCamShakeStart;
    public static Action JumpStart;
    public static Action JumpStop;

    Vector3 startCamPosition;
    Vector3 initialPosition;
    WaitForSeconds shakeDelay = new WaitForSeconds(0.06f);

    [SerializeField]
    [Tooltip("부모 오브젝트의 리지드바디(점프 가속도용)")]
    private Rigidbody2D rigid;

    void Awake()
    {
        StartSetting();
    }

    void Update()
    {
        CamShaking();
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(TestCamShake(1f));
        }
    }


    public void StartSetting()
    {
        startCamPosition = rigid.transform.position;
        NowCamShakeStart = CamShakeStart;
        JumpStart = CallingStartJump;
        JumpStop = StopJump;
        initialPosition.x = transform.position.x;
        initialPosition.z = transform.position.z;
    }
    public void CamShakeStart(float timeInput, float shakeAmountInput)
    {

    }

    private void CamShaking()
    {
        initialPosition.y = transform.position.y;
        transform.position = initialPosition;
    }

    private void CallingStartJump()
    {
        StartCoroutine(StartJump());
    }

    private void StopJump()
    {
        rigid.transform.position = startCamPosition;
        rigid.velocity = Vector2.zero;
        rigid.gravityScale = 0;
    }

    private IEnumerator StartJump()
    {
        rigid.AddForce(Vector2.up * 4.5f, ForceMode2D.Impulse); //12.5
        rigid.gravityScale = 0.4f; //2.5
        yield return new WaitForSeconds(0.65f);
        while (rigid.gravityScale >= 0.2f)
        {
            rigid.gravityScale -= Time.deltaTime * 3.5f;
            yield return null;
        }
        rigid.gravityScale = 1.5f;
    }

    IEnumerator TestCamShake(float shakeAmount)
    {
        int multiplication = 1;
        Vector3 nowCamPos = new Vector3(initialPosition.x, initialPosition.y, initialPosition.z);
        for (int nowShakeCount = 0; nowShakeCount < 5; nowShakeCount++)
        {
            nowCamPos.x = shakeAmount * multiplication;
            nowCamPos.y = initialPosition.y;
            transform.position = nowCamPos;

            shakeAmount = (nowShakeCount == 0) ? shakeAmount -= shakeAmount / 2 : shakeAmount -= shakeAmount / 5;
            
            multiplication *= -1;
            yield return shakeDelay;
        }
    }
}
