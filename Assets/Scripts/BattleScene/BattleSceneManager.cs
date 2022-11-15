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

public enum GameEndKind
{
    GameOver,
    GameClear
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
    [HideInInspector]
    public Vector2 playerCharacterPos; //플레이어 포지션

    [HideInInspector]
    public Vector2 enemyCharacterPos; //적 포지션

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
    [Tooltip("게임 종료 판넬 오브젝트")]
    private GameObject[] gameEndObj;

    #region 화면 페이드(Fade) 연출 관련
    [Header("화면 페이드(Fade) 연출 관련")]

    [SerializeField]
    [Tooltip("페이드에 쓰일 오브젝트")]
    private GameObject faidObj;

    [SerializeField]
    [Tooltip("페이드에 쓰일 이미지")]
    private Image faidImage;

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

    private GameEndKind gameEndKind;
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

    [SerializeField]
    [Tooltip("플레이어, 보스 스탯 UI 오브젝트")]
    private GameObject[] unitStatUIObj;

    [SerializeField]
    [Tooltip("카메라 흔들림 컴포넌트")]
    private CamShake csComponent;

    [HideInInspector]
    public Player Player;

    [HideInInspector]
    public GameObject Enemy;

    [HideInInspector]
    public NowGameState nowGameState;

    private BattleOrMainOptionState nowBattleSceneOptionState;

    private Camera mainCam;

    private WaitForSeconds oneSecondDelay = new WaitForSeconds(1);

    private WaitForSeconds zeroPointFiveDelay = new WaitForSeconds(0.5f);

    private GameManager gmInstance;

    private BattleButtonManager bbmInstance;

    private Vector3 startAnimCamMoveSpeed = new Vector3(7, 0, 0);

    private Vector3 camTargetPos;

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
        nowBattleSceneOptionState = BattleOrMainOptionState.None;

        gmInstance.nowSceneState = NowSceneState.Ingame;

        nowBattleSceneOptionState = BattleOrMainOptionState.None;

