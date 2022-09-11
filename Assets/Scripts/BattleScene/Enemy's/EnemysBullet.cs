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
    private void OnTriggerStay2D(Collider2D collision)
    {
        BasicUnitScript hitObjsUnitScript = collision.GetComponent<BasicUnitScript>();
        if (collision.CompareTag("Player") && isDeflecting == false)
        {
            if (hitObjsUnitScript.nowDefensivePosition == DefensePos.Right)
            {
                CamShake.CamShakeMod(true, 1.5f);
                hitObjsUnitScript.Hit(damage, true);
            }
            else
            {
                CamShake.CamShakeMod(false,  2f);
                hitObjsUnitScript.Hit(damage, false); //대각선
            }
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Enemy") && isDeflecting)
        {
            CamShake.CamShakeMod(false, 2f); //대각선
            hitObjsUnitScript.Hit(damage, false);
            Destroy(gameObject);
        }
        else if (collision.CompareTag("ObjDestroy"))
        {
            Destroy(gameObject);
        }
    }
}
