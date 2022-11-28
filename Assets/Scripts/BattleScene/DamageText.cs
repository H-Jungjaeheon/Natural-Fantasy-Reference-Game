using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
}
