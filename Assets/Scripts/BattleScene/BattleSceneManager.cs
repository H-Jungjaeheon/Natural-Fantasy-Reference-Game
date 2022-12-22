using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public enum UnitKind
{
    Player,
    Enemy
}

public enum NowGameState
{
    GameReady,
    Playing,
    Pausing,
    GameEnd
}

public enum StageKind
{
    Tutorial,
    Stage1,
    Stage2,
    Stage3,
    Stage4,
    Stage5,
    Stage6,
    Stage7
}

public enum Colors
{
    Black,
    White,
    Red,
    Yellow
}

public class BattleSceneManager : Singleton<BattleSceneManager> //나중에 게임 오버 및 게임 클리어, 재화 관리
{
    [System.Serializable]
    public class StageData
    {
        [Tooltip("보스 오브젝트 모음")]
        public GameObject bossObjs;

        [Tooltip("스테이지 기믹 오브젝트 모음")]
        public GameObject gimmickObjs;

        [Tooltip("스테이지 배경 오브젝트 모음")]
        public GameObject bgResources;
    }

    [SerializeField]
    [Tooltip("각 스테이지별 변경될 데이터들")]
    private StageData[] stageData; 

    [HideInInspector]
    public Vector2 playerCharacterPos; //플레이어 시작 포지션

    [HideInInspector]
    public Vector2 enemyCharacterPos; //적 시작 포지션

    private int nowGetBasicGood; //현재 스테이지에서 얻은 재화 개수

    public int NowGetBasicGood
    {
        get { return nowGetBasicGood; }
        set { nowGetBasicGood = value; }
    }

    [SerializeField]
    [Tooltip("게임 종료 및 클리어시 비활성화 가능한 오브젝트들")]
    private GameObject deActivableObj;

    [SerializeField]
    [Tooltip("게임 일시정지 판넬 오브젝트들")]
    private GameObject[] gamePauseObj;

    [SerializeField]
    [Tooltip("게임종료 시 띄울 판넬 오브젝트")]
    private GameObject gameEndObj;

    #region 화면 페이드(Fade) 연출 관련
    [Header("화면 페이드(Fade) 연출 관련")]

    [SerializeField]
    [Tooltip("페이드에 쓰일 오브젝트")]
    private GameObject fadeObj;

    [SerializeField]
    [Tooltip("페이드에 쓰일 이미지")]
    private Image fadeImage;

    [SerializeField]
    [Tooltip("색 모음")]
    private Color[] colors;

    private Color nowColor; //이미지에 적용할 색

    float nowAlpha; //이미지에 적용할 알파값
    #endregion

    #region 스테이지 시작 연출 관련
    [Header("스테이지 시작 연출 관련")]

    [SerializeField]
    [Tooltip("스테이지 및 보스 소개 연출 이미지")]
    private Image introducingTheStageImage;

    [SerializeField]
    [Tooltip("스테이지 및 보스 소개 텍스트")]
    private TextMeshProUGUI introducingTheStageText;

    private bool isIntroducing;

    private IEnumerator introCoroutine;
    #endregion

    #region 게임 오버 관련 
    [Header("게임 오버 관련")]

    [SerializeField]
    [Tooltip("게임 오버 텍스트")]
    private TextMeshProUGUI gameOverText;

    [SerializeField]
    [Tooltip("획득한 기본 재화 텍스트")]
    private TextMeshProUGUI obtainText;

    [SerializeField]
    [Tooltip("안내 텍스트")]
    private TextMeshProUGUI guideText;

    [SerializeField]
    [Tooltip("재화 획득 표기 오브젝트")]
    private GameObject obtainObj;
    #endregion

    #region 재화 획득 애니메이션 관련 
    [Header("재화 획득 애니메이션 관련")]

    [SerializeField]
    [Tooltip("재화 획득 애니메이션 오브젝트")]
    private GameObject getGoodObj;

    [SerializeField]
    [Tooltip("재화 획득 애니메이터")]
    private Animator getGoodAnim;

    [SerializeField]
    [Tooltip("획득 재화 개수 표시 텍스트")]
    private TextMeshProUGUI goodAmountText;
    #endregion

    [Tooltip("스탯(플레이어, 보스) UI 오브젝트")]
    public GameObject statUIObj;

    [SerializeField]
    [Tooltip("카메라 흔들림 컴포넌트")]
    private CamShake csComponent;

    public Player player;

    [HideInInspector]
    public GameObject enemy;

    [HideInInspector]
    public NowGameState nowGameState;

    private OptionPage nowBattleSceneOptionState;

    private Camera mainCam;

    private GameManager gmInstance;

    private BattleButtonManager bbmInstance;

    public void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else if(isDontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
    }

