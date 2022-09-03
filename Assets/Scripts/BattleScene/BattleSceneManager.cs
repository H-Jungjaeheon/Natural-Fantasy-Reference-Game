using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UnitKind
{
    Player,
    Enemy
}

public class BattleSceneManager : Singleton<BattleSceneManager>
{
    [HideInInspector]
    public Vector2 PlayerCharacterPos; //플레이어 포지션

    [HideInInspector]
    public Vector2 EnemyCharacterPos; //적 포지션

    public GameObject Player;

    [HideInInspector]
    public GameObject Enemy;
}
