using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public enum OptionPage
{
    FirstPage,
    SecondPage,
    ThirdPage,
    FourthPage,
    PageCount,
    None
}

public enum ScreenState
{
    MainScreen,
    StageSelectScreen,
    AdmissionCheckScreen,
    UpgradeScreen,
    OptionScreen
}

public class MainManager : Singleton<MainManager>
{
    private ScreenState nowScreenState;
    private OptionPage nowMainOptionState;

    [SerializeField]
    [Tooltip("스테이지 선택 창 오브젝트")]
    private GameObject stgaeSelectObj;

    [SerializeField]
    [Tooltip("스테이지 입장 확인 창 오브젝트")]
    private GameObject admissionCheckObj;

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

    [SerializeField]
    [Tooltip("기본 재화 텍스트")]
    private TextMeshProUGUI basicGoodsText;

    [SerializeField]
    [Tooltip("콘텐츠 버튼 오브젝트들 설명 텍스트")]
    private TextMeshProUGUI[] contentGuidanceTexts;

    private bool isFading;

    WaitForSeconds faidDelay = new WaitForSeconds(1);

    public void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else if (isDontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
    }

    private void Start()
    {
        GameManager.Instance.nowSceneState = NowSceneState.Main;
        nowMainOptionState = OptionPage.None;
        nowScreenState = ScreenState.MainScreen;
        
        BasicGoodsTextFixed();
        StartCoroutine(StartFaidAnim());
        StartCoroutine(GamePauseObjOnOrOff());
        StartCoroutine(ContentGuidanceTextsMove());
    }

    IEnumerator GamePauseObjOnOrOff()
    {
        while (true)
        {
            if (isFading == false)
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
                    if (nowMainOptionState == OptionPage.SecondPage || nowMainOptionState == OptionPage.ThirdPage)
                    {
                        StartCoroutine(PressEscToGamePausePageChange());
                        break;
                    }
                    else if (Input.GetKeyDown(KeyCode.Escape) && nowScreenState == ScreenState.MainScreen)
                    {
                        if (nowMainOptionState == OptionPage.None)
                        {
                            gamePauseObj[(int)OptionPage.FirstPage].SetActive(true);
                        }
                        else
                        {
                            gamePauseObj[(int)OptionPage.FirstPage].SetActive(false);
                        }
                        Time.timeScale = (nowMainOptionState == OptionPage.None) ? 0 : 1;
                        nowMainOptionState = (nowMainOptionState == OptionPage.None) ? OptionPage.FirstPage : OptionPage.None;
                    }
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

                if (nowMainOptionState == OptionPage.SecondPage || nowMainOptionState == OptionPage.ThirdPage)
                {
                    gamePauseObj[(int)nowMainOptionState].SetActive(false);
                    gamePauseObj[(int)OptionPage.FirstPage].SetActive(true);
                }

                nowMainOptionState = OptionPage.FirstPage;
                yield return null;
                StartCoroutine(GamePauseObjOnOrOff());
                break;
            }
            yield return null;
        }
    }

    IEnumerator ContentGuidanceTextsMove()
    {
        Vector2 textsMoveSpeed; //시작 위치에서 움직일 속도
        Vector2[] startPos = new Vector2[2]; //시작 위치

        textsMoveSpeed.x = 0; //x값은 움직이지 않음(초기화)

        for (int nowIndex = 0; nowIndex < contentGuidanceTexts.Length; nowIndex++) //시작 위치 세팅
        {
            startPos[nowIndex] = contentGuidanceTexts[nowIndex].transform.localPosition;
        }

        while (true) //텍스트 애니메이션 반복
        {
            for (int nowIndex = 0; nowIndex < contentGuidanceTexts.Length; nowIndex++)
            {
                textsMoveSpeed.y = Mathf.Sin(Time.time) * 10;

                contentGuidanceTexts[nowIndex].transform.localPosition = startPos[nowIndex] + textsMoveSpeed;
            }
            yield return null;
        }
    }

    IEnumerator StartFaidAnim()
    {
        Color color = faidOutImageColor;
        float nowImageAlpha = 1;

        isFading = true;

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

        isFading = false;

        faidOutObj.SetActive(false);
    }

    public void BasicGoodsTextFixed() => basicGoodsText.text = $"{GameManager.Instance.Gold}";

    public void PressToGamePausePageChangeButton(int nowChange)
    {
        for (int nowIndex = (int)OptionPage.FirstPage; nowIndex < (int)OptionPage.PageCount; nowIndex++)
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
        nowMainOptionState = (OptionPage)nowChange;
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
        nowScreenState = ScreenState.AdmissionCheckScreen;
        admissionCheckObj.SetActive(true);
        switch (StageIndexToEnter)
        {
            case 1:
                //스테이지 1 정보
                break;
        }
    }

    public void AdmissionCheck(bool isAdmissionBattle)
    {
        if (isAdmissionBattle)
        {
            StartCoroutine(MoveToStageAnim());
        }
        else
        {
            admissionCheckObj.SetActive(false);
            nowScreenState = ScreenState.StageSelectScreen;
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
