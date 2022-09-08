using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ButtonPage
{
    FirstPage,
    SecondPage
}

public enum Skills
{
    FirstSkill
}

public class BattleButtonManager : Singleton<BattleButtonManager>
{
    #region 행동 버튼모음
    [Header("행동 버튼모음")]
    [Tooltip("스킬선택 버튼")]
    [SerializeField]
    private Button SkillChooseButton;

    [Tooltip("스킬선택 나가기 버튼")]
    [SerializeField]
    private Button OutSkillChooseButton;

    [Tooltip("스킬버튼들")]
    [SerializeField]
    private Button[] SkillButtons = new Button[7];
    #endregion

    [Header("각 버튼들 페이지 오브젝트")]
    [SerializeField]
    private GameObject[] ButtonPageObjs = new GameObject[2];

    [SerializeField]
    [Tooltip("스킬 스크롤러 조절 오브젝트")]
    private GameObject content;

    public ButtonPage nowButtonPage;

    private Player playerComponent;

    // Start is called before the first frame update
    void Start()
    {
        StartSetting();
    }

    private void StartSetting()
    {
        playerComponent = BattleSceneManager.Instance.Player;
        SkillChooseButton.onClick.AddListener(() => ActionButtonsSetActive(false, true, true));
        OutSkillChooseButton.onClick.AddListener(() => ActionButtonsSetActive(true, false, false));
        SkillButtons[(int)Skills.FirstSkill].onClick.AddListener(() => playerComponent.SkillUse(1, 5));
    }

    /// <summary>
    /// 매개변수 : 기본 페이지 활성화 여부, 스킬 페이지 활성화 여부, 스킬 스크롤 초기화 여부
    /// </summary>
    /// <param name="firstPageSetActive"></param>
    /// <param name="secondPageSetActive"></param>
    public void ActionButtonsSetActive(bool firstPageSetActive, bool secondPageSetActive, bool isScrollInitialization) 
    {
        if (firstPageSetActive)
        {
            nowButtonPage = ButtonPage.FirstPage;
        }
        else if(secondPageSetActive)
        {
            nowButtonPage = ButtonPage.SecondPage;
            if (isScrollInitialization)
            {
                content.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 20, 0);
            }
        }
        ButtonPageObjs[(int)ButtonPage.FirstPage].SetActive(firstPageSetActive);
        ButtonPageObjs[(int)ButtonPage.SecondPage].SetActive(secondPageSetActive);
    }
}
