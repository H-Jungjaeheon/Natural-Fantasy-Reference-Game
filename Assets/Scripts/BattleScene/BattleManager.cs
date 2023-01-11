using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public abstract class BattleManager : MonoBehaviour
{
    #region 게임 정지, 종료시 필요한 오브젝트 모음
    [SerializeField]
    [Tooltip("게임 종료 및 클리어시 비활성화 가능한 오브젝트들")]
    protected GameObject deActivableObj;

    [SerializeField]
    [Tooltip("게임 일시정지 판넬 오브젝트들")]
    protected GameObject[] gamePauseObj;

    [SerializeField]
    [Tooltip("게임종료 시 띄울 판넬 오브젝트")]
    protected GameObject gameEndObj;

    [Tooltip("스탯(플레이어, 보스) UI 오브젝트")]
    public GameObject statUIObj;
    #endregion

    #region 화면 페이드(Fade) 연출 관련
    [Header("화면 페이드(Fade) 연출 관련")]

    [SerializeField]
    [Tooltip("페이드에 쓰일 오브젝트")]
    protected GameObject fadeObj;

    [SerializeField]
    [Tooltip("페이드에 쓰일 이미지")]
    protected Image fadeImage;

    [SerializeField]
    [Tooltip("색 모음")]
    protected Color[] colors;

    [Tooltip("현재 연출할 이미지에 적용할 색")]
    protected Color nowColor;

    [Tooltip("현재 연출할 이미지에 적용할 알파값")]
    protected float nowAlpha;
    #endregion

    #region 스테이지 시작 연출 관련
    [Header("스테이지 시작 연출 관련")]

    [SerializeField]
    [Tooltip("보스 소개 연출 오브젝트")]
    protected GameObject introduceObj;

    [SerializeField]
    [Tooltip("보스 소개 연출 이미지들")]
    protected Image[] introduceImgs;

    [SerializeField]
    [Tooltip("보스 설명 텍스트")]
    protected TextMeshProUGUI explanationText;

    [SerializeField]
    [Tooltip("보스 이름 텍스트")]
    protected TextMeshProUGUI nameText;

    protected bool isIntroducing;

    protected IEnumerator introCoroutine;
    #endregion

    #region 보스 스탯 UI 관련
    [Header("보스 스탯 UI 관련")]

    [SerializeField]
    [Tooltip("보스 스탯 UI : 초상화 이미지")]
    protected Image pictureImg;

    [SerializeField]
    [Tooltip("보스 스탯 UI : 체력바 이미지")]
    protected Image hpBarImg;

    [SerializeField]
    [Tooltip("보스 스탯 UI : 스테미너바 이미지")]
    protected Image enegyBarImg;

    [SerializeField]
    [Tooltip("보스 스탯 UI : 몽환 게이지바 오브젝트")]
    protected GameObject figureBarObj;

    [SerializeField]
    [Tooltip("보스 스탯 UI : 몽환 게이지바 이미지")]
    protected Image figureBarImg;

    [SerializeField]
    [Tooltip("보스 스탯 UI : 보스 속성 이미지")]
    protected Image propertyImg;
    #endregion

    #region 컴포넌트 모음
    [Header("컴포넌트 모음")]

    [SerializeField]
    [Tooltip("CamShake 컴포넌트")]
    protected CamShake csComponent;

    protected Camera mainCam;

    protected GameManager gmInstance;

    protected BattleButtonManager bbmInstance;
    #endregion

    #region 현재 게임의 유닛 정보 모음 (공통)
    [Header("현재 게임의 유닛 정보 모음")]

    [HideInInspector]
    public GameObject enemy;

    [HideInInspector]
    public Vector2 playerCharacterPos; //플레이어 시작 포지션

    [HideInInspector]
    public Vector2 enemyCharacterPos; //적 시작 포지션
    #endregion

    #region 상태(enum) 모음
    [HideInInspector]
    public NowGameState nowGameState;

    protected OptionPage nowBattleSceneOptionState;
    #endregion

    protected void Start()
    {
        StartSetting();
    }

    protected virtual void StartSetting()
    {
        mainCam = Camera.main;
        gmInstance = GameManager.Instance;
        bbmInstance = BattleButtonManager.Instance;

        gmInstance.nowScene = SceneKind.Ingame;

        nowBattleSceneOptionState = OptionPage.None;

        StartCoroutine(StartFaidAnim());
        StartCoroutine(GamePauseObjOnOrOff());
    }

    /// <summary>
    /// 일시정지 화면 띄우기(화면 끄기 및 세팅 등의 화면 넘기기)
    /// </summary>
    /// <returns></returns>
    protected IEnumerator GamePauseObjOnOrOff()
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
    protected IEnumerator PressEscToGamePausePageChange()
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
    protected IEnumerator StartFaidAnim()
    {
        WaitForSeconds animDelay = new WaitForSeconds(1f);

        nowGameState = NowGameState.GameReady;

        nowColor = colors[(int)Colors.Black];
        nowAlpha = 1;

        bbmInstance.ActionButtonSetActive(false);
        statUIObj.SetActive(false);
        fadeObj.SetActive(true);

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

    protected IEnumerator IntroSkip()
    {
        while (isIntroducing)
        {
            if (Input.anyKeyDown)
            {
                StopCoroutine(introCoroutine);
                DOTween.PauseAll();

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
    protected IEnumerator IntroAnim()
    {
        Vector3 camMoveSpeed = new Vector3(7f, 0f, 0f);
        Vector3 camTargetPos = new Vector3(4f, 0.5f, -10f);

        isIntroducing = true;

        StartCoroutine(IntroSkip());

        while (mainCam.transform.position.x < 4f)
        {
            mainCam.transform.position += camMoveSpeed * Time.deltaTime;
            mainCam.orthographicSize -= Time.deltaTime * 3.5f;
            yield return null;
        }

        mainCam.transform.position = camTargetPos;
        mainCam.orthographicSize = 7.5f;

        nowColor = colors[(int)Colors.White];

        nowColor.a = 0f;
        nowAlpha = 0f;

        while (nowAlpha < 1f)
        {
            nowAlpha += Time.deltaTime * 1.5f;
            nowColor.a = nowAlpha;
            explanationText.color = nowColor;
            nameText.color = nowColor;
            yield return null;
        }

        yield return new WaitForSeconds(3f);

        StartCoroutine(EndIntroAnim());
    }

    protected virtual IEnumerator EndIntroAnim()
    {
        Vector3 camMoveSpeed = new Vector3(7f, 0f, 0f);
        Vector3 camTargetPos = new Vector3(0f, 0.5f, -10f);

        explanationText.DOFade(0f, 1.5f);
        nameText.DOFade(0f, 1.5f);

        while (mainCam.transform.position.x > 0) //카메라 확대에서 원래 시야만큼 돌리기
        {
            mainCam.transform.position -= camMoveSpeed * (Time.deltaTime * 2);
            mainCam.orthographicSize += Time.deltaTime * 7f;
            yield return null;
        }

        for (int nowIndex = 0; nowIndex < introduceImgs.Length; nowIndex++)
        {
            introduceImgs[nowIndex].DOFade(0f, 1.5f);
        }

        mainCam.transform.position = camTargetPos;
        mainCam.orthographicSize = 9.5f;

        yield return new WaitForSeconds(1f);

        fadeObj.SetActive(false);
        statUIObj.SetActive(true);
        bbmInstance.ActionButtonSetActive(true);
        introduceObj.SetActive(false);

        isIntroducing = false;
        nowGameState = NowGameState.Playing;
    }

    public void GameExit() => StartCoroutine(SceneChangeFaidOut(SceneKind.Main));

    /// <summary>
    /// 페이드 아웃 효과 후 씬 변경 코루틴
    /// </summary>
    /// <param name="changeScene"> 변경될 씬 종류 </param>
    /// <returns></returns>
    protected IEnumerator SceneChangeFaidOut(SceneKind changeScene)
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
    public abstract void StartGameEndPanelAnim(bool isGameOver, Vector3 bossPos);

    /// <summary>
    /// 플레이어 사망(게임 오버)시 페이드 애니메이션 함수
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerator PlayerDeadAnim();

    /// <summary>
    /// 보스 사망 시 실행하는 코루틴(화면 연출, 텍스트 세팅, 결과 화면 띄우기)
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerator BossDeadAnim();
}
