using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemysBullet : MonoBehaviour
{
    [SerializeField]
    private int damage;

    [SerializeField]
    private float speed;

    [SerializeField]
    private bool isDeflecting;

    private Vector3 moveSpeed;

    // Start is called before the first frame update
    void Start()
    {
        StartSetting();
    }

    // Update is called once per frame
    void Update()
    {
        BulletMove();
    }

    private void StartSetting()
    {
        moveSpeed = new Vector3(speed, 0, 0);
    }

    private void BulletMove()
    {
        transform.position = isDeflecting ? transform.position + (Vector3)(moveSpeed * Time.deltaTime) : transform.position - (Vector3)(moveSpeed * Time.deltaTime);
    }

    public void Reflex(bool isReflexToPlayer)
    {
        isDeflecting = isReflexToPlayer;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        BasicUnitScript hitObjsUnitScript = collision.gameObject.GetComponent<BasicUnitScript>();
        if (collision.gameObject.CompareTag("Player") && isDeflecting == false)
        {
            if (hitObjsUnitScript.nowDefensivePosition == DefensePos.Right)
            {
                hitObjsUnitScript.Hit(damage, true);
            }
            else
            {
                hitObjsUnitScript.Hit(damage, false);
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("Enemy") && isDeflecting)
        {
            hitObjsUnitScript.Hit(damage, false);
            Destroy(gameObject);
        }
        else if (collision.gameObject.CompareTag("ObjDestroy"))
        {
            Destroy(gameObject);
        }
    }
}
