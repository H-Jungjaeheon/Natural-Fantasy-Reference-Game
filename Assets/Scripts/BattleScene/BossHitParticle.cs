using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossHitParticle : MonoBehaviour
{
    [SerializeField]
    [Tooltip("파티클 시스템 컴포넌트")]
    private ParticleSystem particle;

    WaitForSeconds endDealy = new WaitForSeconds(2);

    private void OnEnable()
    {
        particle.Play();
        StartCoroutine(ReturnPool());
    }

    private IEnumerator ReturnPool()
    {
        yield return endDealy;
        ObjectPool.Instance.ReturnObject(gameObject, (int)PoolObjKind.BossHitParticle);
    }
}
