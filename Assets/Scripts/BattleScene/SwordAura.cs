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

    private SpriteRenderer SR;

    private bool isEnemyHit;

    private void Awake()
    {
        StartSetting();
    }
    void FixedUpdate()
    {
        AuraMove();
    }

    private void EnchantedSetting()
    {
        print("강화 완료");
        SR.material.color = new Color(245, 110, 225);
        //이미지 교체로 코드 변경
        damage += 2; //나중에 스킬 강화나 데미지 강화 레벨에 비례해서 증가
    }

    private void StartSetting()
    {
        SR = GetComponent<SpriteRenderer>();
        damage += (4 + 0); //나중에 스킬 강화나 데미지 강화 레벨에 비례해서 증가
        movingPlusVector = new Vector2(speed, 0);
    }

    private void AuraMove()
    {
        transform.position += (Vector3)movingPlusVector * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<BasicUnitScript>().Hp_F -= damage;
            if(isEnemyHit == false)
            {
                CamShake.NowCamShakeStart(0.3f, 1);
                isEnemyHit = true;
            }
        }
        else if(collision.gameObject.CompareTag("ObjDestroy"))
        {
            damage = 0;
            SR.material.color = new Color(144, 0, 123);
            Destroy(gameObject); //ObjectPool 변경
        }
    }
}
