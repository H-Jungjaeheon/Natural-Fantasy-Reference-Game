using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHpRecoveryBead : MonoBehaviour
{
    [SerializeField]
    [Tooltip("체력 회복량")]
    private int RecoveryAmount;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnEnable()
    {
        //랜덤 위치
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            BattleSceneManager.Instance.Player.Hp_F += RecoveryAmount;
            ObjectPool.Instance.ReturnObject(gameObject, (int)PoolObjKind.PlayerHpRecoveryBead);
        }
    }
}
