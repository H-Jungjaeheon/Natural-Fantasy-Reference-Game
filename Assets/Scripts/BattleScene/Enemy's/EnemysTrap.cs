using System.Collections;
using UnityEngine;

public class EnemysTrap : MonoBehaviour
{
    [SerializeField]
    [Tooltip("해당 오브젝트 리지드바디")]
    private Rigidbody2D rigid;

    [SerializeField]
    [Tooltip("해당 오브젝트 애니메이터")]
    private Animator animator;

    [SerializeField]
    [Tooltip("장애물 사라지는 시간")]
    private float deleteTime;

    private Player player; //플레이어 스크립트 컴포넌트

    private int shootPower; //소환 시 날아가는 현재 속도값

    private bool isContacting; //플레이어와 접촉중인지 판별

    private bool isFalling; //땅에 닿기 전(날아가는 중)인지 판별

    private WaitForSeconds deleteDelay;

    private void Start()
    {
        player = BattleSceneManager.Instance.player;
        deleteDelay = new WaitForSeconds(deleteTime);
    }

    private void OnEnable()
    {
        isFalling = true;
        rigid.gravityScale = 2;
        shootPower = Random.Range(10, 24); //랜덤 속도값 지정
        rigid.AddForce(Vector2.left * shootPower, ForceMode2D.Impulse); //랜덤 속도값 만큼 힘을 가함
    }

    /// <summary>
    /// 오브젝트 충돌 시 실행
    /// </summary>
    /// <param name="collision"> 충돌한 오브젝트 콜라이더 </param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Floor") && isFalling) //날아가는 중 땅에 닿았을 때의 세팅
        {
            isFalling = false;
            rigid.velocity = Vector2.zero;
            rigid.gravityScale = 0;
            StartCoroutine(DestroyCount());
        }
        else if (collision.gameObject.CompareTag("Player")) //플레이어와 충돌했을 때 실행
        {
            isContacting = true;

            player.hitSlowCount++;

            if (isFalling == false)
            {
                player.SlowDebuff(true, 50);
            }
            else
            {
                StartCoroutine(WaitForFalling());
            }
        }
    }

    /// <summary>
    /// 오브젝트 충돌 범위 탈출 시 실행
    /// </summary>
    /// <param name="collision"> 탈출한 오브젝트 콜라이더 </param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) //탈출한 플레이어 디버프 비활성화 실행
        {
            isContacting = false;

            player.hitSlowCount--;
            
            if (player.hitSlowCount == 0) //다른 함정으로 인한 느리게 효과 발동중이라면, 디버프 해제 중지
            {
                player.SlowDebuff(false, 0);
            }
        }
    }

    /// <summary>
    /// 날아가며 플레이어와 충돌시 대기 함수 
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForFalling()
    {
        while (isFalling) //땅에 닿을 때까지 대기
        {
            yield return null;
        }

        if (isContacting) //땅에 닿고 나서도 충돌중이라면, 디버프 실행
        {
            player.SlowDebuff(true, 50);
        }
    }

    private IEnumerator DestroyCount()
    {
        yield return deleteDelay;
        //없어지는 애니메이션 실행 (애니메이션 함수로 오브젝트 풀 반환 제작)
        yield return new WaitForSeconds(2);
        ObjectPool.Instance.ReturnObject(gameObject, (int)PoolObjKind.SlimeEnemyTrap);
    }
}
