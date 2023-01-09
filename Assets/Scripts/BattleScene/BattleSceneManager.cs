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

public enum Stage
{
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

        [Tooltip("보스 등장 연출 : 이름")]
        public string bossName;

        [Tooltip("보스 등장 연출 : 설명")]
        public string explanation;

        [Tooltip("보스 스탯 UI : 초상화")]
        public Sprite pictureSprite;

        [Tooltip("보스 스탯 UI : 체력바 배경")]
        public Sprite hpBarSprite;

        [Tooltip("보스 스탯 UI : 스테미너바 배경")]
        public Sprite enegyBarSprite;

        [Tooltip("보스 몽환 게이지 소유 여부")]
        public bool isHaveFigure;

        [Tooltip("보스 스탯 UI : 몽환 게이지바 배경")]
        public Sprite figureBarSprite;

        [Tooltip("현재 보스 속성 아이콘")]
        public Sprite propertySprite;

        [Tooltip("현재 스테이지 BGM")]
        public AudioClip stageBgm;

        [Tooltip("플레이어 기본공격 인식 사거리")]
        public float intersection;
    }

    [Tooltip("각 스테이지별 변경될 데이터들")]
    public StageData[] stageData;

    StageData nowStageData; //현재 스테이지 데이터

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

    private float nowAlpha; //이미지에 적용할 알파값
    #endregion

    #region 스테이지 시작 연출 관련
    [Header("스테이지 시작 연출 관련")]

    [SerializeField]
    [Tooltip("보스 소개 연출 이미지")]
    private Image introducingTheStageImage;

    [SerializeField]
    [Tooltip("보스 설명 텍스트")]
    private TextMeshProUGUI explanationText;

    [SerializeField]
    [Tooltip("보스 이름 텍스트")]
    private TextMeshProUGUI nameText;

    private bool isIntroducing;

    private IEnumerator introCoroutine;
    #endregion

    #region 보스 스탯 UI 관련
    [Header("보스 스탯 UI 관련")]

    [SerializeField]
    [Tooltip("보스 스탯 UI : 초상화 이미지")]
    private Image pictureImg;

    [SerializeField]
    [Tooltip("보스 스탯 UI : 체력바 이미지")]
    private Image hpBarImg;

    [SerializeField]
    [Tooltip("보스 스탯 UI : 스테미너바 이미지")]
    private Image enegyBarImg;

    [SerializeField]
    [Tooltip("보스 스탯 UI : 몽환 게이지바 오브젝트")]
    private GameObject figureBarObj;

    [SerializeField]
    [Tooltip("보스 스탯 UI : 몽환 게이지바 이미지")]
    private Image figureBarImg;

    [SerializeField]
    [Tooltip("보스 스탯 UI : 보스 속성 이미지")]
    private Image propertyImg;
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

    void Start()
    {
        StartSetting();
    }

    void StartSetting()
    {
        mainCam = Camera.main;
        gmInstance = GameManager.Instance;
        bbmInstance = BattleButtonManager.Instance;

        nowStageSetting();

        gmInstance.nowScene = SceneKind.Ingame;

        nowBattleSceneOptionState = OptionPage.None;

        playerCharacterPos = player.transform.position;

        StartCoroutine(StartFaidAnim());
        StartCoroutine(GamePauseObjOnOrOff());
    }

    /// <summary>
    /// 스테이지 요소들 세팅(현재 스테이지 정보 기반)
    /// </summary>
    void nowStageSetting()
    {
        nowStageData = stageData[(int)gmInstance.nowStage];

        explanationText.text = nowStageData.explanation;
        nameText.text = nowStageData.bossName;

        nowStageData.bossObjs.SetActive(true); 
        nowStageData.bgResources.SetActive(true);

        pictureImg.sprite = nowStageData.pictureSprite;
        hpBarImg.sprite = nowStageData.hpBarSprite;
        enegyBarImg.sprite = nowStageData.enegyBarSprite;
        propertyImg.sprite = nowStageData.propertySprite;

        if (nowStageData.isHaveFigure)
        {
            figureBarImg.sprite = nowStageData.figureBarSprite;
            figureBarObj.SetActive(true);
        }
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

                fadeObj.SetActive(isNonePage);

                nowAlpha = 0.698f;
                nowColor.a = nowAlpha;

                fadeImage.color = nowColor;

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

    /// <summary>
    /// 일시정지 버튼들 페이지 세팅 코루틴
    /// </summary>
    /// <param name="nowChange"> 현재 변경할 페이지 인덱스 </param>
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
                explanationText.rectTransform.DOAnchorPosX(-1920f, duration);

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

        introducingTheStageImage.rectTransform.DOAnchorPosX(10f, 0.3f);
        explanationText.rectTransform.DOAnchorPosX(10f, 0.3f);

        while (introducingTheStageImage.rectTransform.anchoredPosition.x < 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(3f);

        introducingTheStageImage.rectTransform.DOAnchorPosX(1920f, 0.25f);
        explanationText.rectTransform.DOAnchorPosX(-1920f, 0.25f);

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

        nowStageData.gimmickObjs.SetActive(true);
    }

    public void GameExit() => StartCoroutine(SceneChangeFaidOut(SceneKind.Main));
    
    /// <summary>
    /// 페이드 아웃 효과 후 씬 변경 코루틴
    /// </summary>
    /// <param name="changeScene"> 변경될 씬 종류 </param>
    /// <returns></returns>
    IEnumerator SceneChangeFaidOut(SceneKind changeScene)
    {
        fadeObj.transform.SetAsLastSibling();

        while (nowAlpha <= 1)
        {
            nowColor.a = nowAlpha;
            fadeImage.color = nowColor;

            nowAlpha += Time.unscaledDeltaTime;
            yield return null;
        }

        Time.timeScale = 1;
        SceneManager.LoadScene((int)changeScene);
    }

    /// <summary>
    /// 게임 마무리 애니메이션 함수
    /// </summary>
    /// <param name="isGameOver"> 게임오버 판별 </param>
    public void StartGameEndPanelAnim(bool isGameOver, Vector3 bossPos)
    {
        nowGameState = NowGameState.GameEnd;

        if (isGameOver)
        {
            StartCoroutine(PlayerDeadAnim());
        }
        else
        {
            csComponent.GameEndSetting(bossPos);

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
    
    /// <summary>
    /// 보스 사망 시 실행하는 코루틴(화면 연출, 텍스트 세팅, 결과 화면 띄우기)
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 게임 종료 판넬 애니메이션(게임 오버, 게임 클리어 공통)
    /// </summary>
    /// <returns></returns>
    IEnumerator SameEndAnim()
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

    /// <summary>
    /// 종료 시 페이드 애니메이션(효과)
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 재화 획득 애니메이션 실행 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator EndGetGoodAnim()
    {
        WaitForSeconds animDelay = new WaitForSeconds(1f);

        if (NowGetBasicGood > 0)
        {
            getGoodObj.SetActive(true);

            getGoodObj.transform.DOLocalMoveY(-15, 0.7f);

            yield return animDelay;

            getGoodAnim.SetFloat("NowAnimSpeed", 1);

            goodAmountText.text = $"+{NowGetBasicGood}";

            yield return new WaitForSeconds(0.5f);

            nowColor = colors[(int)Colors.Yellow];

            goodAmountText.color = nowColor;

            while (goodAmountText.fontSize > 40)
            {
                goodAmountText.fontSize -= Time.deltaTime * 400;
                yield return null;
            }

            goodAmountText.fontSize = 40;

            yield return animDelay;

            getGoodObj.transform.DOLocalMoveY(-150, 0.5f).SetEase(Ease.InBack);
            
            nowAlpha = 1;

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
