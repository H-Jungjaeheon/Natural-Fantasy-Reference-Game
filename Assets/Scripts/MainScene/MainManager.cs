using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Update()
    {
        WaitUntilPressEsc();
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
}
