using System.Collections;
using UnityEngine;

public class HitActionTiming : MonoBehaviour
{
    [SerializeField]
    [Tooltip("현재 타이밍 표시 바 오브젝트")]
    private GameObject barObj;

    [SerializeField]
    [Tooltip("성공 타이밍 애니메이션 오브젝트")]
    private GameObject animObj;

    [SerializeField]
    [Tooltip("타이밍 표시 바 애니메이터")]
    private Animator animator;

    private Vector2 barStartPos; //현재 타이밍 표시 바 시작 초기화 위치

    private Vector2 plusVector = new Vector2(0, 0); //움직임을 위한 벡터값

    private const float maxDistance = 1.1f; //현재 타이밍 표시 바 최대 X값

    private const string successAnim = "PlayerTimingUISuccessAnimation"; //타이밍 맞추기 성공 UI 연출 애니메이션 이름

    IEnumerator movingCoroutine; //현재 실행중인 타이밍 바 움직이는 코루틴

    WaitForSeconds delay = new WaitForSeconds(0.3f); //성공 애니메이션 딜레이

    private void Awake()
    {
        barStartPos = new Vector2(barObj.transform.localPosition.x, barObj.transform.localPosition.y);
    }

    /// <summary>
    /// 공격 히트액션 시작 시 실행하는 함수
    /// </summary>
    /// <param name="nowSpeed"> 현재 공격 타이밍 표시 바 속도 </param>
    public void HitActionTimingStart(float nowSpeed)
    {
        plusVector.x = nowSpeed;
        gameObject.SetActive(true);

        barObj.transform.localPosition = barStartPos;

        movingCoroutine = barMoving();
        StartCoroutine(movingCoroutine);
    }

    /// <summary>
    /// 히트 액션 실행 시(스페이스바를 눌렀을 때)
    /// </summary>
    /// <param name="isFail"> 히트 액션 성공 유무 </param>
    public IEnumerator pressSpace(bool isFail)
    {
        StopCoroutine(movingCoroutine);

        gameObject.SetActive(false);

        if (isFail == false)
        {
            animObj.SetActive(true);
            animator.Play(successAnim);
        }

        yield return delay;

        animObj.SetActive(false);
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

        gameObject.SetActive(false); //나중에 실패 연출 나오면 실패 연출 실행 후 끄기
    }
}
