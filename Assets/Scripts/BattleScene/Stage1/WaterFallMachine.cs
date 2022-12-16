using System.Collections;
using UnityEngine;
using DG.Tweening;

public enum TagKind
{
    PlayerProjectile,
    Player
}

public enum WaterFallKind
{
    Water,
    HotWater
}

public enum SwitchState
{
    Off,
    On
}

public class WaterFallMachine : MonoBehaviour
{
    [SerializeField]
    [Tooltip("스위치의 스프라이트 렌더러")]
    private SpriteRenderer switchSR;

    [SerializeField]
    [Tooltip("현재 폭포 종류 표시 오브젝트의 스프라이트 렌더러")]
    private SpriteRenderer displayObjSR;

    [SerializeField]
    [Tooltip("스위치의 표시 색 모음")]
    private Color[] switchColors;

    [SerializeField]
    [Tooltip("종류 표시 색 모음")]
    private Color[] displayColors;

    private bool isDrawing; //폭포 종류 바뀜 작동 중 판별

    private bool isWorked; //스위치 작동 여부 판별

    private bool isStop; //기능 정지 여부 판별

    private IEnumerator moveCoroutine; //현재 실행중인 스위치를 움직이는 코루틴 

    private IEnumerator changeCoroutine; //폭포의 종류 돌아가며 뽑는 코루틴

    private WaterFallKind waterFallKind; //현재 폭포의 종류

    private ObjectPool objectPoolInstance; //오브젝트 풀 싱글톤 인스턴스

    void Start()
    {
        objectPoolInstance = ObjectPool.Instance;
        moveCoroutine = SwitchDown();
        StartCoroutine(moveCoroutine);
    }

    /// <summary>
    /// 폭포 작동 스위치 내려오는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator SwitchDown()
    {
        transform.DOMoveY(0, 3.5f).SetEase(Ease.OutSine);

        while(transform.position.y > 0)
        {
            yield return null;
        }

        if (isDrawing == false)
        {
            isDrawing = true;
            changeCoroutine = StartDrawing();
            StartCoroutine(changeCoroutine);
        }

        yield return new WaitForSeconds(15f);

        moveCoroutine = SwitchUp();
        StartCoroutine(moveCoroutine);
    }

    /// <summary>
    /// 폭포 작동 스위치 올라가는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator SwitchUp()
    {
        transform.DOMoveY(24, 3.5f).SetEase(Ease.InSine);

        while (transform.position.y < 24)
        {
            yield return null;
        }

        yield return new WaitForSeconds(20f);

        if (isWorked == true)
        {
            isWorked = false;
            switchSR.color = switchColors[(int)SwitchState.Off];
        }

        moveCoroutine = SwitchDown();
        StartCoroutine(moveCoroutine);
    }

    /// <summary>
    /// 스위치 올라가기 전 까지 폭포의 종류 돌아가는 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator StartDrawing()
    {
        WaitForSeconds drawingDelay = new WaitForSeconds(1); //폭포 종류 변경 딜레이

        while (true)
        {
            waterFallKind = (waterFallKind == WaterFallKind.HotWater) ?
                waterFallKind = WaterFallKind.Water : waterFallKind + 1; //현재 폭포 순서가 HotWater이면, Water로 순서 변경 (아니면 다음 순서의 폭포로 변경)

            displayObjSR.color = displayColors[(int)waterFallKind];

            yield return drawingDelay;
        }
    }

    /// <summary>
    /// 폭포 작동 코루틴
    /// </summary>
    /// <returns></returns>
    IEnumerator Operation()
    {
        WaitForSeconds delay = new WaitForSeconds(2);

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        if (changeCoroutine != null)
        {
            StopCoroutine(changeCoroutine);
        }

        isDrawing = false;
        isWorked = true;
        switchSR.color = switchColors[(int)SwitchState.On];

        yield return delay;

        GameObject waterfallObj = null;
        Laser nowWaterfallObj;

        switch (waterFallKind)
        {
            case WaterFallKind.Water:
                waterfallObj = objectPoolInstance.GetObject((int)PoolObjKind.Waterfall);
                break;

            case WaterFallKind.HotWater:
                waterfallObj = objectPoolInstance.GetObject((int)PoolObjKind.HotWaterfall);
                break;
        }

        nowWaterfallObj = waterfallObj.GetComponent<Laser>();

        nowWaterfallObj.onEnablePos.x = Random.Range(-10, 6);
        nowWaterfallObj.onEnablePos.y = 6.75f;

        yield return delay;
        
        moveCoroutine = SwitchUp();
        StartCoroutine(moveCoroutine);
    }

    /// <summary>
    /// 애니메이션(동작) 작동 중지 함수
    /// </summary>
    public void StopFunction()
    {
        isStop = true;
        DOTween.KillAll();
        StopAllCoroutines();
    }

    /// <summary>
    /// 다른 오브젝트와 충돌했을 때의 처리
    /// </summary>
    /// <param name="collision"> 충돌한 오브젝트 콜라이더 </param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isStop == false && isWorked == false && collision.gameObject.CompareTag("PlayerProjectile"))
        {
            StartCoroutine(Operation());
        }
    }
}
