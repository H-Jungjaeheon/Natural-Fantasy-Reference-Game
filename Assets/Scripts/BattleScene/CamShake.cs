using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class CamShake : MonoBehaviour
{
    public static Action<float, float> NowCamShakeStart;
    private float shakeAmount;
    private float shakeTime;
    private bool isShaking;

    private Player playerComponent;

    Vector3 jumpingShaking = new Vector3(0, 0.4f,-10);
    Vector3 settingPosition;
    Vector3 initialPosition;
    WaitForSeconds shakeDelay = new WaitForSeconds(0.06f);


    // Start is called before the first frame update
    void Awake()
    {
        StartSetting();
    }

    // Update is called once per frame
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
        NowCamShakeStart += CamShakeStart;
        playerComponent = BattleSceneManager.Instance.Player;
        initialPosition = transform.position;
        settingPosition = new Vector3(0, transform.position.y, -10);
    }

    public void CamShakeStart(float timeInput, float shakeAmountInput)
    {
        shakeTime = timeInput;
        shakeAmount = shakeAmountInput;
        isShaking = true;
    }

    private void CamShaking()
    {
        jumpingShaking.y = transform.position.y + playerComponent.transform.position.y;

        transform.position = (playerComponent.isJumping) ? jumpingShaking : initialPosition;
    }

    IEnumerator TestCamShake(float shakeAmount)
    {
        int multiplication = 1;
        Vector3 nowCamPos = new Vector3(0,0,-10);
        for (int nowShakeCount = 0; nowShakeCount < 5; nowShakeCount++)
        {
            nowCamPos.x = shakeAmount * multiplication;
            nowCamPos.y = jumpingShaking.y;
            transform.position = nowCamPos;

            shakeAmount = (nowShakeCount == 0) ? shakeAmount -= shakeAmount / 2 : shakeAmount -= shakeAmount / 5;
            
            multiplication *= -1;
            yield return shakeDelay;
        }
    }
}
