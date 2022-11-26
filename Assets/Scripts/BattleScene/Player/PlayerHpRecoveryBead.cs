using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHpRecoveryBead : MonoBehaviour
{
    [SerializeField]
    [Tooltip("체력 회복량")]
    private int recoveryAmount;

    private float nowDeleteTimeLimit;

    private int maxDeleteTimeLimit;

    private ObjectPool OP;

    private Player Player;

    private void Start()
    {
        StartSetting();
    }

    private void StartSetting()
    {
        StartCoroutine(DeleteTimeLimit());
        StartCoroutine(DeterminePlayerProperties());

        maxDeleteTimeLimit = 5;
        OP = ObjectPool.Instance;
        Player = BattleSceneManager.Instance.Player;
    }

    private void OnEnable()
    {
        int upwardPlacementProbability = 35;

        nowDeleteTimeLimit = 0;
        int randomSpawnPositionProbability = Random.Range(0, 100);
        if (randomSpawnPositionProbability < upwardPlacementProbability)
        {
            transform.position = new Vector2(-10, 5);
        }
        else
        {
            int randomSpawnXPosition = Random.Range(-7, 16);
            transform.position = new Vector2(randomSpawnXPosition, -1);
        }
    }

    private IEnumerator DeleteTimeLimit()
    {
        while (true)
        {
            nowDeleteTimeLimit += Time.deltaTime;
            if (nowDeleteTimeLimit >= maxDeleteTimeLimit)
            {
                DeleteSetting();
                yield break;
            }
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) //나중에 인터페이스로 충돌 상호작용 제작
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player.Hp += recoveryAmount;
            DeleteSetting();
        }
    }

    private void DeleteSetting()
    {
        OP.ReturnObject(gameObject, (int)PoolObjKind.PlayerHpRecoveryBead);
        Player.isSpawnNatureBead = false;
        Player.NowNaturePassiveCount = 0;
    }

    private IEnumerator DeterminePlayerProperties()
    {
        while (true)
        {
            if (Player.nowProperty != NowPlayerProperty.NatureProperty)
            {
                DeleteSetting();
                yield break;
            }
            yield return null;
        }
    }
}
