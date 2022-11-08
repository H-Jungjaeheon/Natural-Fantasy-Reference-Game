using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [Tooltip("현재 선택한 스탯 텍스트")]
    private TextMeshProUGUI curChooseStatText;

    [SerializeField]
    [Tooltip("현재 선택한 스탯 레벨 텍스트")]
    private TextMeshProUGUI curChooseStatLevelText;

    [SerializeField]
    [Tooltip("현재 선택한 스탯 강화 내용 텍스트")]
    private TextMeshProUGUI curChooseStatEnhanceContentText;

    [SerializeField]
    [Tooltip("필요 재화 표기 텍스트")]
    private TextMeshProUGUI goodsRequiredForTheCurrentUpgradeText;

    private float[] currentLevelBarValues = new float[(int)UpgradeableStatKind.TotalStats];

    private int[] goodsRequiredForTheCurrentUpgrade = new int[(int)UpgradeableStatKind.TotalStats];

    private UpgradeableStatKind nowUpgradeableStatKind;

    private GameManager gameManager;

    private const int maxStatLevel = 10;

    private const float maxLevelBarValues = 1;

    private void Start()
    {
        gameManager = GameManager.Instance;
    }

    private void OnEnable()
    {
        OnEnableSetting();
    }

    private void OnEnableSetting()
    {
        TextsFixed(0); //처음 켰을 때 스탯 UI바 초기화(체력으로)
        for (int nowIndex = 0; nowIndex < (int)UpgradeableStatKind.TotalStats; nowIndex++) //레벨 바 이미지 fillamount 변경
        {
           
        }
    }

    public void ChangeStatsToUpgrade() //업그레이드할 스탯 변경(업그레이드 하고자 하는 스탯 버튼 클릭 시 실행)
    {
        
    }

    private void StatUpgradeButtonClick(int nowChooseStatIndex) //업그레이드 버튼 클릭
    {
        
    }

    private void FixedLevelBarFillAmount() //레벨 바 이미지 fillamount 변경
    {
        
    }


    private void TextsFixed(int nowChooseStatIndex) //텍스트 변경
    {
        //아직 강화에 필요한 재료 텍스트 수정 안넣음
        switch ((UpgradeableStatKind)nowChooseStatIndex)
        {
            case UpgradeableStatKind.Hp:
                curChooseStatText.text = "체력 강화";
                curChooseStatEnhanceContentText.text = $"체력이 {gameManager.statLevels[nowChooseStatIndex] * 10}% 상승합니다. > 체력이 {(gameManager.statLevels[nowChooseStatIndex] + 1) * 10}% 상승합니다.";
                break;
            case UpgradeableStatKind.Damage:
                curChooseStatText.text = "공격력 강화";
                curChooseStatEnhanceContentText.text = $"공격력이 {gameManager.statLevels[nowChooseStatIndex] * 4}% 상승합니다. > 체력이 {(gameManager.statLevels[nowChooseStatIndex] + 1) * 4}% 상승합니다.";
                break;
            case UpgradeableStatKind.Energy:
                curChooseStatText.text = "기력 강화";
                curChooseStatEnhanceContentText.text = $"기력이 {gameManager.statLevels[nowChooseStatIndex] * 3} 상승합니다. > 체력이 {(gameManager.statLevels[nowChooseStatIndex] + 1) * 3} 상승합니다.";
                break;
        }
        curChooseStatLevelText.text = $"LV. {gameManager.statLevels[nowChooseStatIndex]}/{maxStatLevel}";
    }
}
