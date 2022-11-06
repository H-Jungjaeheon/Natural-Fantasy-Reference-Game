using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    private NowOptionState nowOptionState;

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
    [Tooltip("페이드 아웃 이미지")]
    private Image faidOutImage;

    [SerializeField]
    [Tooltip("페이드 아웃 이미지 컬러")]
    private Color faidOutImageColor;

    WaitForSeconds faidDelay = new WaitForSeconds(1);

    private void Start()
    {
        StartCoroutine(StartFaidAnim());
    }

    private void Update()
    {
        WaitUntilPressEsc();
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

    private void WaitUntilPressEsc()
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
}
