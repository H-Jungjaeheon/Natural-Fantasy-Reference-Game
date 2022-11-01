using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
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
                  
    WaitForSeconds faidDelay = new WaitForSeconds(1);

    void Start()
    {
        StartCoroutine(StartOp());
    }

    IEnumerator StartOp()
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

    IEnumerator GoToMainFaidOut()
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

    public void GoToMainSceneAnim()
    {
        opObj.SetActive(true);
        StartCoroutine(GoToMainFaidOut());
    }

    public void Quit()
    {
        Application.Quit();
    }
}
