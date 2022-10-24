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

    public GameObject Enemy;

    private BasicUnitScript nowEnemyBasicUnitScriptComponent;

    [SerializeField]
    [Tooltip("적 체력 텍스트")]
    private Text enemyHpText;

    [SerializeField]
    [Tooltip("적 기력 텍스트")]
    private Text enemyEnergyText;

    [SerializeField]
    [Tooltip("적 몽환게이지 텍스트")]
    private Text enemyDreamyFigureText;

    private void Start()
    {
        nowEnemyBasicUnitScriptComponent = Enemy.GetComponent<BasicUnitScript>();
    }

    private void Update()
    {
        EnemyStatTextSetting();
    }

    private void EnemyStatTextSetting()
    {
        enemyHpText.text = $"{nowEnemyBasicUnitScriptComponent.Hp_F}/{nowEnemyBasicUnitScriptComponent.MaxHp_F}";
        enemyEnergyText.text = $"{nowEnemyBasicUnitScriptComponent.Energy_F}/{nowEnemyBasicUnitScriptComponent.MaxEnergy_F}";
        enemyDreamyFigureText.text = $"{nowEnemyBasicUnitScriptComponent.DreamyFigure_F}/{nowEnemyBasicUnitScriptComponent.maxDreamyFigure_F}";
    }
}
