using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ButtonPage
{
    FirstPage,
    SecondPage
}

public class BattleButtonManager : MonoBehaviour
{
    private int nowPage;
    private int maxPage;
    private int minPage;
    #region 행동 버튼모음
    [Header("행동 버튼모음")]
    [Tooltip("기본공격 버튼")]
    [SerializeField]
    private Button BasicAttackButton;

    [Tooltip("스킬선택 버튼")]
    [SerializeField]
    private Button SkillChooseButton;

    [Tooltip("스킬버튼들")]
    [SerializeField]
    private Button[] SkillButtons = new Button[7];

    [Tooltip("휴식 버튼")]
    [SerializeField]
    private Button RestButton;

    [Tooltip("스킬선택 나가기 버튼")]
    [SerializeField]
    private Button OutSkillChooseButton;
    #endregion

    [Header("플레이어 오브젝트")]
    [SerializeField]
    private GameObject Player;

    [Header("각 버튼들 페이지 오브젝트")]
    [SerializeField]
    private GameObject[] ButtonPageObjs;

    Player playerComponent;

    // Start is called before the first frame update
    void Start()
    {
        StartSetting();
        ButtonsSetting();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void StartSetting()
    {
        playerComponent = Player.GetComponent<Player>();
        nowPage = 0;
        minPage = 0;
        maxPage = 1; //+스킬 페이지 많을 수록 수 더하기
    }

    private void ButtonsSetting()
    {
        BasicAttackButton.onClick.AddListener(() => playerComponent.CloseAttackStart());
        SkillChooseButton.onClick.AddListener(() => ButtonsPageChange(true, false));
        OutSkillChooseButton.onClick.AddListener(() => ButtonsPageChange(false, true));
        //SkillButtons[0].onClick.AddListener(() => print("Skills"));
        RestButton.onClick.AddListener(() => playerComponent.RestStart());
    }

    public void ButtonsPageChange(bool isTurnToNextPage, bool isExitSkillButtonPage)
    {
        if (isExitSkillButtonPage)
        {
            playerComponent.isSkillButtonPage = false;
            ButtonPageObjs[(int)ButtonPage.FirstPage].SetActive(true);
            for (int nowPageIndex = (int)ButtonPage.SecondPage; nowPageIndex <= maxPage; nowPageIndex++)
            {
                ButtonPageObjs[nowPageIndex].SetActive(false);
            }
            nowPage = minPage;
        }
        else if (isTurnToNextPage)
        {
            playerComponent.isSkillButtonPage = true;
            int nextPageIndex = nowPage + 1;
            if (nextPageIndex <= maxPage)
            {
                for (int nowPageIndex = (int)ButtonPage.FirstPage; nowPageIndex <= maxPage; nowPageIndex++)
                {
                    if (nowPageIndex == nextPageIndex)
                    {
                        ButtonPageObjs[nowPageIndex].SetActive(true);
                    }
                    else
                    {
                        ButtonPageObjs[nowPageIndex].SetActive(false);
                    }
                }
                nowPage++;
            }
            else 
            {
                ButtonPageObjs[(int)ButtonPage.SecondPage].SetActive(true);
                for (int nowPageIndex = minPage; nowPageIndex <= maxPage; nowPageIndex++)
                {
                    if (nowPageIndex == (int)ButtonPage.SecondPage)
                    {
                        continue;
                    }
                    else
                    {
                        ButtonPageObjs[nowPageIndex].SetActive(false);
                    }
                }
                nowPage = (int)ButtonPage.SecondPage;
            }
        }
        else if(isTurnToNextPage == false)
        {
            int nextPageIndex = nowPage - 1;
            if (nextPageIndex >= minPage)
            {
                for (int nowPageIndex = (int)ButtonPage.FirstPage; nowPageIndex <= maxPage; nowPageIndex++)
                {
                    if (nowPageIndex == nextPageIndex)
                    {
                        ButtonPageObjs[nowPageIndex].SetActive(true);
                    }
                    else
                    {
                        ButtonPageObjs[nowPageIndex].SetActive(false);
                    }
                }
                nowPage--;
            }
            else
            {
                for (int nowPageIndex = minPage; nowPageIndex <= maxPage; nowPageIndex++)
                {
                    if (nowPageIndex == maxPage)
                    {
                        ButtonPageObjs[nowPageIndex].SetActive(true);
                    }
                    else
                    {
                        ButtonPageObjs[nowPageIndex].SetActive(false);
                    }
                }
                nowPage = maxPage;
            }
        }
    }
}
