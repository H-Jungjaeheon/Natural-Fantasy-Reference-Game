using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemysTrap : MonoBehaviour
{
    [SerializeField]
    [Tooltip("해당 오브젝트 리지드바디")]
    private Rigidbody2D rigid;

    [SerializeField]
    [Tooltip("해당 오브젝트 애니메이터")]
    private Animator animator;

    private Player player; //플레이어 스크립트 컴포넌트

    private int shootPower; //소환 시 날아가는 현재 속도값

    private bool isContacting; //플레이어와 접촉중인지 판별

    private bool isFalling = true; //땅에 닿기 전(날아가는 중)인지 판별

    private void Start()
    {
        player = BattleSceneManager.Instance.player;
    }

    /// <summary>
    /// 오브젝트 활성화 시 세팅
    /// </summary>
    private void OnEnable()
    {
        shootPower = Random.Range(10, 24); //랜덤 속도값 지정
        rigid.AddForce(Vector2.left * shootPower, ForceMode2D.Impulse); //랜덤 속도값 만큼 힘을 가함
    }

    /// <summary>
    /// 오브젝트 충돌 시 실행
    /// </summary>
    /// <param name="collision"> 충돌한 오브젝트 콜라이더 </param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isContacting = true;
        }

        if (collision.gameObject.CompareTag("Floor") && isFalling)
        {
            isFalling = false;
            rigid.velocity = Vector2.zero;
            rigid.gravityScale = 0;
        }
        else if (collision.gameObject.CompareTag("Player") && isFalling == false)
        {
            player.SlowDebuff(true, 40);
        }
        else if(collision.gameObject.CompareTag("Player") && isFalling)
        {
            StartCoroutine(WaitForFalling());
        }
    }

    /// <summary>
    /// 오브젝트 충돌 범위 탈출 시 실행
    /// </summary>
    /// <param name="collision"> 탈출한 오브젝트 콜라이더 </param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isContacting = false;
        }

        if (collision.gameObject.CompareTag("Player") && isFalling == false) //탈출한 플레이어 디버프 비활성화 실행
        {
            player.SlowDebuff(false, 0);
        }
    }

    private IEnumerator WaitForFalling()
    {
        while (isFalling)
        {
            yield return null;
        }

        if (isContacting)
        {
            player.SlowDebuff(true, 40);
        }
    }
}
