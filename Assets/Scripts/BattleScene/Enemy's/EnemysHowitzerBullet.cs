using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemysHowitzerBullet : MonoBehaviour
{
    [SerializeField]
    [Tooltip("총알 데미지")]
    private int damage;

    [SerializeField]
    [Tooltip("총알 속도")]
    private float speed;

    [SerializeField]
    [Tooltip("초기 위치 Y값")]
    private float startYPos;

    [SerializeField]
    [Tooltip("오브젝트 풀의 총알 종류")]
    private PoolObjKind thisBulletPoolObjKind;

    [SerializeField]
    [Tooltip("궤도 표시 오브젝트")]
    private GameObject orbitalIndicationObj;

    private Vector2 moveSpeed;

    private Vector2 onEnablePos;

    private WaitForSeconds OrbitalIndicationDelay;

    void Start()
    {
        StartSetting();
    }

    void Update()
    {
        BulletMove();
    }

    private void OnEnable()
    {
        StartCoroutine(OrbitalIndication());
    }

    private void StartSetting()
    {
        moveSpeed = new Vector2(0, speed);
        OrbitalIndicationDelay = new WaitForSeconds(2);
    }

    private void BulletMove()
    {
        transform.position -= (Vector3)(moveSpeed * Time.deltaTime);
    }

    IEnumerator OrbitalIndication()
    {
        onEnablePos.x = BattleSceneManager.Instance.Player.transform.position.x;
        onEnablePos.y = startYPos;
        transform.position = onEnablePos;

        orbitalIndicationObj.SetActive(true);
        yield return OrbitalIndicationDelay;
        orbitalIndicationObj.SetActive(false);


        yield return null;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        BasicUnitScript hitObjsUnitScript = collision.GetComponent<BasicUnitScript>();
        if (collision.CompareTag("Player"))
        {
            if (hitObjsUnitScript.nowDefensivePosition == DefensePos.Right)
            {
                CamShake.CamShakeMod(true, 1.5f);
                hitObjsUnitScript.Hit(damage, true);
            }
            else
            {
                CamShake.CamShakeMod(false, 2f);
                hitObjsUnitScript.Hit(damage, false); //대각선
            }
            ReturnToObjPool();
        }
        else if (collision.CompareTag("Enemy"))
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