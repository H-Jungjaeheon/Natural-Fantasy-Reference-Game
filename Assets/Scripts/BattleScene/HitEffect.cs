using System.Collections;
using UnityEngine;

public enum EffectType
{
    Slash,
    Shock,
    Defense
}

public class HitEffect : MonoBehaviour
{
    [Tooltip("이펙트 타입 표시")]
    public EffectType effectType;

    [SerializeField]
    [Tooltip("이펙트 애니메이터")]
    private Animator animator;

    private Vector3 nowRotation = new Vector3(0, 0, 0);

    private int rotationZ;

    #region 이펙트 애니메이션 트리거 문자열
    private const string slashAnim = "StartSlash";

    private const string shockAnim = "StartShock";
    #endregion

    private void OnEnable()
    {
        StartCoroutine(Classification());
    }

    /// <summary>
    /// 현재 이펙트 타입에 따라 구분 및 애니메이션 실행하는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator Classification()
    {
        yield return null;

        switch (effectType)
        {
            case EffectType.Slash:
                
                rotationZ = Random.Range(0, 360);

                nowRotation.z = rotationZ;
                animator.SetTrigger(slashAnim);
                break;

            case EffectType.Shock:

                nowRotation.z = 0;
                animator.SetTrigger(shockAnim);
                break;
        }

        transform.rotation = Quaternion.Euler(nowRotation);
    }

    public void ReturnToObjPool() => ObjectPool.Instance.ReturnObject(gameObject, (int)PoolObjKind.HitEffects);
}
