using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum StatKind
{
    Hp,
    Damage,
    Energy,
    TotalStats
}

public enum TextColors
{
    Red,
    Green,
    Black
}

public class StatUpgrade : MonoBehaviour
{
    [SerializeField]
    [Tooltip("스탯 레벨 게이지 이미지들")]
    private Image[] statLevelGaugeImages;

    [SerializeField]
    [Tooltip("변경할 텍스트 색상")]
    private Color[] textColors;

    [SerializeField]
    [Tooltip("필요 재화 표기 텍스트")]
    private TextMeshProUGUI goodsRequiredForTheCurrentUpgradeText;

    private float[] currentLevelValues = new float[(int)StatKind.TotalStats];

    private int[] goodsRequiredForTheCurrentUpgrade = new int[(int)StatKind.TotalStats];


    public void ChangeStatsToUpgrade()
    {
        
    }

    public void Upgrade()
    {
        
    }

    public void TextsFixed()
    {
        
    }
}
