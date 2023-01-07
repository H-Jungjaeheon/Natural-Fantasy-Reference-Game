using UnityEngine;

public enum BulletState
{
    Firing,
    Deflecting
}

public class EnemysBullet : MonoBehaviour
{
    #region 총알 필수 요소들
    [Header("총알 요소들")]

    [SerializeField]
    [Tooltip("오브젝트 풀의 총알 종류")]
    private PoolObjKind thisBulletPoolObjKind;

    [SerializeField]
    [Tooltip("총알 데미지")]
    private int damage;

    [SerializeField]
    [Tooltip("총알 속도")]
    private float speed;

    public BulletState nowBulletState;
    #endregion

    #region 판별 요소들
    [Header("판별 요소들")]

    [Tooltip("총알 반사 가능한지 판별")]
    public bool isDeflectAble;

    [SerializeField]
    [Tooltip("총알 방어 가능한지 판별")]
    protected bool isDefendable;

    [SerializeField]
    [Tooltip("총알 회전 효과 여부 판별")]
    private bool isSpinning;
    #endregion

    #region 그 외
    [Header("그 외")]

    [SerializeField]
    [Tooltip("총알 회전 효과 : 속도 벡터")]
    private Vector3 basicSpinVector;


    private Vector3 moveSpeed;
    #endregion

    private void OnEnable()
    {
        moveSpeed.x = speed * -1;
        Reflex(BulletState.Firing);
    }

    void Update()
    {
        BulletMove();

        if (isSpinning)
        {
            AuraSpin();
        }
    }

    private void BulletMove()
    {
        transform.position += moveSpeed * Time.deltaTime;

        //position = new Vector2(Mathf.Cos(i * Mathf.Deg2Rad), Mathf.Sin(i * Mathf.Deg2Rad));
        //Fire(position, Vector2.one * 0.2f, (position - transform.position).normalized, 5, 1, Bullet.BulletType.Enemy, system);
    }

    /// <summary>
    /// 검기 회전 함수
    /// </summary>
    private void AuraSpin() => transform.eulerAngles += basicSpinVector * Time.deltaTime;

    public void Reflex(BulletState isReflexToPlayer)
    {
        nowBulletState = isReflexToPlayer;

        if (isReflexToPlayer == BulletState.Deflecting)
        {
            moveSpeed.x *= -1;
            transform.rotation = Quaternion.Euler(0, 0, 180);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        BasicUnitScript hitObjsUnitScript = collision.GetComponent<BasicUnitScript>();
        if (collision.CompareTag("Player") && nowBulletState == BulletState.Firing)
        {
            if (hitObjsUnitScript.nowDefensivePos == DefensePos.Right && isDefendable)
            {
                CamShake.CamShakeMod(true, 1.5f);
                hitObjsUnitScript.Hit(damage, true, EffectType.Defense);
            }
            else
            {
                CamShake.CamShakeMod(false,  2f);
                hitObjsUnitScript.Hit(damage, false, EffectType.Shock); //대각선
            }
            ReturnToObjPool();
        }
        else if (collision.CompareTag("Enemy") && nowBulletState == BulletState.Deflecting)
        {
            CamShake.CamShakeMod(false, 2f); //대각선
            hitObjsUnitScript.Hit(damage, false, EffectType.Shock);
            ReturnToObjPool();
        }
        else if (collision.CompareTag("ObjDestroy"))
        {
            ReturnToObjPool();
        }
    }

    private void ReturnToObjPool() => ObjectPool.Instance.ReturnObject(gameObject, (int)thisBulletPoolObjKind);
}
