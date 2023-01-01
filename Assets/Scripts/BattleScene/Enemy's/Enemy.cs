using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossPhase
{
    PhaseOne = 1,
    PhaseTwo = 2,
    PhaseThree = 3
}

public enum NowEnemyProperty
{
    Mutant,
    Guardian,
    Rot,
    EvilSpirit,
    Angel,
    PropertyTotalNumber
}

public class Enemy : BasicUnitScript
{
    private string[] pattonText; //텍스트로 불러온 패턴 번호들

    private int pattonCount; //현재 패턴 사용 횟수

    /// <summary>
    /// 게임 처음 세팅
    /// </summary>
    protected override void StartSetting()
    {
        nowState = NowState.Standingby;
        nowDefensivePosition = DefensePos.None;

        isWaiting = true;

        pattonText = PattonText();

        bsm.enemyCharacterPos = transform.position;
        bsm.enemy = gameObject;

        Energy = MaxEnergy;
        Hp = MaxHp;

        restWaitTime = 1.85f;

        originalDamage = Damage;
        originalMaxActionCoolTime = maxActionCoolTime;
        originalRestWaitTime = restWaitTime;
        originalSpeed = Speed;

        plusVector = new Vector3(0f, -6f, 0f);
        particlePos = new Vector3(0f, -2f, 0f);

        StartCoroutine(WaitUntilTheGameStarts());
    }
}
