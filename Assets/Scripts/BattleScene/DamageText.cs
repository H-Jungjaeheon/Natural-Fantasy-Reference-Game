using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum TextState
{
    BasicDamage,
    CriticalDamage,
    Blocking
}

public enum RandMotionEffects
{
    None,
    Left,
    Right
}

public class DamageText : MonoBehaviour
{
    [SerializeField]
    [Tooltip("데미지 표기 텍스트")]
    private TextMeshProUGUI thisDamageText;

    [SerializeField]
    [Tooltip("텍스트 색 모음")]
    private Color[] textColors;

    [SerializeField]
    [Tooltip("텍스트 상태별 텍스트 크기 모음")]
    private float[] textSizes;

    [SerializeField]
    [Tooltip("자신의 리지드바디")]
    protected Rigidbody2D rigid;

    private int randIndex; //텍스트 이동(왼, 오)효과 랜덤 뽑기 인덱스

    private WaitForSeconds textMoveDelay = new WaitForSeconds(0.6f);

    private void OnEnable()
    {
        StartCoroutine(TextMoving());
    }

    /// <summary>
    /// 데미지 텍스트 소환 시 텍스트 커스텀 함수
    /// </summary>
    /// <param name="nowTextState"> 현재 데미지 텍스트 상태  </param>
    /// <param name="spawnPos"> 데미지 텍스트 스폰 위치 </param>
    /// <param name="damage"> 텍스트로 표현될 데미지 </param>
    public void TextCustom(TextState nowTextState, Vector3 spawnPos, float damage)
    {
        transform.position = spawnPos;

        thisDamageText.text = (nowTextState == TextState.Blocking) ? "Blocking!" : $"{damage}";

        thisDamageText.color = textColors[(int)nowTextState];

        thisDamageText.fontSize = textSizes[(int)nowTextState];
    }

    /// <summary>
    /// 텍스트 움직임 함수
    /// </summary>
    /// <returns></returns>
    private IEnumerator TextMoving()
    {
        randIndex = Random.Range(0, 3);

        switch (randIndex)
        {
            case (int)RandMotionEffects.Left:
                rigid.AddForce(Vector2.left * 0.5f, ForceMode2D.Impulse);
                break;
            case (int)RandMotionEffects.Right:
                rigid.AddForce(Vector2.right * 0.5f, ForceMode2D.Impulse);
                break;
        }

        rigid.AddForce(Vector2.up * 25, ForceMode2D.Impulse);

        yield return textMoveDelay;

        rigid.velocity = Vector2.zero;

        ReturnToObjPool();
    }

    private void ReturnToObjPool() => ObjectPool.Instance.ReturnObject(gameObject, (int)PoolObjKind.DamageText);
}