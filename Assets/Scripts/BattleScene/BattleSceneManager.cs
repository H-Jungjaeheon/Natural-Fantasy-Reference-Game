using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSceneManager : Singleton<BattleSceneManager>
{
    [Header("근접공격 도착 포지션")]
    [Tooltip("플레이어가 공격하러 갈 때의 포지션")]
    public Vector2 PlayerCharacterCloseRangeAttackPos;
    [Tooltip("적이 플레이어에게 공격하러 올 때의 포지션")]
    public Vector2 EnemyCharacterCloseRangeAttackPos;

    private void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
}
