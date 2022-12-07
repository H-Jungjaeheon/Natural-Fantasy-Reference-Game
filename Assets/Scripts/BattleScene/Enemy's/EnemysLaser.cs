using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemysLaser : MonoBehaviour
{
    [SerializeField]
    [Tooltip("오브젝트 풀의 총알 종류")]
    private PoolObjKind thisBulletPoolObjKind;

    [SerializeField]
    [Tooltip("레이저 데미지")]
    private int damage;

    [SerializeField]
    [Tooltip("레이저 타격 횟수")]
    private int hitCount;

    [SerializeField]
    [Tooltip("궤도 표기 오브젝트")]
    private GameObject orbitalIndicationObj;

    [SerializeField]
    [Tooltip("레이저 애니메이션 오브젝트")]
    private GameObject laserAnimationObj;

    public List<GameObject> targetInRange = new List<GameObject>();
    
    public float launchAngle;

    [HideInInspector]
    public Vector2 onEnablePos; //활성화 시 초기 포지션

    private Vector3 onEnableRotation; //활성화 시 초기 회전값

    private WaitForSeconds orbitalIndicationDelay = new WaitForSeconds(1); //레이저 궤도 표기 시간

    private WaitForSeconds laserAnimDelay = new WaitForSeconds(0.5f); //레이저 발사 애니메이션 시간

    private WaitForSeconds laserHitDelay = new WaitForSeconds(0.2f);

    private void OnEnable()
    {
        StartCoroutine(OrbitalIndication());
    }

    IEnumerator OrbitalIndication()
    {
        yield return null;

        transform.position = onEnablePos;

        onEnableRotation.z = launchAngle;

        transform.rotation = Quaternion.Euler(onEnableRotation);

        yield return orbitalIndicationDelay;
        
        orbitalIndicationObj.SetActive(false);
        laserAnimationObj.SetActive(true);

        StartCoroutine(GiveDamage());
    }

    IEnumerator GiveDamage()
    {
        bool isCameraShaking = false;

        for (int nowHitCount = 0; nowHitCount < hitCount; nowHitCount++)
        {
            for (int nowIndex = 0; nowIndex < targetInRange.Count; nowIndex++) //공격
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
                }
            }

            if (hitCount > 1)
            {
                yield return laserHitDelay;
            }
        }

        for (int nowIndex = targetInRange.Count - 1; nowIndex >= 0; nowIndex--) //리스트 정리
        {
            targetInRange.Remove(targetInRange[nowIndex]);
        }

        yield return laserAnimDelay;

        orbitalIndicationObj.SetActive(true);
        laserAnimationObj.SetActive(false);

        ReturnToObjPool();
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