    private void Start()
    {
        mainCam = Camera.main;
        gmInstance = GameManager.Instance;
        bbmInstance = BattleButtonManager.Instance;

        stageData[(int)StageKind.Stage1].bossObjs.SetActive(true); //나중에 게임 매니저에서 현재 선택한 스테이지 인덱스 받아서 실행하기

        gmInstance.nowScene = SceneKind.Ingame;

        nowBattleSceneOptionState = OptionPage.None;

        playerCharacterPos = player.transform.position;

        StartCoroutine(StartFaidAnim());
        StartCoroutine(GamePauseObjOnOrOff());
    }

    /// <summary>
    /// 일시정지 화면 띄우기(화면 끄기 및 세팅 등의 화면 넘기기)
    /// </summary>
    /// <returns></returns>
    IEnumerator GamePauseObjOnOrOff()
    {
        while (true)
        {
            if (nowBattleSceneOptionState == OptionPage.SecondPage || nowBattleSceneOptionState == OptionPage.ThirdPage || nowBattleSceneOptionState == OptionPage.FourthPage)
            {
                if (nowBattleSceneOptionState != OptionPage.SecondPage)
                {
                    StartCoroutine(PressEscToGamePausePageChange());
                }
                break;
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && nowGameState != NowGameState.GameEnd && nowGameState != NowGameState.GameReady)
            {
                bool isNonePage = (nowBattleSceneOptionState == OptionPage.None); //현재 아무 페이지도 띄우고 있지 않다면 참(띄우고 있다면 거짓)

                gamePauseObj[(int)OptionPage.FirstPage].SetActive(isNonePage); //아무 페이지도 띄우고 있지 않은 상태로 ESC키를 누르면 옵션(1페이지)창 띄우기(띄우고 있다면 옵션창 닫기)

                yield return null;

                Time.timeScale = (nowBattleSceneOptionState == OptionPage.None) ? 0 : 1;
                nowBattleSceneOptionState = (nowBattleSceneOptionState == OptionPage.None) ? OptionPage.FirstPage : OptionPage.None;
                nowGameState = (nowGameState == NowGameState.Playing) ? NowGameState.Pausing : NowGameState.Playing;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 옵션 창 띄울 때 ESC 작동
    /// </summary>
    /// <returns></returns>
    IEnumerator PressEscToGamePausePageChange()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                gamePauseObj[(int)nowBattleSceneOptionState].SetActive(false);
                gamePauseObj[(int)OptionPage.FirstPage].SetActive(true);
                
                nowBattleSceneOptionState = OptionPage.FirstPage;

                yield return null;

                StartCoroutine(GamePauseObjOnOrOff());

                break;
            }
            yield return null;
        }
    }

    public void PressToGamePausePageChangeButton(int nowChange)
    {
        bool isNowPage = false;

        for (int nowIndex = (int)OptionPage.FirstPage; nowIndex < (int)OptionPage.PageCount; nowIndex++)
        {
            isNowPage = (nowIndex == nowChange); //현재 인덱스가 현재 페이지 인덱스와 일치한다면, 현재 인덱스 페이지 활성화(다르다면, 비활성화)

            gamePauseObj[nowIndex].SetActive(isNowPage);
        }

        nowBattleSceneOptionState = (OptionPage)nowChange;

        if (nowChange == (int)OptionPage.FirstPage)
        {
            StartCoroutine(GamePauseObjOnOrOff());
        }
    }

    /// <summary>
    /// 전투 시작 시 페이드 연출
    /// </summary>
    /// <returns></returns>
    IEnumerator StartFaidAnim()
    {
        WaitForSeconds animDelay = new WaitForSeconds(1f);

        nowColor = colors[(int)Colors.Black];
        nowAlpha = 1;

        bbmInstance.ActionButtonSetActive(false);

        statUIObj.SetActive(false);

        nowGameState = NowGameState.GameReady;

        fadeObj.SetActive(true);
        nowColor.a = 1;
        fadeImage.color = nowColor;

        yield return animDelay;

        while (nowAlpha > 0)
        {
            nowAlpha -= Time.deltaTime;
            nowColor.a = nowAlpha;
            fadeImage.color = nowColor;
            yield return null;
        }

        yield return animDelay;

        introCoroutine = IntroAnim();
        StartCoroutine(introCoroutine);
    }

