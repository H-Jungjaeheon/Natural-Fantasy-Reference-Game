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

    private BattleSceneManager BS;

    private Player playerScript;

    private void Awake()
    {
        StartSetting();
    }

    private void OnEnable()
    {
        OnEnableSetting();
    }

    void FixedUpdate()
    {
        AuraMove();
        AuraSpin();
    }

    private void StartSetting()
    {
        playerScript = BattleSceneManager.Instance.Player.GetComponent<Player>();
        BS = BattleSceneManager.Instance;

        spawnPlusVector = new Vector2(2.5f, 0);
        movingPlusVector = new Vector2(speed, 0);
        basicSpinVector = new Vector3(0, 0, 2000);
    }

    private void EnchantedSetting()
    {
        sR.sprite = swordAuraImages[(int)NowSwordAuraState.Enchanted];
        damage += 2; //나중에 스킬 강화나 데미지 강화 레벨에 비례해서 증가
    }

    public void OnEnableSetting()
    {
        sR.sprite = swordAuraImages[(int)NowSwordAuraState.Basic];
        transform.position = BS.Player.transform.position + (Vector3)spawnPlusVector;
        transform.rotation = Quaternion.identity;
        damage = 0;
        damage += (4 + 0); //나중에 스킬 강화나 데미지 강화 레벨에 비례해서 증가
        isEnemyHit = false;
    }

    private void AuraMove() => transform.position += (Vector3)movingPlusVector * Time.deltaTime;

    private void AuraSpin() => transform.eulerAngles += basicSpinVector * Time.deltaTime;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<BasicUnitScript>().Hit(damage, false);
            if (isEnemyHit == false)
            {
                CamShake.CamShakeMod(false, 2f); //대각선
                isEnemyHit = true;
            }
            if (playerScript.nowProperty == NowPlayerProperty.FlameProperty)
            {
                collision.gameObject.GetComponent<BasicUnitScript>().BurnDamageStart();
            }
        }
        else if(collision.gameObject.CompareTag("ObjDestroy"))
        {
            ObjectPool.Instance.ReturnObject(gameObject, (int)PoolObjKind.PlayerSwordAura);
        }
    }
}
