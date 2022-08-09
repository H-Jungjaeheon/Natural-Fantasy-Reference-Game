using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : BasicUnitScript
{
    private void Awake()
    {
        StartSetting();
    }

    void Update()
    {
        UISetting();
    }

    protected override void StartSetting()
    {
        isWaiting = true;
        nowActionCoolTime = 0;
        Cam = Camera.main;
        rigid = gameObject.GetComponent<Rigidbody2D>();
        startPos_Vector = transform.position;
        nowAttackCount_I = 1;
        BattleSceneManager.Instance.PlayerCharacterPos = transform.position;
        actionCoolTimeObj.SetActive(true);
    }

    protected override void UISetting()
    {
        if (isWaiting)
        {
            actionCoolTimeImage.fillAmount = nowActionCoolTime / maxActionCoolTime;
            nowActionCoolTime += Time.deltaTime;
            if (nowActionCoolTime >= maxActionCoolTime)
            {
                isWaiting = false;
                actionCoolTimeObj.SetActive(false);
            }
        }
        nullActionCoolTimeImage.transform.position = Cam.WorldToScreenPoint(transform.position + new Vector3(0, actionCoolTimeImageYPos_F, 0));
    }
}