        StartCoroutine(StartFaidAnim());
        StartCoroutine(GamePauseObjOnOrOff());
    }

    IEnumerator GamePauseObjOnOrOff()
    {
        while (true)
        {
            if (nowBattleSceneOptionState == BattleOrMainOptionState.SecondPage || nowBattleSceneOptionState == BattleOrMainOptionState.ThirdPage)
            {
                StartCoroutine(PressEscToGamePausePageChange());
                break;
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && nowGameState != NowGameState.GameReady)
            {
                if (nowBattleSceneOptionState == BattleOrMainOptionState.None)
                {
                    gamePauseObj[(int)BattleOrMainOptionState.FirstPage].SetActive(true);
                }
                else
                {
                    gamePauseObj[(int)BattleOrMainOptionState.FirstPage].SetActive(false);
                }
                yield return null;
                Time.timeScale = (nowBattleSceneOptionState == BattleOrMainOptionState.None) ? 0 : 1;
                nowBattleSceneOptionState = (nowBattleSceneOptionState == BattleOrMainOptionState.None) ? BattleOrMainOptionState.FirstPage : BattleOrMainOptionState.None;
                nowGameState = (nowGameState == NowGameState.Playing) ? NowGameState.Pausing : NowGameState.Playing;
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
                if (nowBattleSceneOptionState == BattleOrMainOptionState.SecondPage)
                {
                    gamePauseObj[(int)BattleOrMainOptionState.SecondPage].SetActive(false);
                    gamePauseObj[(int)BattleOrMainOptionState.FirstPage].SetActive(true);
                }
                else if (nowBattleSceneOptionState == BattleOrMainOptionState.ThirdPage)
                {
                    gamePauseObj[(int)BattleOrMainOptionState.ThirdPage].SetActive(false);
                    gamePauseObj[(int)BattleOrMainOptionState.FirstPage].SetActive(true);
                }
                nowBattleSceneOptionState = BattleOrMainOptionState.FirstPage;
                yield return null;
                StartCoroutine(GamePauseObjOnOrOff());
                break;
            }
            yield return null;
        }
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
        nowBattleSceneOptionState = (BattleOrMainOptionState)nowChange;
    }

    IEnumerator StartFaidAnim() //처음 게임 페이드 인 연출
    {
        nowColor = colors[(int)Colors.Black];
        nowAlpha = 1;

        bbmInstance.ActionButtonsSetActive(false, false, false);

        for (int nowIndex = 0; nowIndex < unitStatUIObj.Length; nowIndex++)
        {
            unitStatUIObj[nowIndex].SetActive(false);
        }

        nowGameState = NowGameState.GameReady;

        faidObj.SetActive(true);
        nowColor.a = 1;
        faidImage.color = nowColor;

        yield return oneSecondDelay;

        while (nowAlpha > 0)
        {
            nowAlpha -= Time.deltaTime;
            nowColor.a = nowAlpha;
            faidImage.color = nowColor;
            yield return null;
        }

        yield return oneSecondDelay;

        StartCoroutine(IntroducingTheStageAnim());
    }

    IEnumerator IntroducingTheStageAnim()
    {
        nowColor = colors[(int)Colors.Black];
        nowAlpha = 0;

        while (mainCam.transform.position.x < 4f)
        {
            mainCam.transform.position += startAnimCamMoveSpeed * Time.deltaTime;
            mainCam.orthographicSize -= Time.deltaTime * 3.5f;
            yield return null;
        }

        camTargetPos.x = 4f;
        camTargetPos.y = 0.5f;
        camTargetPos.z = -10;

        mainCam.transform.position = camTargetPos;
        mainCam.orthographicSize = 7.5f;

        while (nowAlpha < 0.75f)
        {
            nowAlpha += Time.deltaTime * 3;
            nowColor.a = nowAlpha;
            faidImage.color = nowColor;
            yield return null;
        }

        introducingTheStageImage.rectTransform.DOAnchorPosX(0, 0.5f);
        introducingTheStageText.rectTransform.DOAnchorPosX(0, 0.5f);

        while (introducingTheStageImage.rectTransform.anchoredPosition.x < 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(3);

        introducingTheStageImage.rectTransform.DOAnchorPosX(1920, 0.25f);
        introducingTheStageText.rectTransform.DOAnchorPosX(-1920, 0.25f);

        while (nowAlpha > 0)
        {
            nowAlpha -= Time.deltaTime * 3;
            nowColor.a = nowAlpha;
            faidImage.color = nowColor;
            yield return null;
        }


        while (introducingTheStageImage.rectTransform.anchoredPosition.x < 1920)
        {
            yield return null;
        }

        while (mainCam.transform.position.x > 0)
        {
            mainCam.transform.position -= startAnimCamMoveSpeed * (Time.deltaTime * 3);
            mainCam.orthographicSize += Time.deltaTime * 10.5f;
            yield return null;
        }

        camTargetPos.x = 0;
        camTargetPos.y = 0.5f;
        camTargetPos.z = -10;

        mainCam.transform.position = camTargetPos;
        mainCam.orthographicSize = 9.5f;

        yield return oneSecondDelay;

        faidObj.SetActive(false);
        nowGameState = NowGameState.Playing;

        for (int nowIndex = 0; nowIndex < unitStatUIObj.Length; nowIndex++)
        {
            unitStatUIObj[nowIndex].SetActive(true);
        }

        bbmInstance.ActionButtonsSetActive(true, false, false);
    }

    public void ChangeScene(string changeSceneName)
    {
        StartCoroutine(SceneChangeFaidOut(changeSceneName));
    }

    IEnumerator SceneChangeFaidOut(string changeSceneName)
    {
        nowColor = colors[(int)Colors.Black];
        WaitForSecondsRealtime faidDelay = new WaitForSecondsRealtime(0.01f);
        float nowAlphaPlusPerSecond = 0.025f;
        nowAlpha = 0;

        faidImage.transform.SetAsLastSibling();
        faidObj.SetActive(true);

        while (nowAlpha < 3)
        {
            nowColor.a = nowAlpha;
            faidImage.color = nowColor;

            nowAlpha += nowAlphaPlusPerSecond;
            yield return faidDelay;
        }

        Time.timeScale = 1;
        SceneManager.LoadScene(changeSceneName);
    }

    public void StartGameEndPanelAnim(bool isGameOver)
    {
        nowGameState = NowGameState.GameEnd;
        gameEndKind = isGameOver ? GameEndKind.GameOver : GameEndKind.GameClear;

        if (isGameOver) //테스트용 판별 나중에 삭제
        {
            StartCoroutine(PlayerDeadAnim());
        }
        else
        {
            csComponent.GameEndSetting();

            bbmInstance.statUIObj.SetActive(false);
            bbmInstance.buttonObj.SetActive(false);

            StartCoroutine(BossDeadAnim());
        }

    }

    IEnumerator PlayerDeadAnim()
    {
        nowColor = colors[(int)Colors.White];
        nowAlpha = 1;

        yield return null;

        deActivableObj.SetActive(false);
        faidObj.SetActive(true);
        faidImage.color = nowColor;

        gameEndObj[(int)GameEndKind.GameOver].SetActive(true);

        yield return zeroPointFiveDelay;

        nowColor = colors[(int)Colors.Red];

        while (nowAlpha > 0)
        {
            faidImage.color = nowColor;
            nowColor.a = nowAlpha;
            nowAlpha -= Time.deltaTime;
            yield return null;
        }

        faidObj.SetActive(false);

        StartCoroutine(SameEndAnim());
    }

    IEnumerator BossDeadAnim()
    {
        nowColor = colors[(int)Colors.White];
        nowAlpha = 0;

        faidObj.SetActive(true);

        yield return null;

        while (nowAlpha < 1)
        {
            if (mainCam.orthographicSize > 6.5f)
            {
                mainCam.orthographicSize -= Time.deltaTime * 0.15f;
            }

            nowColor.a = nowAlpha;
            faidImage.color = nowColor;

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

        gameEndObj[(int)GameEndKind.GameOver].SetActive(true);
        gameOverText.text = "Stage 1 Clear!";

        yield return oneSecondDelay;

        while (nowAlpha > 0)
        {
            faidImage.color = nowColor;
            nowColor.a = nowAlpha;
            nowAlpha -= Time.deltaTime;
            yield return null;
        }

        faidObj.SetActive(false);

        StartCoroutine(SameEndAnim());
    }

    IEnumerator SameEndAnim() //게임 종료 판넬 애니메이션 (공통)
    {
        nowColor = colors[(int)Colors.White];

        nowAlpha = 0;

        yield return zeroPointFiveDelay;

        gameOverText.transform.DOLocalMoveY(300, 0.5f);
        obtainText.text = $"획득한 몽환의 구슬 : {NowGetBasicGood}개";

        yield return oneSecondDelay;

        obtainObj.SetActive(true);

        yield return oneSecondDelay;

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

        faidObj.SetActive(true);
        faidImage.color = nowColor;

        while (true)
        {
            nowAlpha += Time.deltaTime;
            nowColor.a = nowAlpha;
            faidImage.color = nowColor;

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
        if (NowGetBasicGood > 0)
        {
            nowColor = colors[(int)Colors.Yellow];
            nowAlpha = 1;

            getGoodObj.SetActive(true);

            getGoodObj.transform.DOLocalMoveY(-15, 0.7f);

            yield return oneSecondDelay;

            getGoodAnim.SetFloat("NowAnimSpeed", 1);

            goodAmountText.text = $"+{NowGetBasicGood}";

            yield return zeroPointFiveDelay;

            goodAmountText.color = nowColor;

            while (goodAmountText.fontSize > 70)
            {
                goodAmountText.fontSize -= Time.deltaTime * 350;
                yield return null;
            }

            yield return oneSecondDelay;

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

        yield return oneSecondDelay;

        SceneManager.LoadScene("Main");

        yield return null;
    }
}
