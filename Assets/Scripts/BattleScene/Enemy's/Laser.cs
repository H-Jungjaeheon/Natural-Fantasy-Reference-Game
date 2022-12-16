using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectKind
{
    None,
    BurnEffect,
    SlowEffect
}

public enum DmgLimit
{
    Player,
    Enemy,
    All
}

public class Laser : MonoBehaviour
{
    [SerializeField]
    [Tooltip("오브젝트 풀의 총알 종류")]
    private PoolObjKind thisBulletPoolObjKind;

    [SerializeField]
    [Tooltip("해당 공격 피격 시 걸리는 효과")]
    private EffectKind effectKind;

    [SerializeField]
    [Tooltip("타격 시 피해 한정(타겟 한정)")]
    private DmgLimit dmgLimit;

    [SerializeField]
    [Tooltip("레이저 데미지")]
    private int damage;

    [SerializeField]
    [Tooltip("레이저 타격 횟수")]
    private int hitCount;

    [SerializeField]
    [Tooltip("발사 대기시간(범위 표시시간)")]
    private float waitTime;

    [SerializeField]
    [Tooltip("궤도 표기 오브젝트")]
    private GameObject orbitalIndicationObj;

    [SerializeField]
    [Tooltip("레이저 애니메이션 오브젝트")]
    private GameObject laserAnimationObj;

    public List<GameObject> targetInRange = new List<GameObject>();
    
    [HideInInspector]
    public float launchAngle;

    [HideInInspector]
    public Vector2 onEnablePos; //활성화 시 초기 포지션

    private Vector3 onEnableRotation; //활성화 시 초기 회전값

    private WaitForSeconds orbitalIndicationDelay; //레이저 궤도 표기 시간

    private WaitForSeconds laserAnimDelay = new WaitForSeconds(0.5f); //레이저 발사 애니메이션 시간

    private WaitForSeconds laserHitDelay = new WaitForSeconds(0.2f);

    private void Start()
    {
        orbitalIndicationDelay = new WaitForSeconds(waitTime);
    }

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
        if (collision.CompareTag("Player") && (dmgLimit == DmgLimit.Player || dmgLimit == DmgLimit.All))
        {

        }
        else if (collision.CompareTag("Enemy") && (dmgLimit == DmgLimit.Enemy || dmgLimit == DmgLimit.All))
        {

        }
        else
        {
            return;
        }

        targetInRange.Add(collision.gameObject);

        BasicUnitScript busComponent = collision.GetComponent<BasicUnitScript>();

        if (effectKind == EffectKind.SlowEffect)
        {
            busComponent.SlowDebuff(true, 60);
        }
        else if (effectKind == EffectKind.BurnEffect)
        {

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            BasicUnitScript busComponent = collision.GetComponent<BasicUnitScript>();
            if (effectKind == EffectKind.SlowEffect)
            {
                busComponent.SlowDebuff(false, 0);
            }
            else if (effectKind == EffectKind.BurnEffect)
            {

            }

            targetInRange.Remove(collision.gameObject);
        }
    }

    private void ReturnToObjPool() => ObjectPool.Instance.ReturnObject(gameObject, (int)thisBulletPoolObjKind);
}
