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

    Vector3 jumpingShaking = new Vector3(0,0,-10);
    Vector3 initialPosition;
    
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
            StartCoroutine(TestCamShake(0.6f));
        }
    }

    public void StartSetting()
    {
        NowCamShakeStart += CamShakeStart;
        playerComponent = BattleSceneManager.Instance.Player;
        initialPosition = transform.position;
    }

    public void CamShakeStart(float timeInput, float shakeAmountInput)
    {
        shakeTime = timeInput;
        shakeAmount = shakeAmountInput;
        isShaking = true;
    }

    private void CamShaking()
    {
        if (isShaking == true)
        {
            if (playerComponent.isJumping == false)
            {
                transform.position = Random.insideUnitSphere * shakeAmount + initialPosition;
            }
            else
            {
                jumpingShaking.x = Random.insideUnitSphere.x * shakeAmount;
                jumpingShaking.y = Random.insideUnitSphere.y * shakeAmount + playerComponent.transform.position.y;
                transform.position = jumpingShaking;
            }
            shakeTime -= Time.deltaTime;
            if (shakeTime <= 0)
            {
                isShaking = false;
                shakeTime = 0;
            }
        }
        else 
        {
            if (playerComponent.isJumping == false)
            {
                transform.position = initialPosition;
            }
            else
            {
                jumpingShaking.x = transform.position.x;
                jumpingShaking.y = playerComponent.transform.position.y;
                transform.position = jumpingShaking;
            }
        }
    }

    IEnumerator TestCamShake(float shakeAmount)
    {
        WaitForSeconds shakeDelay = new WaitForSeconds(0.05f);
        int maxShakeCount = 5;
        int multiplication = 1;
        float nowShakeAmount = shakeAmount;
        while (maxShakeCount > 0)
        {
            jumpingShaking.x = nowShakeAmount * multiplication;
            jumpingShaking.y = transform.position.y;
            transform.position = jumpingShaking;
            nowShakeAmount -= shakeAmount / 7;
            multiplication *= -1;
            maxShakeCount -= 1;
            yield return shakeDelay;
        }
    }
}
