using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSceneManager : Singleton<BattleSceneManager>
{
    [Header("근접공격 도착 포지션")]
    [Tooltip("플레이어의 포지션")]
    public Vector2 PlayerCharacterPos;
    [Tooltip("적의 포지션")]
    public Vector2 EnemyCharacterPos;

    private void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
}
