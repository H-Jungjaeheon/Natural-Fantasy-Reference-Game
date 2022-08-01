using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    #region Ω∫≈» µ•¿Ã≈Õ
    public class GameStatData
    {
        public int BasicDamageLevel;
        public int BasicHealthLevel;
        public int BasicEnergyLevel;
        public int ReduceCoolTimeLevel;
    }

    public GameStatData GSD;

    #endregion

    private int gold;
    public int Gold
    {
        get
        {
            return gold;
        }
        set
        {
            gold = value;
        }
    }

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
