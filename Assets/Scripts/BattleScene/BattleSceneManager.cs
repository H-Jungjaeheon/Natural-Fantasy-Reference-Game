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
    [Header("근접공격 도착 포지션")]
    [Tooltip("플레이어의 포지션")]
    public Vector2 PlayerCharacterPos;
    [Tooltip("적의 포지션")]
    public Vector2 EnemyCharacterPos;

    [SerializeField]
    private GameObject Player;

    [SerializeField]
    private GameObject Enemy;

    [SerializeField]
    private Image[] unitHpBars;

    [SerializeField]
    private Image[] unitEnergyBars;

    [SerializeField]
    private Image[] unitDreamyFigureBars;

    Player playerComponent;
    Enemy enemyComponenet;

    private void Awake()
    {
        Enemy = GameObject.FindGameObjectWithTag("Enemy");
        playerComponent = Player.GetComponent<Player>();
        enemyComponenet = Enemy.GetComponent<Enemy>();
    }

    // Update is called once per frame
    void Update()
    {
        UnitBarsUpdate();
    }
    private void UnitBarsUpdate()
    {
        unitHpBars[(int)UnitKind.Player].fillAmount = playerComponent.Hp_F / playerComponent.MaxHp_F;
        unitEnergyBars[(int)UnitKind.Player].fillAmount = playerComponent.Energy_F / playerComponent.MaxEnergy_F;
        unitDreamyFigureBars[(int)UnitKind.Player].fillAmount = playerComponent.DreamyFigure_F / playerComponent.MaxDreamyFigure_F;
        unitHpBars[(int)UnitKind.Enemy].fillAmount = enemyComponenet.Hp_F / enemyComponenet.MaxHp_F;
        unitEnergyBars[(int)UnitKind.Enemy].fillAmount = enemyComponenet.Energy_F / enemyComponenet.MaxEnergy_F;
        unitDreamyFigureBars[(int)UnitKind.Enemy].fillAmount = enemyComponenet.DreamyFigure_F / enemyComponenet.MaxDreamyFigure_F;
    }

}
