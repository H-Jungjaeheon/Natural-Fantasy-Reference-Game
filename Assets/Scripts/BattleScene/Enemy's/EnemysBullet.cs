using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BulletState
{
    Firing,
    Deflecting
}

public class EnemysBullet : MonoBehaviour
{
    [SerializeField]
    [Tooltip("총알 데미지")]
    private int damage;

    [SerializeField]
    [Tooltip("총알 속도")]
    private float speed;

    [Tooltip("총알 반사 가능한지 판별")]
    public bool isDeflectAble;

    public BulletState nowBulletState;

    public Vector2 moveDirection;

    [SerializeField]
    [Tooltip("오브젝트 풀의 총알 종류")]
    private PoolObjKind thisBulletPoolObjKind;

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
        if (isDeflectAble)
        {
            transform.position = (nowBulletState == BulletState.Deflecting) ? transform.position + (Vector3)(moveSpeed * Time.deltaTime) : transform.position - (Vector3)(moveSpeed * Time.deltaTime);
        }
        //transform.Translate(moveDirection * (Time.deltaTime * speed));
        //position = new Vector2(Mathf.Cos(i * Mathf.Deg2Rad), Mathf.Sin(i * Mathf.Deg2Rad));
        //Fire(position, Vector2.one * 0.2f, (position - transform.position).normalized, 5, 1, Bullet.BulletType.Enemy, system);
    }

    public void Reflex(BulletState isReflexToPlayer)
    {
        nowBulletState = isReflexToPlayer;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        BasicUnitScript hitObjsUnitScript = collision.GetComponent<BasicUnitScript>();
        if (collision.CompareTag("Player") && nowBulletState == BulletState.Firing)
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
            ReturnToObjPool();
        }
        else if (collision.CompareTag("Enemy") && nowBulletState == BulletState.Deflecting)
        {
            CamShake.CamShakeMod(false, 2f); //대각선
            hitObjsUnitScript.Hit(damage, false);
            ReturnToObjPool();
        }
        else if (collision.CompareTag("ObjDestroy"))
        {
            ReturnToObjPool();
        }
    }

    private void ReturnToObjPool() => ObjectPool.Instance.ReturnObject(gameObject, (int)thisBulletPoolObjKind);
}
