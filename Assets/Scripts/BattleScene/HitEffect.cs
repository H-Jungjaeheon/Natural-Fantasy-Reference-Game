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
                animator.SetTrigger(slashAnim);
                break;
            case EffectType.Shock:
                animator.SetTrigger(shockAnim);
                break;
        }
    }

    public void ReturnToObjPool() => ObjectPool.Instance.ReturnObject(gameObject, (int)PoolObjKind.HitEffects);
}
