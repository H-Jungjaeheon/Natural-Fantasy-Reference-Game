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

    //[SerializeField]
    //[Tooltip("공격 범위 콜라이더")]
    //private Collider2D attackRangeCollider;

    [SerializeField]
    [Tooltip("콜라이더 X 좌표")]
    private float setColliderXPos;

    private Vector2 colliderPos;

    private bool isLeft;

    private void Update()
    {
        PositionSetting();
    }

    private void PositionSetting()
    {
        colliderPos.x = unitObj.transform.position.x + setColliderXPos;
        colliderPos.y = unitObj.transform.position.y;
        transform.position = colliderPos;
        if (transform.rotation.y != 0 && isLeft == false)
        {
            ChangeRotation(false);
            
        }
        else if(transform.rotation.y == 0 && isLeft)
        {
            ChangeRotation(true);
            isLeft = false;
        }
    }

    private void ChangeRotation(bool ChangeRotate)
    {
        ;
        setColliderXPos *= -1;
    }

    private void OnTriggerEnter2D(Collider2D collision) //적이 범위에 들어올 시
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Player") && collision.gameObject != unitObj)
        {
            unitObj.GetComponent<BasicUnitScript>().rangeInEnemy.Add(collision.gameObject);
        }
        else if(collision.gameObject.CompareTag("DeflectAbleObj"))
        {
            unitObj.GetComponent<BasicUnitScript>().rangeInDeflectAbleObj.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) //적이 범위에 나갈 시
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Player") && collision.gameObject != unitObj)
        {
            unitObj.GetComponent<BasicUnitScript>().rangeInEnemy.Remove(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("DeflectAbleObj"))
        {
            unitObj.GetComponent<BasicUnitScript>().rangeInDeflectAbleObj.Remove(collision.gameObject);
        }
    }
}
