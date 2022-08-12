using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRange : MonoBehaviour
{
    [SerializeField]
    [Tooltip("�� ���� ������ ���� �ش� ���� ����")]
    private bool isPlayer;

    [SerializeField]
    [Tooltip("�� ���� ������ ���� �ش� ���� ������Ʈ")]
    private GameObject unitObj;

    //[SerializeField]
    //[Tooltip("���� ���� �ݶ��̴�")]
    //private Collider2D attackRangeCollider;

    [SerializeField]
    [Tooltip("�ݶ��̴� X ��ǥ")]
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

    private void OnTriggerEnter2D(Collider2D collision) //���� ������ ���� ��
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

    private void OnTriggerExit2D(Collider2D collision) //���� ������ ���� ��
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
