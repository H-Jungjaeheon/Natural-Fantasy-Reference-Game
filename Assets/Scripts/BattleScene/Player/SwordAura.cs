using UnityEngine;

public enum NowSwordAuraState
{
    Basic,
    Enchanted
}

public class SwordAura : MonoBehaviour
{
    #region 검기 관련 스탯 모음
    [Header("검기 관련 스탯 모음")]
    [SerializeField]
    private float speed;

    [SerializeField]
    private int damage;

    private bool isEnchanted;

    public bool IsEnchanted
    {
        get
        {
            return isEnchanted;
        }
        set
        {
            if (value == false)
            {
                isEnchanted = value;
            }
            else 
            {
                isEnchanted = false;
                EnchantedSetting();
            }
        }
    }
    #endregion

    [SerializeField]
    [Tooltip("강화 검기 스프라이트들")]
    private Sprite[] swordAuraImages;

    [SerializeField]
    [Tooltip("검기 오브젝트 스프라이트 렌더러")]
    private SpriteRenderer sR;

    Vector2 movingPlusVector;

    Vector2 spawnPlusVector;

    Vector3 basicSpinVector;

    private bool isEnemyHit; //카메라 효과용 충돌 판별

    private BattleSceneManager bsm;

    private GameManager gm;

    private Player playerScript;

    private void Awake()
    {
        StartSetting();
    }

    private void OnEnable()
    {
        OnEnableSetting();
    }

    void Update()
    {
        AuraMove();
        AuraSpin();
    }

    /// <summary>
    /// 시작 세팅 함수
    /// </summary>
    private void StartSetting()
    {
        bsm = BattleSceneManager.Instance;
        gm = GameManager.Instance;

        playerScript = bsm.player.GetComponent<Player>();

        spawnPlusVector = new Vector2(2.5f, 0);
        movingPlusVector = new Vector2(speed, 0);
        basicSpinVector = new Vector3(0, 0, 2000);
    }

    /// <summary>
    /// 강화 성공 시 세팅 함수 
    /// </summary>
    private void EnchantedSetting()
    {
        sR.sprite = swordAuraImages[(int)NowSwordAuraState.Enchanted]; //강화 성공한 스프라이트로 교체
        damage += (2 + (gm.statLevels[(int)UpgradeableStatKind.Damage] / 3)); //강화 성공 기본 추가 데미지(2) + 레벨당 증가 단위
    }

    /// <summary>
    /// 활성화시 세팅 함수
    /// </summary>
    public void OnEnableSetting()
    {
        isEnemyHit = false; //카메라 효과용 충돌 판별 초기화

        sR.sprite = swordAuraImages[(int)NowSwordAuraState.Basic]; //스프라이트 초기화

        transform.position = bsm.player.transform.position + (Vector3)spawnPlusVector; //포지션 초기화(발사 시작점)
        transform.rotation = Quaternion.identity; //로테이션값 초기화
        
        damage = 0; //데미지 초기화
        damage += (4 + gm.statLevels[(int)UpgradeableStatKind.Damage] * 2); //레벨당 데미지 증가 연산 
    }

    /// <summary>
    /// 검기 움직임 함수
    /// </summary>
    private void AuraMove() => transform.position += (Vector3)movingPlusVector * Time.deltaTime;

    /// <summary>
    /// 검기 회전 함수
    /// </summary>
    private void AuraSpin() => transform.eulerAngles += basicSpinVector * Time.deltaTime;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var busComponent = collision.gameObject.GetComponent<BasicUnitScript>();

        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (playerScript.nowProperty == NowPlayerProperty.ForceProperty)
            {
                damage += damage / 4; //힘 속성 데미지 증가 연산
            }

            busComponent.Hit(damage, false);
            busComponent.GetBasicGood();

            if (isEnemyHit == false) //적에게 처음 닿았을 시
            {
                CamShake.CamShakeMod(false, 2f); //대각선
                isEnemyHit = true;
            }

            if (playerScript.nowProperty == NowPlayerProperty.FlameProperty)
            {
                busComponent.BurnDamageStart();
            }
        }
        else if(collision.gameObject.CompareTag("ObjDestroy"))
        {
            ObjectPool.Instance.ReturnObject(gameObject, (int)PoolObjKind.PlayerSwordAura);
        }
    }
}
