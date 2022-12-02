using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitActionTiming : MonoBehaviour
{
    [SerializeField]
    [Tooltip("현재 타이밍 표시 바 오브젝트")]
    private GameObject barObj;

    private Vector2 barStartPos; //현재 타이밍 표시 바 시작 초기화 위치

    private Vector2 plusVector = new Vector2(0, 0);

    private const float maxDistance = 1.1f; //현재 타이밍 표시 바 최대 X값

    IEnumerator movingCoroutine;

    WaitForSeconds delay = new WaitForSeconds(0.5f);

    private void Awake()
    {
        barStartPos = new Vector2(barObj.transform.position.x, barObj.transform.position.y);
    }

    private void OnEnable()
    {
        barObj.transform.position = barStartPos;

        movingCoroutine = barMoving();
        StartCoroutine(movingCoroutine);
    }

    /// <summary>
    /// 공격 히트액션 시작 시 실행하는 함수
    /// </summary>
    /// <param name="nowSpeed"> 현재 공격 타이밍 표시 바 속도 </param>
    public void HitActionTimingStart(float nowSpeed)
    {
        plusVector.x = nowSpeed;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 히트 액션 실행 시(스페이스바를 눌렀을 때)
    /// </summary>
    /// <param name="isFail"> 히트 액션 성공 유무 </param>
    public IEnumerator pressSpace(bool isFail)
    {
        StopCoroutine(movingCoroutine);

        if (isFail)
        {
            
        }
        else
        {
            
        }

        //yield return delay;

        gameObject.SetActive(false); //나중에 실패 or 성공 연출 나오면 실패 연출 실행 후 끄기
        yield return null;
    }

    /// <summary>
    /// 현재 타이밍 바 움직이는 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator barMoving()
    {
        while (barObj.transform.localPosition.x < maxDistance)
        {
            barObj.transform.Translate(plusVector * Time.deltaTime);
            yield return null;
        }

        //yield return delay;

        gameObject.SetActive(false); //나중에 실패 연출 나오면 실패 연출 실행 후 끄기
    }
}
