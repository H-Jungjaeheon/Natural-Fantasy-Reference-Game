using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("오프닝 / 연출 관련 모음")]
    [SerializeField]
    [Tooltip("오프닝 오브젝트")]
    private GameObject opObj;

    [SerializeField]
    [Tooltip("검은 배경 판넬 이미지")]
    private Image blackBGPanelImage;

    [SerializeField]
    [Tooltip("팀 로고 이미지")]
    private Image teamLogoImage;

    [SerializeField]
    [Tooltip("색 초기화(검은 이미지)")]
    Color blackColor;

    [SerializeField]
    [Tooltip("색 초기화(투명 이미지)")]
    Color basicColor;

    [Header("옵션 관련 모음")]
    [SerializeField]
    [Tooltip("옵션 오브젝트")]
    private GameObject optionObj;

    private NowOptionState nowOptionState;

    WaitForSeconds faidDelay = new WaitForSeconds(1);

    void Awake()
    {
        StartCoroutine(StartOp());
    }

    IEnumerator StartOp() //타이틀 씬 시작 시 띄우는 로고 오프닝
    {
        Color color = basicColor;
        float nowImageAlpha = 0;

        for (int nowFaidCount = 0; nowFaidCount < 2; nowFaidCount++)
        {
            yield return faidDelay;

            while ((nowFaidCount == 0 && nowImageAlpha < 1) || (nowFaidCount == 1 && nowImageAlpha > 0))
            {
                nowImageAlpha += (nowFaidCount == 0) ? Time.deltaTime : -Time.deltaTime;
                color.a = nowImageAlpha;
                teamLogoImage.color = color;
                yield return null;
            }
        }

        color = blackColor;
        nowImageAlpha = 1;

        yield return faidDelay;

        while (nowImageAlpha > 0)
        {
            nowImageAlpha -= Time.deltaTime;
            color.a = nowImageAlpha;
            blackBGPanelImage.color = color;
            yield return null;
        }

        opObj.SetActive(false);
    }


    public void OpenOptionScreen() //옵션 버튼 클릭
    {
        nowOptionState = NowOptionState.FirstPage;
        optionObj.SetActive(true);
        StartCoroutine(PressEscInOptionScreen());
    }

    IEnumerator PressEscInOptionScreen() //옵션창에 있을 시 계속 실행됨
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                switch (nowOptionState)
                {
                    case NowOptionState.FirstPage:
                        optionObj.SetActive(false);
                        break;
                }

                if (nowOptionState == NowOptionState.FirstPage) //ESC키를 눌렀을 때 옵션의 첫번째 페이지면 반복 종료
                {
                    break;
                }
            }
            yield return null;
        }
    }

    public void GoToMainSceneAnim() //플레이 버튼 클릭
    {
        opObj.SetActive(true);
        StartCoroutine(GoToMainFaidOut());
    }

    IEnumerator GoToMainFaidOut() //메인화면 전환시 페이드 아웃
    {
        Color color = blackColor;
        float nowImageAlpha = 0;

        while (nowImageAlpha < 1)
        {
            nowImageAlpha += Time.deltaTime;
            color.a = nowImageAlpha;
            blackBGPanelImage.color = color;
            yield return null;
        }

        yield return faidDelay;
        
        SceneManager.LoadScene("Main");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
