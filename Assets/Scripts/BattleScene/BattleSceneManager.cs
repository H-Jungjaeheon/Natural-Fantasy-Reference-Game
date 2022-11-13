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

    private int nowGetBasicGood;

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

    [SerializeField]
    [Tooltip("씬 전환 시 필요한 판넬 오브젝트 이미지 컴포넌트")]
    private Image faidPanelObjImageComponent;

    [SerializeField]
    [Tooltip("페이드에 쓰일 오브젝트")]
    private GameObject faidObj;

    [SerializeField]
    [Tooltip("페이드에 쓰일 이미지")]
    private Image faidImage;

    [SerializeField]
    [Tooltip("색 모음")]
    private Color[] colors;

    [SerializeField]
    [Tooltip("스테이지 및 보스 소개 연출 이미지")]
    private Image introducingTheStageImage;

    [SerializeField]
    [Tooltip("스테이지 및 보스 소개 텍스트")]
    private TextMeshProUGUI introducingTheStageText;

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
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (nowBattleSceneOptionState == BattleOrMainOptionState.None && nowGameState == NowGameState.Playing)
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
        Color faidOutImageColor = colors[(int)Colors.Black];
        float nowImageAlpha = 1;

        bbmInstance.ActionButtonsSetActive(false, false, false);

        for (int nowIndex = 0; nowIndex < unitStatUIObj.Length; nowIndex++)
        {
            unitStatUIObj[nowIndex].SetActive(false);
        }

        nowGameState = NowGameState.GameReady;

        faidObj.SetActive(true);
        faidOutImageColor.a = 1;
        faidImage.color = faidOutImageColor;

        yield return oneSecondDelay;

        while (nowImageAlpha > 0)
        {
            nowImageAlpha -= Time.deltaTime;
            faidOutImageColor.a = nowImageAlpha;
            faidImage.color = faidOutImageColor;
            yield return null;
        }

        yield return oneSecondDelay;

        StartCoroutine(IntroducingTheStageAnim());
    }

    IEnumerator IntroducingTheStageAnim()
    {
        Color faidOutImageColor = colors[(int)Colors.Black];
        float nowImageAlpha = 0;

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

        while (nowImageAlpha < 0.75f)
        {
            nowImageAlpha += Time.deltaTime * 3;
            faidOutImageColor.a = nowImageAlpha;
            faidImage.color = faidOutImageColor;
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

        while (nowImageAlpha > 0)
        {
            nowImageAlpha -= Time.deltaTime * 3;
            faidOutImageColor.a = nowImageAlpha;
            faidImage.color = faidOutImageColor;
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
        Color color = colors[(int)Colors.Black];
        WaitForSecondsRealtime faidDelay = new WaitForSecondsRealtime(0.01f);
        float nowAlphaPlusPerSecond = 0.025f;
        float nowFaidPanelObjAlpha = 0;

        faidPanelObjImageComponent.transform.SetAsLastSibling();
        while (nowFaidPanelObjAlpha < 3)
        {
            color.a = nowFaidPanelObjAlpha;
            faidPanelObjImageComponent.color = color;
            
            nowFaidPanelObjAlpha += nowAlphaPlusPerSecond;
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
            StartCoroutine(GameEndPanelAnim());
        }
        else
        {
            csComponent.GameEndSetting();

            bbmInstance.statUIObj.SetActive(false);
            bbmInstance.buttonObj.SetActive(false);

            StartCoroutine(BossDeadAnim());
        }

    }

    IEnumerator BossDeadAnim()
    {
        Color faidOutImageColor = colors[(int)Colors.White];
        float nowImageAlpha = 0;

        faidObj.SetActive(true);

        yield return null;

        while (nowImageAlpha < 1)
        {
            if (mainCam.orthographicSize > 6.5f)
            {
                mainCam.orthographicSize -= Time.deltaTime * 0.15f;
            }

            faidOutImageColor.a = nowImageAlpha;
            faidImage.color = faidOutImageColor;

            if (nowImageAlpha < 0.4f)
            {
                nowImageAlpha += Time.deltaTime * 0.07f;
            }
            else
            {
                nowImageAlpha += Time.deltaTime * 0.5f;
            }

            yield return null;
        }

        yield return oneSecondDelay;
    }

    IEnumerator GameEndPanelAnim() //게임 종료 판넬 애니메이션 (게임 오버인지 판별)
    {
        Color faidOutImageColor = colors[(int)Colors.White];
        WaitForSeconds animDelay = new WaitForSeconds(0.5f);
        float nowImageAlpha = 1;

        yield return null;

        deActivableObj.SetActive(false);
        faidObj.SetActive(true);
        faidImage.color = faidOutImageColor;

        gameEndObj[(int)GameEndKind.GameOver].SetActive(true);

        yield return animDelay;

        faidOutImageColor = colors[(int)Colors.Red];

        while (nowImageAlpha > 0)
        {
            faidImage.color = faidOutImageColor;
            faidOutImageColor.a = nowImageAlpha;
            nowImageAlpha -= Time.deltaTime;
            yield return null;
        }

        faidObj.SetActive(false);

        yield return animDelay;

        gameOverText.transform.DOLocalMoveY(300, 0.5f);
        obtainText.text = $"획득한 몽환의 구슬 : {NowGetBasicGood}개";

        yield return oneSecondDelay;

        obtainObj.SetActive(true);

        yield return oneSecondDelay;

        faidOutImageColor = colors[(int)Colors.White];

        nowImageAlpha = 0;

        while (true)
        {
            while (nowImageAlpha < 1)
            {
                if (Input.anyKeyDown)
                {
                    StartCoroutine(EndFaidAnim());
                    yield break;
                }
                nowImageAlpha += Time.deltaTime * 0.5f;
                faidOutImageColor.a = nowImageAlpha;
                guideText.color = faidOutImageColor;
                yield return null;
            }
            while (nowImageAlpha > 0)
            {
                if (Input.anyKeyDown)
                {
                    StartCoroutine(EndFaidAnim());
                    yield break;
                }
                nowImageAlpha -= Time.deltaTime * 0.8f;
                faidOutImageColor.a = nowImageAlpha;
                guideText.color = faidOutImageColor;
                yield return null;
            }
        }
    }

    IEnumerator EndFaidAnim()
    {
        Color faidOutImageColor = colors[(int)Colors.Black];
        float nowImageAlpha = 0;

        yield return null;

        faidObj.SetActive(true);
        faidImage.color = faidOutImageColor;

        while (true)
        {
            nowImageAlpha += Time.deltaTime;
            faidOutImageColor.a = nowImageAlpha;
            faidImage.color = faidOutImageColor;

            if (nowImageAlpha > 1)
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
            Color goodAmountTextColor = colors[(int)Colors.Yellow];
            float nowTextAlpha = 1;

            getGoodObj.SetActive(true);

            getGoodObj.transform.DOLocalMoveY(-15, 0.7f);

            yield return oneSecondDelay;

            getGoodAnim.SetFloat("NowAnimSpeed", 1);

            goodAmountText.text = $"+{NowGetBasicGood}";

            yield return new WaitForSeconds(0.5f);

            goodAmountText.color = goodAmountTextColor;

            while (goodAmountText.fontSize > 70)
            {
                goodAmountText.fontSize -= Time.deltaTime * 350;
                yield return null;
            }

            yield return oneSecondDelay;

            getGoodObj.transform.DOLocalMoveY(-150, 0.5f).SetEase(Ease.InBack);

            while (nowTextAlpha > 0)
            {
                nowTextAlpha -= Time.deltaTime * 5;
                goodAmountText.color = goodAmountTextColor;
                goodAmountTextColor.a = nowTextAlpha;
                yield return null;
            }
            gmInstance.Gold += NowGetBasicGood;
        }

        yield return oneSecondDelay;

        SceneManager.LoadScene("Main");

        yield return null;
    }
}
