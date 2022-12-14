using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TextColors
{
    Red,
    Green
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
    [Tooltip("스탯 최대 레벨 달성시 없어지는 UI 오브젝트")]
    private GameObject disappearingAtHighestLevelObj;

    #region 필요 재화 표기 텍스트들 모음
    [Header("필요 재화 표기 텍스트들 모음")]
    [SerializeField]
    [Tooltip("필요 재화 표기 텍스트(기본 재화)")]
    private TextMeshProUGUI basicGoodsRequiredForTheCurrentUpgradeText;

    #endregion

    private float[] currentLevelBarValues = new float[(int)UpgradeableStatKind.TotalStats];

    private int[] goodsRequiredForTheCurrentUpgrade = new int[(int)UpgradeableStatKind.TotalStats];

    private UpgradeableStatKind nowUpgradeableStatKind;

    private GameManager gameManager;

    private const int maxStatLevel = 10;

    private const float maxLevelBarValues = 1;

    private void Awake()
    {
        gameManager = GameManager.Instance;
        ReflectLevelData(true, nowUpgradeableStatKind);
    }

    private void OnEnable()
    {
        TextsFixed(0); //처음 켰을 때 스탯 UI바 초기화(체력으로)
    }

    public void ChangeStatsToUpgrade(int nowChooseStatIndex) //업그레이드할 스탯 변경(업그레이드 하고자 하는 스탯 버튼 클릭 시 실행)
    {
        nowUpgradeableStatKind = (UpgradeableStatKind)nowChooseStatIndex;
        if (disappearingAtHighestLevelObj.activeSelf == false && gameManager.statLevels[nowChooseStatIndex] < maxStatLevel)
        {
            disappearingAtHighestLevelObj.SetActive(true);
        }
        TextsFixed(nowUpgradeableStatKind);
    }

    public void StatUpgradeButtonClick() //업그레이드 버튼 클릭 (나중에 특수 재화가 추가되면 따로 판별 변수 만들어서 비교)
    {
        if (gameManager.Gold >= goodsRequiredForTheCurrentUpgrade[(int)nowUpgradeableStatKind])
        {
            gameManager.Gold -= goodsRequiredForTheCurrentUpgrade[(int)nowUpgradeableStatKind];
            gameManager.statLevels[(int)nowUpgradeableStatKind]++;
            ReflectLevelData(false, nowUpgradeableStatKind);
        }
    }

    /// <summary>
    /// 스탯 데이터 수정(필요 재화, 레벨 그래프 수치 등등..)
    /// </summary>
    /// <param name="isEveryReflectLevelData"></param>
    /// <param name="nowReflectLevelData"></param>
    private void ReflectLevelData(bool isEveryReflectLevelData, UpgradeableStatKind nowReflectLevelData) //(참이면 모든 스탯 데이터 수정, 거짓이면 nowReflectLevelData를 통한 현재 고른 스탯 데이터만 수정)
    {
        if (isEveryReflectLevelData == false)
        {
            int nowReflectStatLevel = gameManager.statLevels[(int)nowReflectLevelData];

            goodsRequiredForTheCurrentUpgrade[(int)nowReflectLevelData] = 30 + (nowReflectStatLevel * 70);

            if (nowReflectStatLevel == 5 || nowReflectStatLevel == 8 || nowReflectStatLevel == 10)
            {
                currentLevelBarValues[(int)nowReflectLevelData] += 0.175f;
            }
            else
            {
                currentLevelBarValues[(int)nowReflectLevelData] += 0.075f;
            }

            statLevelGaugeImages[(int)nowReflectLevelData].fillAmount = currentLevelBarValues[(int)nowReflectLevelData] / maxLevelBarValues;

            TextsFixed(nowReflectLevelData);
        }
        else
        {
            for (int nowIndex = 0; nowIndex < (int)UpgradeableStatKind.TotalStats; nowIndex++) //레벨 데이터 가져와서 막대 표현과 필요 재화 표현
            {
                int nowReflectStatLevel = gameManager.statLevels[nowIndex];

                goodsRequiredForTheCurrentUpgrade[nowIndex] = 30 + (nowReflectStatLevel * 70);
                for (int nowLevelIndex = 1; nowLevelIndex <= nowReflectStatLevel; nowLevelIndex++)
                {
                    if (nowLevelIndex == 5 || nowLevelIndex == 8 || nowLevelIndex == 10)
                    {
                        currentLevelBarValues[nowIndex] += 0.175f;
                    }
                    else
                    {
                        currentLevelBarValues[nowIndex] += 0.075f;
                    }
                }
                statLevelGaugeImages[nowIndex].fillAmount = currentLevelBarValues[nowIndex] / maxLevelBarValues;
            }
        }
    }

    private void TextsFixed(UpgradeableStatKind nowUpgradeableStatKind) //텍스트 변경
    {
        int nowUpgradeableStatIndex = (int)nowUpgradeableStatKind;

        switch (nowUpgradeableStatKind)
        {
            case UpgradeableStatKind.Hp:
                curChooseStatText.text = "체력 강화";

                curChooseStatEnhanceContentText.text = (gameManager.statLevels[nowUpgradeableStatIndex] < maxStatLevel) ?
                    curChooseStatEnhanceContentText.text = $"체력이 {gameManager.statLevels[nowUpgradeableStatIndex] * 10}% 상승합니다. > 체력이 {(gameManager.statLevels[nowUpgradeableStatIndex] + 1) * 10}% 상승합니다." :
                    curChooseStatEnhanceContentText.text = $"체력이 {gameManager.statLevels[nowUpgradeableStatIndex] * 10}% 상승합니다.";

                break;
            case UpgradeableStatKind.Damage:
                curChooseStatText.text = "공격력 강화";

                curChooseStatEnhanceContentText.text = (gameManager.statLevels[nowUpgradeableStatIndex] < maxStatLevel) ?
                   curChooseStatEnhanceContentText.text = $"공격력이 {gameManager.statLevels[nowUpgradeableStatIndex] * 50}% 상승합니다. > 공격력이 {(gameManager.statLevels[nowUpgradeableStatIndex] + 1) * 50}% 상승합니다." :
                   curChooseStatEnhanceContentText.text = $"공격력이 {gameManager.statLevels[nowUpgradeableStatIndex] * 50}% 상승합니다.";

                break;
            case UpgradeableStatKind.Energy:
                curChooseStatText.text = "최대 기력 강화";

                curChooseStatEnhanceContentText.text = (gameManager.statLevels[nowUpgradeableStatIndex] < maxStatLevel) ?
                    curChooseStatEnhanceContentText.text = $"최대 기력이 {gameManager.statLevels[nowUpgradeableStatIndex] * 3} 상승합니다. > 최대 기력이 {(gameManager.statLevels[nowUpgradeableStatIndex] + 1) * 3} 상승합니다." :
                    curChooseStatEnhanceContentText.text = $"최대 기력이 {gameManager.statLevels[nowUpgradeableStatIndex] * 3} 상승합니다.";

                break;
        }

        curChooseStatLevelText.text = $"LV. {gameManager.statLevels[nowUpgradeableStatIndex]}/{maxStatLevel}"; //레벨 텍스트 수정

        if (gameManager.statLevels[nowUpgradeableStatIndex] == maxStatLevel)
        {
            disappearingAtHighestLevelObj.SetActive(false);
        }

        basicGoodsRequiredForTheCurrentUpgradeText.text = $"{goodsRequiredForTheCurrentUpgrade[nowUpgradeableStatIndex]}"; //업그레이드 필요 재화 텍스트 수정

        if (gameManager.Gold >= goodsRequiredForTheCurrentUpgrade[nowUpgradeableStatIndex])
        {
            basicGoodsRequiredForTheCurrentUpgradeText.color = textColors[(int)TextColors.Green];
        }
        else
        {
            basicGoodsRequiredForTheCurrentUpgradeText.color = textColors[(int)TextColors.Red];
        }
    }
}
