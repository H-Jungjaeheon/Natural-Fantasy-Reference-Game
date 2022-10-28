using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUIObj : MonoBehaviour
{
    [SerializeField]
    [Tooltip("기절 UI 애니메이션일 때 바뀔 오브젝트 크기")]
    private Vector3 faintAnimChangeScale;

    [SerializeField]
    [Tooltip("휴식 UI 애니메이션일 때 바뀔 오브젝트 크기")]
    private Vector3 restAnimChangeScale;

    public void ChangeFaintAnimObjScale()
    {
        transform.localScale = faintAnimChangeScale;
    }

    public void ChangeRestAnimObjScale()
    {
        transform.localScale = restAnimChangeScale;
    }
}
