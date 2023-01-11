using System.Collections;
using UnityEngine;
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

public class BattleSceneManager : BattleManager //나중에 게임 오버 및 게임 클리어, 재화 관리 //Singleton<BattleSceneManager>
{
    public static BattleSceneManager instance;

    #region 스테이지 데이터 관련 모음
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
    public StageData[] stageDatas;

    StageData nowStageData; //현재 스테이지 데이터
    #endregion

    private int nowGetBasicGood; //현재 스테이지에서 얻은 재화 개수

    public int NowGetBasicGood
    {
        get { return nowGetBasicGood; }
        set { nowGetBasicGood = value; }
    }

    #region 현재 게임의 유닛 정보 모음 (고유)
    [Header("현재 게임의 고유 유닛 정보 모음")]
    public Player player;
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

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    protected override void StartSetting()
    {
        base.StartSetting();
        playerCharacterPos = player.transform.position;
        nowStageSetting();
    }

    /// <summary>
    /// 스테이지 요소들 세팅(현재 스테이지 정보 기반)
    /// </summary>
    void nowStageSetting()
    {
        nowStageData = stageDatas[(int)gmInstance.nowStage];
        
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

    public override void StartGameEndPanelAnim(bool isGameOver, Vector3 bossPos)
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
    protected override IEnumerator PlayerDeadAnim()
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
    protected override IEnumerator BossDeadAnim()
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

        deActivableObj.SetActive(false);

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

    protected override IEnumerator EndIntroAnim()
    {
        yield return StartCoroutine(base.EndIntroAnim());
        nowStageData.gimmickObjs.SetActive(true);
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
