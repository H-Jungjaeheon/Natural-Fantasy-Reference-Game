using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum UpgradeableStatKind
{
    Hp,
    Damage,
    Energy,
    TotalStats
    //CoolTime,
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
    private int gold;
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
        }
    }

    private int slimeBossMaterial;

    public int SlimeBossMaterial
    {
        get { return slimeBossMaterial; }
        set { slimeBossMaterial = value; }
    }

    private int reduceCoolTimeLevel;
    public int ReduceCoolTimeLevel 
    {
        get { return reduceCoolTimeLevel; }
        set { reduceCoolTimeLevel = value; }
    }

    [HideInInspector]
    public int[] statLevels = new int[(int)UpgradeableStatKind.TotalStats];
    #endregion

}
