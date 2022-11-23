using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRange : MonoBehaviour
{
    [SerializeField]
    [Tooltip("이 공격 범위를 가진 해당 유닛 종류")]
    private bool isPlayer;

    private bool isLeft;

    [SerializeField]
    [Tooltip("이 공격 범위를 가진 해당 유닛 오브젝트")]
    private GameObject unitObj;

    [SerializeField]
    [Tooltip("콜라이더 X 좌표")]
    private float setColliderXPos;

    private Vector2 colliderPos;

    private BasicUnitScript gameObjectBasicUnitScriptComponent;

    private void Start()
    {
        gameObjectBasicUnitScriptComponent = unitObj.GetComponent<BasicUnitScript>();
    }

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
            ChangeRotation();
            isLeft = true;
        }
        else if(transform.rotation.y == 0 && isLeft)
        {
            ChangeRotation();
            isLeft = false;
        }
    }

    /// <summary>
    /// 방향 전환
    /// </summary>
    private void ChangeRotation()
    {
        setColliderXPos *= -1;
    }

    /// <summary>
    /// 적이 공격 범위에 들어오면 실행
    /// </summary>
    /// <param name="collision"> 공격 범위에 들어온 오브젝트 콜라이더 </param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("DeflectAbleObj"))
        {
            gameObjectBasicUnitScriptComponent.rangeInDeflectAbleObj.Add(collision.gameObject);
        }
        else 
        {
            if (collision.gameObject != unitObj)
            {
                switch (isPlayer)
                {
                    case true:
                        if (collision.gameObject.CompareTag("Enemy"))
                        {
                            gameObjectBasicUnitScriptComponent.rangeInEnemy.Add(collision.gameObject);
                        }
                        break;
                    case false:
                        if (collision.gameObject.CompareTag("Player"))
                        {
                            gameObjectBasicUnitScriptComponent.rangeInEnemy.Add(collision.gameObject);
                        }
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 적이 공격 범위에서 나가면 실행
    /// </summary>
    /// <param name="collision"> 공격 범위에서 나간 오브젝트 콜라이더 </param>
    private void OnTriggerExit2D(Collider2D collision) //적이 범위에 나갈 시
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Player"))
        {
            gameObjectBasicUnitScriptComponent.rangeInEnemy.Remove(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("DeflectAbleObj"))
        {
            gameObjectBasicUnitScriptComponent.rangeInDeflectAbleObj.Remove(collision.gameObject);
        }
    }
}
