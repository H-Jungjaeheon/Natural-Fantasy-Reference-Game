using System.Collections;
using UnityEngine;
using System.Text;

public enum BossPhase
{
    PhaseOne = 1,
    PhaseTwo = 2,
    PhaseThree = 3
}

public enum EnemyProperty
{
    Mutant,
    Guardian,
    Rot,
    EvilSpirit,
    Angel,
    PropertyTotalNumber
}

public abstract class Enemy : BasicUnitScript
{
    [SerializeField]
    [Tooltip("사망 시 띄울 이펙트 오브젝트")]
    protected GameObject deadEffectObj;

    protected Vector2 speedVector = new Vector2(0f, 0f); //자세한 움직임이 필요한 패턴 사용 시 사용할 속도 벡터

    protected Vector2 spawnPos = new Vector2(0f, 0f); //패턴에 소환하는 오브젝트들 초기 위치 설정 벡터

    protected bool isPhysicalAttacking; //특정 패턴 사용 시 몸체 충돌 데미지 판정 판별 변수

    protected bool isChangePhase; //현재 페이지 변경중인지 판별

    protected const int maxRestLimitTurn = 3; //최대 휴식 턴 딜레이

    protected int restLimitTurn; //현재 휴식 턴 딜레이

    protected string[] pattonText; //텍스트로 불러온 패턴 번호들

    protected StringBuilder sb; //패턴 시드 파일 경로 검색에 쓰일 스트링 빌더

    protected int pattonCount; //현재 패턴 사용 횟수

    protected BossPhase nowPhase = BossPhase.PhaseOne; //현재 보스 페이즈

    /// <summary>
    /// 게임 처음 세팅
    /// </summary>
    protected override void StartSetting()
    {
        nowState = NowState.Standingby;
        nowDefensivePos = DefensePos.None;

        isWaiting = true;

        PattonText();

        bm.enemyCharacterPos = transform.position;
        bm.enemy = gameObject;

        Energy = MaxEnergy;
        Hp = MaxHp;

        restWaitTime = 1.85f;

        originalDamage = Damage;
        originalMaxActionCoolTime = maxActionCoolTime;
        originalRestWaitTime = restWaitTime;
        originalSpeed = Speed;

        StartCoroutine(WaitUntilTheGameStarts());
    }

    /// <summary>
    /// 오프닝 시간동안 대기
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitUntilTheGameStarts()
    {
        while (true)
        {
            if (bm.nowGameState == NowGameState.Playing)
            {
                actionCoolTimeObj.SetActive(true);
                WaitingTimeStart();
                break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 보스 패턴 텍스트 가져오기 (현재 페이즈에 맞는 텍스트 파일 : 2가지 경우 중 랜덤)
    /// </summary>
    /// <returns></returns>
    protected abstract void PattonText();

    /// <summary>
    /// 보스 행동 실행 함수
    /// </summary>
    protected abstract void RandBehaviorStart();
}
