using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    void Start()
    {
        StartCoroutine(StartOp());
    }

    IEnumerator StartOp()
    {
        WaitForSeconds opDelay = new WaitForSeconds(1);
        Color color = basicColor;
        float nowImageAlpha = 0;

        for (int nowFaidCount = 0; nowFaidCount < 2; nowFaidCount++)
        {
            yield return opDelay;

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

        yield return opDelay;

        while (nowImageAlpha > 0)
        {
            nowImageAlpha -= Time.deltaTime;
            color.a = nowImageAlpha;
            blackBGPanelImage.color = color;
            yield return null;
        }

        opObj.SetActive(false);
    }
}
