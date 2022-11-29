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

public class DamageText : MonoBehaviour
{
    [SerializeField]
    [Tooltip("데미지 표기 텍스트")]
    private TextMeshProUGUI thisDamageText;

    [SerializeField]
    [Tooltip("텍스트 색 모음")]
    private Color[] textColors;

    //위로 올라갔다가 내려오고 없어지는 애니메이션 함수
    //텍스트 커스텀(상황에 따라서 다르게 바꿀) 함수

    /// <summary>
    /// 데미지 텍스트 소환 시 텍스트 커스텀 함수
    /// </summary>
    /// <param name="spawnPos"> 처음 소환 위치 </param>
    /// <param name="isBlocking"> 무적 표기 판별 </param>
    /// <param name="damage"> 현재 표기될 데미지 </param>
    /// <param name="nowColor"> 현재 데미지 텍스트 색 </param>
    private void TextCustom(Vector2 spawnPos, bool isBlocking, float damage, Color nowColor)
    {
        transform.position = spawnPos;

        thisDamageText.color = nowColor;

        thisDamageText.text = (isBlocking) ? "Blocking!" : $"{damage}";

    }
}