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
    #endregion

    Vector2 movingPlusVector;

    private bool isEnemyHit;

    private void Start()
    {
        StartSetting();
    }
    void FixedUpdate()
    {
        AuraMove();
    }

    private void StartSetting()
    {
        damage += 0; //나중에 스킬 강화나 데미지 강화 레벨에 비례해서 증가
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
            Destroy(gameObject); //ObjectPool 변경
        }
    }
}
