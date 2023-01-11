using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class TutorialBattleManager : BattleManager
{
    public static TutorialBattleManager instance;

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

    #region 현재 게임의 유닛 정보 모음 (고유)
    [Header("현재 게임의 고유 유닛 정보 모음")]
    public TutorialPlayer tutorialPlayer;
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
        playerCharacterPos = tutorialPlayer.transform.position;
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

            tutorialPlayer.GameClearSetting();

            StartCoroutine(BossDeadAnim());
        }
    }

    /// <summary>
    /// 플레이어 사망(게임 오버)시 페이드 애니메이션 함수
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator PlayerDeadAnim()
    {
        print("튜토리얼에 맞는 이벤트 실행");
        yield return null;
        //nowColor = colors[(int)Colors.White];
        //nowAlpha = 1;

        //CamShake.JumpStop(true);

        //yield return null;

        //deActivableObj.SetActive(false);
        //fadeObj.SetActive(true);
        //fadeImage.color = nowColor;

        //gameEndObj.SetActive(true);

        //yield return new WaitForSeconds(0.5f);

        //nowColor = colors[(int)Colors.Red];

        //while (nowAlpha > 0)
        //{
        //    fadeImage.color = nowColor;
        //    nowColor.a = nowAlpha;
        //    nowAlpha -= Time.deltaTime;
        //    yield return null;
        //}

        //fadeObj.SetActive(false);
    }

    /// <summary>
    /// 보스 사망 시 실행하는 코루틴(화면 연출, 텍스트 세팅, 결과 화면 띄우기)
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator BossDeadAnim()
    {
        print("튜토리얼에 맞는 이벤트 실행");
        yield return null;
        //nowColor = colors[(int)Colors.White];
        //nowAlpha = 0;

        //fadeObj.SetActive(true);

        //while (nowAlpha < 1)
        //{
        //    if (mainCam.orthographicSize > 6.5f)
        //    {
        //        mainCam.orthographicSize -= Time.deltaTime * 0.15f;
        //    }

        //    nowColor.a = nowAlpha;
        //    fadeImage.color = nowColor;

        //    if (nowAlpha < 0.4f)
        //    {
        //        nowAlpha += Time.deltaTime * 0.07f;
        //    }
        //    else
        //    {
        //        nowAlpha += Time.deltaTime * 0.5f;
        //    }

        //    yield return null;
        //}

        //gameEndObj.SetActive(true);
        //gameOverText.text = "Stage 1 Clear!";

        //yield return new WaitForSeconds(1f);

        //while (nowAlpha > 0)
        //{
        //    fadeImage.color = nowColor;
        //    nowColor.a = nowAlpha;
        //    nowAlpha -= Time.deltaTime;
        //    yield return null;
        //}

        //fadeObj.SetActive(false);
    }
}
