using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRange : MonoBehaviour
{
    [SerializeField]
    [Tooltip("이 공격 범위를 가진 해당 유닛 종류")]
    private bool isPlayer;

    [SerializeField]
    [Tooltip("이 공격 범위를 가진 해당 유닛 오브젝트")]
    private GameObject unitObj;

    [SerializeField]
    [Tooltip("공격 범위 콜라이더")]
    private Collider2D attackRangeCollider;

    [SerializeField]
    [Tooltip("콜라이더 X 좌표")]
    private float setColliderXPos;

    private Vector2 colliderPos;

    private void Update()
    {
        PositionSetting();
    }

    private void PositionSetting()
    {
        colliderPos.x = unitObj.transform.position.x + setColliderXPos;
        colliderPos.y = unitObj.transform.position.y;
        transform.position = colliderPos;
    }

    private void OnTriggerEnter2D(Collider2D collision) //적이 범위에 들어올 시
    {
        if (isPlayer && collision.gameObject.CompareTag("Enemy"))
        {
            unitObj.GetComponent<Player>().rangeInEnemy.Add(collision.gameObject);
        }
        else if(isPlayer == false && collision.gameObject.CompareTag("Player"))
        {
            //unitObj.GetComponent<Enemy>().
        }
    }

    private void OnTriggerExit2D(Collider2D collision) //적이 범위에 나갈 시
    {
        if (isPlayer && collision.gameObject.CompareTag("Enemy"))
        {
            unitObj.GetComponent<Player>().rangeInEnemy.Remove(collision.gameObject);
        }
        else if(isPlayer == false && collision.gameObject.CompareTag("Player"))
        {
            //unitObj.GetComponent<Enemy>().
        }
    }
}
