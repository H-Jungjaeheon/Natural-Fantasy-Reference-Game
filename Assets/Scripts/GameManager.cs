using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    #region 저장 데이터 모음
    [System.Serializable]
    public class GameSaveData
    {
        public int BasicDamageLevel;
        public int BasicHealthLevel;
        public int BasicEnergyLevel;
        public int ReduceCoolTimeLevel;
        public int Gold;
    }
    #endregion

    #region 재화 및 레벨 변수들
    private uint gold;
    public uint Gold
    {
        get { return gold; }
        set { gold = value; }
    }

    private int reduceCoolTimeLevel;
    public int ReduceCoolTimeLevel 
    {
        get { return reduceCoolTimeLevel; }
        set { reduceCoolTimeLevel = value; }
    }

    [SerializeField]
    private int maxHpUpgradeLevel;
    public int MaxHpUpgradeLevel
    {
        get { return maxHpUpgradeLevel; }
        set { maxHpUpgradeLevel = value; }
    }

    private int maxEnergyUpgradeLevel;
    public int MaxEnergyUpgradeLevel
    {
        get { return maxEnergyUpgradeLevel; }
        set { maxEnergyUpgradeLevel = value; }
    }

    private int damageUpgradeLevel;
    public int DamageUpgradeLevel
    {
        get { return damageUpgradeLevel; }
        set { damageUpgradeLevel = value; }
    }
    #endregion

    public GameObject testBullet;

    IEnumerator TestBulletFire() //360도로 총알 발사되는 것
    {
        //mathf탄막 연습
        for (int i = 0; i < 360; i += 30)
        {
            GameObject bulletObj;
            Vector3 dir = new Vector2(Mathf.Cos(i * Mathf.Deg2Rad), Mathf.Sin(i* Mathf.Deg2Rad)); //* Mathf.Deg2Rad
            bulletObj = Instantiate(testBullet, BattleSceneManager.Instance.Enemy.transform.position, Quaternion.identity);
            bulletObj.GetComponent<EnemysBullet>().moveDirection = dir;//(dir - transform.position).normalized;
            yield return null;
        }
    }
}
