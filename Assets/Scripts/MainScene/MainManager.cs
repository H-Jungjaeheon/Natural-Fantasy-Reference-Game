using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum BattleOrMainOptionState
{
    FirstPage,
    SecondPage,
    ThirdPage,
    PageCount,
    None
}

public enum ScreenState
{
    MainScreen,
    StageSelectScreen,
    UpgradeScreen,
    OptionScreen
}

public class MainManager : MonoBehaviour
{
    private ScreenState nowScreenState;

    [SerializeField]
    [Tooltip("스테이지 선택 창 오브젝트")]
    private GameObject stgaeSelectObj;

    [SerializeField]
    [Tooltip("업그레이드 시스템 창 오브젝트")]
    private GameObject upgradeSystemObj;

    [SerializeField]
    [Tooltip("페이드 아웃 오브젝트")]
    private GameObject faidOutObj;

    [SerializeField]
    [Tooltip("일시정지 창 오브젝트들")]
    private GameObject[] gamePauseObj;

    [SerializeField]
    [Tooltip("페이드 아웃 이미지")]
    private Image faidOutImage;

    [SerializeField]
    [Tooltip("페이드 아웃 이미지 컬러")]
    private Color faidOutImageColor;

    private BattleOrMainOptionState nowMainOptionState;

    WaitForSeconds faidDelay = new WaitForSeconds(1);

    private void Start()
    {
        StartCoroutine(StartFaidAnim());
        StartCoroutine(GamePauseObjOnOrOff());
    }

    IEnumerator GamePauseObjOnOrOff()
    {
        while (true)
        {
            if (nowScreenState != ScreenState.MainScreen)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    switch (nowScreenState)
                    {
                        case ScreenState.StageSelectScreen:
                            stgaeSelectObj.SetActive(false);
                            nowScreenState = ScreenState.MainScreen;
                            break;
                        case ScreenState.UpgradeScreen:
                            upgradeSystemObj.SetActive(false);
                            nowScreenState = ScreenState.MainScreen;
                            break;
                    }
                }
            }
            else
            {
                if (nowMainOptionState == BattleOrMainOptionState.SecondPage || nowMainOptionState == BattleOrMainOptionState.ThirdPage)
                {
                    StartCoroutine(PressEscToGamePausePageChange());
                    break;
                }
                else if (Input.GetKeyDown(KeyCode.Escape) && nowScreenState == ScreenState.MainScreen)
                {
                    if (nowMainOptionState == BattleOrMainOptionState.None)
                    {
                        gamePauseObj[(int)BattleOrMainOptionState.FirstPage].SetActive(true);
                    }
                    else
                    {
                        gamePauseObj[(int)BattleOrMainOptionState.FirstPage].SetActive(false);
                    }
                    Time.timeScale = (nowMainOptionState == BattleOrMainOptionState.None) ? 0 : 1;
                    nowMainOptionState = (nowMainOptionState == BattleOrMainOptionState.None) ? BattleOrMainOptionState.FirstPage : BattleOrMainOptionState.None;
                }
            }
            yield return null;
        }
    }

    IEnumerator PressEscToGamePausePageChange()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (nowMainOptionState == BattleOrMainOptionState.SecondPage)
                {
                    gamePauseObj[(int)BattleOrMainOptionState.SecondPage].SetActive(false);
                    gamePauseObj[(int)BattleOrMainOptionState.FirstPage].SetActive(true);
                }
                else if(nowMainOptionState == BattleOrMainOptionState.ThirdPage)
                {
                    gamePauseObj[(int)BattleOrMainOptionState.ThirdPage].SetActive(false);
                    gamePauseObj[(int)BattleOrMainOptionState.FirstPage].SetActive(true);
                }
                nowMainOptionState = BattleOrMainOptionState.FirstPage;
                yield return null;
                StartCoroutine(GamePauseObjOnOrOff());
                break;
            }
            yield return null;
        }
    }

    IEnumerator StartFaidAnim()
    {
        Color color = faidOutImageColor;
        float nowImageAlpha = 1;

        faidOutObj.SetActive(true);
        faidOutImageColor.a = 1;
        faidOutImage.color = faidOutImageColor;

        yield return faidDelay;

        while (nowImageAlpha > 0)
        {
            nowImageAlpha -= Time.deltaTime;
            color.a = nowImageAlpha;
            faidOutImage.color = color;
            yield return null;
        }

        faidOutObj.SetActive(false);
    }

    public void PressToGamePausePageChangeButton(int nowChange)
    {
        for (int nowIndex = (int)BattleOrMainOptionState.FirstPage; nowIndex < (int)BattleOrMainOptionState.PageCount; nowIndex++)
        {
            if (nowIndex == nowChange)
            {
                gamePauseObj[nowIndex].SetActive(true);
            }
            else
            {
                gamePauseObj[nowIndex].SetActive(false);
            }
        }
        nowMainOptionState = (BattleOrMainOptionState)nowChange;
    }

    public void PressContentButton(int nowPressContentIndex)
    {
        switch (nowPressContentIndex)
        {
            case (int)ScreenState.StageSelectScreen:
                stgaeSelectObj.SetActive(true);
                break;
            case (int)ScreenState.UpgradeScreen:
                upgradeSystemObj.SetActive(true);
                break;
        }
        nowScreenState = (ScreenState)nowPressContentIndex;
    }

    public void MoveToBattleScene(int StageIndexToEnter)
    {
        switch (StageIndexToEnter) //이곳에서 입장하려는 스테이지에 따라 나중에 스테이지 정보 넣기 (입장 재확인 창 띄우기)
        {
            case 1:
                StartCoroutine(MoveToStageAnim());
                break;
        }
    }

    IEnumerator MoveToStageAnim()
    {
        Color color = faidOutImageColor;
        float nowImageAlpha = 0;

        faidOutObj.SetActive(true);
        faidOutImageColor.a = 0;
        faidOutImage.color = faidOutImageColor;

        while (nowImageAlpha < 1)
        {
            nowImageAlpha += Time.deltaTime;
            color.a = nowImageAlpha;
            faidOutImage.color = color;
            yield return null;
        }

        yield return faidDelay;

        SceneManager.LoadScene("BattleScene");
    }

    public void GameQuit()
    {
        Application.Quit();
    }
}
