using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSceneManager : Singleton<BattleSceneManager>
{
    [Header("�������� ���� ������")]
    [Tooltip("�÷��̾��� ������")]
    public Vector2 PlayerCharacterPos;
    [Tooltip("���� ������")]
    public Vector2 EnemyCharacterPos;

    private void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
}
