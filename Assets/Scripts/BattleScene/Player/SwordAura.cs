using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAura : MonoBehaviour
{
    #region
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

    Vector2 movingPlusVector;

    Vector2 spawnPlusVector;

    private SpriteRenderer SR;

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
    }

    private void StartSetting()
    {
        SR = GetComponent<SpriteRenderer>();
        playerScript = BattleSceneManager.Instance.Player.GetComponent<Player>();
        BS = BattleSceneManager.Instance;
        spawnPlusVector = new Vector2(2.5f, 0);
        movingPlusVector = new Vector2(speed, 0);
    }

    private void EnchantedSetting()
    {
        print("강화 완료");
        SR.material.color = new Color(245, 110, 225);
        //이미지 교체로 코드 변경
        damage += 2; //나중에 스킬 강화나 데미지 강화 레벨에 비례해서 증가
    }

    public void OnEnableSetting()
    {
        transform.position = BS.Player.transform.position + (Vector3)spawnPlusVector;
        damage = 0;
        damage += (4 + 0); //나중에 스킬 강화나 데미지 강화 레벨에 비례해서 증가
        SR.material.color = new Color(144, 0, 123);
        isEnemyHit = false;
    }

    private void AuraMove() => transform.position += (Vector3)movingPlusVector * Time.deltaTime;

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
