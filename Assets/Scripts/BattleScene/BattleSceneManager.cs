using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSceneManager : Singleton<BattleSceneManager>
{
    [Header("�������� ���� ������")]
    public Vector2 PlayerCharacterCloseRangeAttackPos;
    public Vector2 EnemyCharacterCloseRangeAttackPos;

    private void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
}
