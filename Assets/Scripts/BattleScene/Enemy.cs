using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : BasicUnitScript
{

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
        print(isWaiting);
    }

    protected override void UISetting()
    {
        if (isWaiting)
        {
            nullActionCoolTimeImage.transform.position = Cam.WorldToScreenPoint(transform.position + new Vector3(0, actionCoolTimeImageYPos_F, 0));
            actionCoolTimeImage.fillAmount = nowActionCoolTime / maxActionCoolTime;
            nowActionCoolTime += Time.deltaTime;
            if (nowActionCoolTime >= maxActionCoolTime)
            {
                isWaiting = false;
                actionCoolTimeObj.SetActive(false);
            }
        }
    }
}
