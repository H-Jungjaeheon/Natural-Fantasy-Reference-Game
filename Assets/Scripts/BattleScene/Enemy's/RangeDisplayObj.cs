using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeDisplayObj : MonoBehaviour
{
    /// <summary>
    /// 범위 오브젝트 소환 시 세팅
    /// </summary>
    /// <param name="rangeScale"> 범위 크기 </param>
    /// <param name="destroyTime"> 범위 표시 지속시간 </param>
    /// <returns></returns>
    public void OnEnableSetting(Vector2 rangeScale, float destroyTime)
    {
        transform.localScale = rangeScale;
        StartCoroutine(DestroyTime(destroyTime));
    }

    /// <summary>
    /// 범위 표시 지속시간 세팅
    /// </summary>
    /// <param name="destroyTime"> 범위 표시 지속시간 </param>
    /// <returns></returns>
    public IEnumerator DestroyTime(float destroyTime)
    {
        yield return new WaitForSeconds(destroyTime);
        ObjectPool.Instance.ReturnObject(gameObject, (int)PoolObjKind.RangeDisplay);
    }
}
