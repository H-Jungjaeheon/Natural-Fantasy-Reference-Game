using System.Collections;
using UnityEngine;

public class BossDeadEffect : MonoBehaviour
{
    [SerializeField]
    [Tooltip("빛줄기 애니메이션 오브젝트들 모음")]
    private GameObject[] lightObjs;

    WaitForSeconds lightDelay = new WaitForSeconds(0.6f);

    private void OnEnable()
    {
        StartCoroutine(DeadAnimStart());
    }

    IEnumerator DeadAnimStart()
    {
        for (int nowIndex = 0; nowIndex < lightObjs.Length; nowIndex++)
        {
            lightObjs[nowIndex].SetActive(true);
            yield return lightDelay;
        }
    }
}
