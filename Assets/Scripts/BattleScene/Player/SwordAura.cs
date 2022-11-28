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

    private bool isEnemyHit;

    private BattleSceneManager bsm;

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
        sR.sprite = swordAuraImages[(int)NowSwordAuraState.Enchanted];
        damage += 2; //나중에 스킬 강화나 데미지 강화 레벨에 비례해서 증가
    }

    /// <summary>
    /// 활성화시 세팅 함수
    /// </summary>
    public void OnEnableSetting()
    {
        sR.sprite = swordAuraImages[(int)NowSwordAuraState.Basic];
        transform.position = bsm.player.transform.position + (Vector3)spawnPlusVector;
        transform.rotation = Quaternion.identity;
        damage = 0;
        damage += (4 + 0); //나중에 스킬 강화나 데미지 강화 레벨에 비례해서 증가
        isEnemyHit = false;
    }

    /// <summary>
    /// 검기 움직임 함수
    /// </summary>
    private void AuraMove() => transform.position += (Vector3)movingPlusVector * Time.unscaledDeltaTime;

    /// <summary>
    /// 검기 회전 함수
    /// </summary>
    private void AuraSpin() => transform.eulerAngles += basicSpinVector * Time.unscaledDeltaTime;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        var busComponent = collision.gameObject.GetComponent<BasicUnitScript>();

        if (collision.gameObject.CompareTag("Enemy"))
        {
            busComponent.Hit(damage, false);
            busComponent.GetBasicGood();
            if (isEnemyHit == false)
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