    IEnumerator IntroSkip()
    {
        while (isIntroducing)
        {
            if (Input.anyKeyDown)
            {
                float duration = 0;

                StopCoroutine(introCoroutine);
                DOTween.PauseAll();

                if (introducingTheStageImage.rectTransform.anchoredPosition.x >= -1000) //인트로 애니메이션이 시작된지 얼마 되지 않았다면 이미지 바로 사라짐
                {
                    duration = 0.25f;
                }

                introducingTheStageImage.rectTransform.DOAnchorPosX(1920f, duration);
                introducingTheStageText.rectTransform.DOAnchorPosX(-1920f, duration);

                StartCoroutine(EndIntroAnim());
                break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 스테이지 인트로 애니메이션 함수(보스 소개)
    /// </summary>
    /// <returns></returns>
    IEnumerator IntroAnim()
    {
        Vector3 camMoveSpeed = new Vector3(7f, 0f, 0f);
        Vector3 camTargetPos = new Vector3(4f, 0.5f, -10f);

        isIntroducing = true;

        StartCoroutine(IntroSkip());

        nowColor = colors[(int)Colors.Black];
        nowAlpha = 0;

        while (mainCam.transform.position.x < 4f)
        {
            mainCam.transform.position += camMoveSpeed * Time.deltaTime;
            mainCam.orthographicSize -= Time.deltaTime * 3.5f;
            yield return null;
        }

        mainCam.transform.position = camTargetPos;
        mainCam.orthographicSize = 7.5f;

        while (nowAlpha < 0.55f)
        {
            nowAlpha += Time.deltaTime * 3;
            nowColor.a = nowAlpha;
            fadeImage.color = nowColor;
            yield return null;
        }

        introducingTheStageImage.rectTransform.DOAnchorPosX(0f, 0.3f);
        introducingTheStageText.rectTransform.DOAnchorPosX(0f, 0.3f);

        while (introducingTheStageImage.rectTransform.anchoredPosition.x < 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(3f);

        introducingTheStageImage.rectTransform.DOAnchorPosX(1920f, 0.25f);
        introducingTheStageText.rectTransform.DOAnchorPosX(-1920f, 0.25f);

        StartCoroutine(EndIntroAnim());
    }

    IEnumerator EndIntroAnim()
    {
        Vector3 camMoveSpeed = new Vector3(7f, 0f, 0f);
        Vector3 camTargetPos = new Vector3(0f, 0.5f, -10f);

        while (nowAlpha > 0)
        {
            nowAlpha -= Time.deltaTime * 3;
            nowColor.a = nowAlpha;
            fadeImage.color = nowColor;
            yield return null;
        }

        while (introducingTheStageImage.rectTransform.anchoredPosition.x < 1910)
        {
            yield return null;
        }

        while (mainCam.transform.position.x > 0) //카메라 확대에서 원래 시야만큼 돌리기
        {
            mainCam.transform.position -= camMoveSpeed * (Time.deltaTime * 3);
            mainCam.orthographicSize += Time.deltaTime * 10.5f;
            yield return null;
        }

        mainCam.transform.position = camTargetPos;
        mainCam.orthographicSize = 9.5f;

        yield return new WaitForSeconds(1f);

        fadeObj.SetActive(false);
        nowGameState = NowGameState.Playing;

        statUIObj.SetActive(true);

        bbmInstance.ActionButtonSetActive(true);

        isIntroducing = false;

        stageData[(int)StageKind.Stage1].gimmickObjs.SetActive(true);
    }

    public void GameExit() => StartCoroutine(SceneChangeFaidOut(SceneKind.Main));
    
    IEnumerator SceneChangeFaidOut(SceneKind changeScene)
    {
        WaitForSecondsRealtime faidDelay = new WaitForSecondsRealtime(0.01f);
        float nowAlphaPlusPerSecond = 0.025f;

        nowColor = colors[(int)Colors.Black];
        nowAlpha = 0;

        fadeImage.transform.SetAsLastSibling();
        fadeObj.SetActive(true);

        while (nowAlpha < 1)
        {
            nowColor.a = nowAlpha;
            fadeImage.color = nowColor;

            nowAlpha += nowAlphaPlusPerSecond;
            yield return faidDelay;
        }

        Time.timeScale = 1;
        SceneManager.LoadScene((int)changeScene);
    }

    /// <summary>
    /// 게임 마무리 애니메이션 함수
    /// </summary>
    /// <param name="isGameOver"> 게임오버 판별 </param>
    public void StartGameEndPanelAnim(bool isGameOver)
    {
        nowGameState = NowGameState.GameEnd;

        if (isGameOver)
        {
            StartCoroutine(PlayerDeadAnim());
        }
        else
        {
            csComponent.GameEndSetting();

            statUIObj.SetActive(false);
            bbmInstance.ActionButtonSetActive(false);

            player.GameClearSetting();

            StartCoroutine(BossDeadAnim());
        }

    }

    /// <summary>
    /// 플레이어 사망(게임 오버)시 페이드 애니메이션 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayerDeadAnim()
    {
        nowColor = colors[(int)Colors.White];
        nowAlpha = 1;

        CamShake.JumpStop(true);

        yield return null;

        deActivableObj.SetActive(false);
        fadeObj.SetActive(true);
        fadeImage.color = nowColor;

        gameEndObj.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        nowColor = colors[(int)Colors.Red];

        while (nowAlpha > 0)
        {
            fadeImage.color = nowColor;
            nowColor.a = nowAlpha;
            nowAlpha -= Time.deltaTime;
            yield return null;
        }

        fadeObj.SetActive(false);

        StartCoroutine(SameEndAnim());
    }

    IEnumerator BossDeadAnim()
    {
        nowColor = colors[(int)Colors.White];
        nowAlpha = 0;

        fadeObj.SetActive(true);

        while (nowAlpha < 1)
        {
            if (mainCam.orthographicSize > 6.5f)
            {
                mainCam.orthographicSize -= Time.deltaTime * 0.15f;
            }

            nowColor.a = nowAlpha;
            fadeImage.color = nowColor;

            if (nowAlpha < 0.4f)
            {
                nowAlpha += Time.deltaTime * 0.07f;
            }
            else
            {
                nowAlpha += Time.deltaTime * 0.5f;
            }

            yield return null;
        }

        gameEndObj.SetActive(true);
        gameOverText.text = "Stage 1 Clear!";

        yield return new WaitForSeconds(1f);

        while (nowAlpha > 0)
        {
            fadeImage.color = nowColor;
            nowColor.a = nowAlpha;
            nowAlpha -= Time.deltaTime;
            yield return null;
        }

        fadeObj.SetActive(false);

        StartCoroutine(SameEndAnim());
    }

    IEnumerator SameEndAnim() //게임 종료 판넬 애니메이션 (공통)
    {
        WaitForSeconds animDelay = new WaitForSeconds(1f);

        nowColor = colors[(int)Colors.White];

        nowAlpha = 0;

        yield return new WaitForSeconds(0.5f);

        gameOverText.transform.DOLocalMoveY(300, 0.5f);
        obtainText.text = $"획득한 몽환의 구슬 : {NowGetBasicGood}개";

        yield return animDelay;

        obtainObj.SetActive(true);

        yield return animDelay;

        nowColor = colors[(int)Colors.White];

        nowAlpha = 0;

        while (true)
        {
            while (nowAlpha < 1)
            {
                if (Input.anyKeyDown)
                {
                    StartCoroutine(EndFaidAnim());
                    yield break;
                }

                nowAlpha += Time.deltaTime * 0.5f;
                nowColor.a = nowAlpha;
                guideText.color = nowColor;

                yield return null;
            }

            while (nowAlpha > 0)
            {
                if (Input.anyKeyDown)
                {
                    StartCoroutine(EndFaidAnim());
                    yield break;
                }

                nowAlpha -= Time.deltaTime * 0.8f;
                nowColor.a = nowAlpha;
                guideText.color = nowColor;
                yield return null;
            }
        }
    }

    IEnumerator EndFaidAnim()
    {
        nowColor = colors[(int)Colors.Black];
        nowAlpha = 0;

        yield return null;

        fadeObj.SetActive(true);
        fadeImage.color = nowColor;

        while (true)
        {
            nowAlpha += Time.deltaTime;
            nowColor.a = nowAlpha;
            fadeImage.color = nowColor;

            if (nowAlpha > 1)
            {
                break;   
            }
            yield return null;
        }

        StartCoroutine(EndGetGoodAnim());
    }

    IEnumerator EndGetGoodAnim()
    {
        WaitForSeconds animDelay = new WaitForSeconds(1f);

        if (NowGetBasicGood > 0)
        {
            nowColor = colors[(int)Colors.Yellow];
            nowAlpha = 1;

            getGoodObj.SetActive(true);

            getGoodObj.transform.DOLocalMoveY(-15, 0.7f);

            yield return animDelay;

            getGoodAnim.SetFloat("NowAnimSpeed", 1);

            goodAmountText.text = $"+{NowGetBasicGood}";

            yield return new WaitForSeconds(0.5f);

            goodAmountText.color = nowColor;

            while (goodAmountText.fontSize > 40)
            {
                goodAmountText.fontSize -= Time.deltaTime * 400;
                yield return null;
            }

            goodAmountText.fontSize = 40;

            yield return animDelay;

            getGoodObj.transform.DOLocalMoveY(-150, 0.5f).SetEase(Ease.InBack);

            while (nowAlpha > 0)
            {
                nowAlpha -= Time.deltaTime * 5;
                goodAmountText.color = nowColor;
                nowColor.a = nowAlpha;
                yield return null;
            }
            gmInstance.Gold += NowGetBasicGood;
        }

        yield return animDelay;

        SceneManager.LoadScene((int)SceneKind.Main);
    }
}
