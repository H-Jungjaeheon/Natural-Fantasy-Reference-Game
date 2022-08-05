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

    [SerializeField]
    [Tooltip("���� ���� �ݶ��̴�")]
    private Collider2D attackRangeCollider;

    private void OnTriggerEnter2D(Collider2D collision) //���� ������ ���� ��
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

    private void OnTriggerExit2D(Collider2D collision) //���� ������ ���� ��
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
