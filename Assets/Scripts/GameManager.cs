using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    #region ���� ������ ����
    private class GameSaveData
    {
        public int BasicDamageLevel;
        public int BasicHealthLevel;
        public int BasicEnergyLevel;
        public int ReduceCoolTimeLevel;
        public int Gold;
    }
    #endregion

    #region ��ȭ �� ���� ������
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
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
