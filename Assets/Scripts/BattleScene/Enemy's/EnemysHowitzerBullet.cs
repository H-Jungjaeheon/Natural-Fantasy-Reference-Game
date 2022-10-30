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

    private IEnumerator bulletDropCoroutine;

    void Awake()
    {
        StartSetting();
    }

    private void OnEnable()
    {
        StartCoroutine(OrbitalIndication());
    }

    private void StartSetting()
    {
        moveSpeed = new Vector2(0, speed);
        bulletDropCoroutine = BulletDrop();
        OrbitalIndicationDelay = new WaitForSeconds(2);
    }

    IEnumerator OrbitalIndication()
    {
        onEnablePos.x = BattleSceneManager.Instance.Player.transform.position.x;
        onEnablePos.y = startYPos;
        transform.position = onEnablePos;

        orbitalIndicationObj.SetActive(true);
        yield return OrbitalIndicationDelay;
        orbitalIndicationObj.SetActive(false);

        StartCoroutine(BulletDrop());
    }

    IEnumerator BulletDrop()
    {
        while (true)
        {
            transform.position -= (Vector3)(moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        BasicUnitScript hitObjsUnitScript = collision.GetComponent<BasicUnitScript>();

        StopCoroutine(bulletDropCoroutine);

        if (collision.CompareTag("Player"))
        {
            if (hitObjsUnitScript.nowDefensivePosition == DefensePos.Up)
            {
                CamShake.CamShakeMod(true, 1.5f);
                hitObjsUnitScript.Hit(damage, true);
            }
            else
            {
                CamShake.CamShakeMod(false, 2f);
                hitObjsUnitScript.Hit(damage, false); //대각선
            }
            //원형으로 터지는 애니메이션 실행 후 풀로 보내기 (코루틴)
            ReturnToObjPool();
        }
        else if (collision.CompareTag("Floor"))
        {
            //바닥에 철퍽 하는 애니메이션 실행 후 풀로 보내기 (코루틴)
            ReturnToObjPool();
        }
    }

    private void ReturnToObjPool() => ObjectPool.Instance.ReturnObject(gameObject, (int)thisBulletPoolObjKind);
}