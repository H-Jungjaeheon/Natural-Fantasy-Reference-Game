using UnityEngine;

public enum UpgradeableStatKind
{
    Hp,
    Damage,
    Energy,
    CoolTime,
    TotalStats
}

public enum SceneKind
{
    Title,
    Main,
    Ingame
}

public class GameManager : Singleton<GameManager>
{
    #region 저장 데이터 모음
    [System.Serializable]
    public class GameSaveData
    {
        public int GoldData;
        public int SlimeBossMaterialData;
        public int BasicDamageLevelData;
        public int BasicHealthLevelData;
        public int BasicEnergyLevelData;
        public int ReduceCoolTimeLevelData;
    }
    #endregion

    #region 재화 및 레벨 변수들
    public int gold;
    public int Gold
    {
        get { return gold; }
        set
        {
            if (value > 999999999)
            {
                value = 999999999;
            }
            gold = value;
            if (nowScene == SceneKind.Main)
            {
                MainManager.Instance.BasicGoodsTextFixed();
            }
        }
    }

    [HideInInspector]
    public int[] statLevels;
    #endregion

    [HideInInspector]
    public SceneKind nowScene;

    [Tooltip("현재 스테이지")]
    public Stage nowStage;

    public void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else if (isDontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }

        statLevels = new int[(int)UpgradeableStatKind.TotalStats];
    }
}
