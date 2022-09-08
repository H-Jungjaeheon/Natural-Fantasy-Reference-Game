using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UnitKind
{
    Player,
    Enemy
}

public class BattleSceneManager : Singleton<BattleSceneManager> //나중에 게임 오버 및 게임 클리어, 재화 관리
{
    [HideInInspector]
    public Vector2 PlayerCharacterPos; //플레이어 포지션

    [HideInInspector]
    public Vector2 EnemyCharacterPos; //적 포지션

    public Player Player;

    [HideInInspector]
    public GameObject Enemy;
}
