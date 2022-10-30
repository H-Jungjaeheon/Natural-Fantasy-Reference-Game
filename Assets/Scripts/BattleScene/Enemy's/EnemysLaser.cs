using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemysLaser : MonoBehaviour
{
    [SerializeField]
    [Tooltip("레이저 데미지")]
    private int damage;

    [SerializeField]
    [Tooltip("오브젝트 풀의 총알 종류")]
    private PoolObjKind thisBulletPoolObjKind;

    [SerializeField]
    [Tooltip("궤도 표기 오브젝트")]
    private GameObject orbitalIndicationObj;

    public List<GameObject> targetInRange = new List<GameObject>();
    
    [HideInInspector]
    public float launchAngle;

    [HideInInspector]
    public Vector2 onEnablePos; //활성화 시 초기 포지션

    private Vector3 onEnableRotation; //활성화 시 초기 회전값

    private WaitForSeconds OrbitalIndicationDelay;

    void Awake()
    {
        OrbitalIndicationDelay = new WaitForSeconds(0.9f);
    }

    private void OnEnable()
    {
        StartCoroutine(OrbitalIndication());
    }

    IEnumerator OrbitalIndication()
    {
        orbitalIndicationObj.SetActive(false);

        yield return null;

        orbitalIndicationObj.transform.position = onEnablePos;

        onEnableRotation.z = launchAngle;
        orbitalIndicationObj.transform.rotation = Quaternion.Euler(onEnableRotation);

        orbitalIndicationObj.SetActive(true);

        yield return OrbitalIndicationDelay;

        StartCoroutine(GiveDamage());
    }

    IEnumerator GiveDamage()
    {
        bool isCameraShaking = false;
        for (int nowIndex = targetInRange.Count - 1; nowIndex >= 0; nowIndex--) //공격과 동시에 리스트 정리
        {
            if (targetInRange[nowIndex] == true)
            {
                BasicUnitScript hitObjsUnitScript = targetInRange[nowIndex].GetComponent<BasicUnitScript>();
                hitObjsUnitScript.Hit(damage, false);
                if (isCameraShaking == false)
                {
                    CamShake.CamShakeMod(false, 2f);
                    isCameraShaking = true;
                }

                targetInRange.Remove(targetInRange[nowIndex]);
            }
        }

        ReturnToObjPool();
        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            targetInRange.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            targetInRange.Remove(collision.gameObject);
        }
    }

    private void ReturnToObjPool() => ObjectPool.Instance.ReturnObject(gameObject, (int)thisBulletPoolObjKind);
}
