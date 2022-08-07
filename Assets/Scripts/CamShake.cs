using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamShake : MonoBehaviour
{
    private float shakeAmount;
    private float shakeTime;
    private bool isShaking;

    [SerializeField]
    private GameObject playerObj;

    Player playerComponent;

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
    }

    public void StartSetting()
    {
        playerComponent = playerObj.GetComponent<Player>();
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
                print("기본 실행");
            }
            else
            {
                jumpingShaking.x = Random.insideUnitSphere.x * shakeAmount;
                jumpingShaking.y = Random.insideUnitSphere.y * shakeAmount + playerObj.transform.position.y;
                transform.position = jumpingShaking;
                print("점프 실행");
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
                jumpingShaking.y = playerObj.transform.position.y;
                transform.position = jumpingShaking;
            }
        }
    }
}
