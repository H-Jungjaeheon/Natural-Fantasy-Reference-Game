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

public class BattleSceneManager : Singleton<BattleSceneManager> //나중에 게임 오버 및 게임 클리어, 재화 관리
{
    [HideInInspector]
    public Vector2 playerCharacterPos; //플레이어 포지션

    [HideInInspector]
    public Vector2 enemyCharacterPos; //적 포지션

    [SerializeField]
    [Tooltip("게임 일시정지 판넬 오브젝트들")]
    private GameObject[] gamePauseObj;

    [SerializeField]
    [Tooltip("씬 전환 시 필요한 판넬 오브젝트 이미지 컴포넌트")]
    private Image faidPanelObjImageComponent;

    [SerializeField]
    [Tooltip("페이드 아웃 오브젝트")]
    private GameObject faidOutObj;

    [SerializeField]
    [Tooltip("페이드 아웃 이미지")]
    private Image faidOutImage;

    [SerializeField]
    [Tooltip("페이드 아웃 이미지 컬러")]
    private Color faidOutImageColor;

    [SerializeField]
    [Tooltip("스테이지 및 보스 소개 연출 이미지")]
    private Image introducingTheStageImage;

    [SerializeField]
    [Tooltip("스테이지 및 보스 소개 텍스트")]
    private TextMeshProUGUI introducingTheStageText;

    [SerializeField]
    [Tooltip("플레이어, 보스 스탯 UI 오브젝트")]
    private GameObject[] unitStatUIObj;

    [HideInInspector]
    public Player Player;

    [HideInInspector]
    public GameObject Enemy;

    [HideInInspector]
    public NowGameState nowGameState;

    private Camera mainCam;
    
    private WaitForSeconds faidDelay = new WaitForSeconds(1);

    private Vector3 startAnimCamMoveSpeed = new Vector3(7, 0, 0);

    private Vector3 camTargetPos;

    private void Start()
    {
        mainCam = Camera.main;
        StartCoroutine(StartFaidAnim());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && nowGameState == NowGameState.Playing)
        {
            bool nowGamePauseObjSetActive = (nowGameState == NowGameState.Playing);
            gamePauseObj[0].SetActive(nowGamePauseObjSetActive);//////////////////////////////////////////////////// 수정
            nowGameState = (nowGamePauseObjSetActive) ? NowGameState.Pausing : NowGameState.Playing;
            Time.timeScale = nowGamePauseObjSetActive ? 0 : 1;
        }
    }

    IEnumerator StartFaidAnim() //처음 게임 페이드 인 연출
    {
        float nowImageAlpha = 1;

        BattleButtonManager.Instance.ActionButtonsSetActive(false, false, false);

        for (int nowIndex = 0; nowIndex < unitStatUIObj.Length; nowIndex++)
        {
            unitStatUIObj[nowIndex].SetActive(false);
        }

        nowGameState = NowGameState.GameReady;

        faidOutObj.SetActive(true);
        faidOutImageColor.a = 1;
        faidOutImage.color = faidOutImageColor;

        yield return faidDelay;

        while (nowImageAlpha > 0)
        {
            nowImageAlpha -= Time.deltaTime;
            faidOutImageColor.a = nowImageAlpha;
            faidOutImage.color = faidOutImageColor;
            yield return null;
        }

        yield return faidDelay;

        StartCoroutine(IntroducingTheStageAnim());
    }

    IEnumerator IntroducingTheStageAnim()
    {
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
            faidOutImage.color = faidOutImageColor;
            yield return null;
        }

        introducingTheStageImage.rectTransform.DOAnchorPosX(0, 0.5f);
        introducingTheStageText.rectTransform.DOAnchorPosX(0, 0.5f);

        while (introducingTheStageImage.rectTransform.anchoredPosition.x < 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(3);

        while (nowImageAlpha > 0)
        {
            nowImageAlpha -= Time.deltaTime * 3;
            faidOutImageColor.a = nowImageAlpha;
            faidOutImage.color = faidOutImageColor;
            yield return null;
        }

        introducingTheStageImage.rectTransform.DOAnchorPosX(1920, 0.5f);
        introducingTheStageText.rectTransform.DOAnchorPosX(-1920, 0.5f);

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

        yield return faidDelay;

        faidOutObj.SetActive(false);
        nowGameState = NowGameState.Playing;

        for (int nowIndex = 0; nowIndex < unitStatUIObj.Length; nowIndex++)
        {
            unitStatUIObj[nowIndex].SetActive(true);
        }

        BattleButtonManager.Instance.ActionButtonsSetActive(true, false, false);
    }

    public void ChangeScene(string changeSceneName)
    {
        StartCoroutine(SceneChangeFaidOut(changeSceneName));
    }

    IEnumerator SceneChangeFaidOut(string changeSceneName)
    {
        Color color = new Color(0,0,0,0);
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
}
