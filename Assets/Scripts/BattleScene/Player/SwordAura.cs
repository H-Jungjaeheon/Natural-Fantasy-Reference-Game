using UnityEngine;

public enum NowSwordAuraState
{
    Basic,
    Enchanted
}

public class SwordAura : MonoBehaviour
{
    #region 검기 관련 스탯 모음
    private const int speed = 65;

    private int damage; //기본 대미지

    private int enchantedDamage; //강화 성공 시 대미지

    private bool isEnchanted;

    public bool IsEnchanted
    {
        get
        {
            return isEnchanted;
        }
        set
        {
            isEnchanted = value;

            if(value == true) 
            {
                sR.sprite = swordAuraImages[(int)NowSwordAuraState.Enchanted]; //강화 성공한 스프라이트로 교체
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

    Vector3 movingPlusVector;

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

        playerScript = bsm.player;

        spawnPlusVector = new Vector2(2.5f, 0);
        movingPlusVector = new Vector3(speed, 0, 0);
        basicSpinVector = new Vector3(0, 0, 1500);

        damage = 3; //기본 대미지 세팅
       
        damage += (3 * gm.statLevels[(int)UpgradeableStatKind.Damage] / 2); //레벨당 데미지 증가 연산 

        enchantedDamage = damage + damage / 2; //강화 성공 대미지 연산
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
    }

    /// <summary>
    /// 검기 움직임 함수
    /// </summary>
    private void AuraMove() => transform.position += (Vector3)movingPlusVector * Time.deltaTime;

    /// <summary>
    /// 검기 회전 함수
    /// </summary>
    private void AuraSpin() => transform.eulerAngles += basicSpinVector * Time.deltaTime * 3;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var busComponent = collision.gameObject.GetComponent<BasicUnitScript>();

        if (collision.gameObject.CompareTag("Enemy"))
        {
            int nowDamage = (isEnchanted) ? enchantedDamage : damage; //최종 대미지 판별(강화 실패 : 기본 대미지, 강화 성공 : 강화 대미지)
            
            busComponent.Hit(nowDamage, false);

            playerScript.GetBasicGood(); //재화 획득

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
            if (isEnchanted)
            {
                isEnchanted = false;
            }

            ObjectPool.Instance.ReturnObject(gameObject, (int)PoolObjKind.PlayerSwordAura);
        }
    }
}
