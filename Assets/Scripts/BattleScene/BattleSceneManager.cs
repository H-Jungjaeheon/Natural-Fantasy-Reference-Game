using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum UnitKind
{
    Player,
    Enemy
}

public enum NowGameState
{
    Playing,
    Pausing,
    GameEnd
}

public class BattleSceneManager : Singleton<BattleSceneManager> //나중에 게임 오버 및 게임 클리어, 재화 관리
{
    [HideInInspector]
    public Vector2 playerCharacterPos; //플레이어 포지션

    [HideInInspector]
    public Vector2 enemyCharacterPos; //적 포지션

    [SerializeField]
    [Tooltip("게임 일시정지 판넬 오브젝트")]
    private GameObject gamePauseObj;

    [SerializeField]
    [Tooltip("씬 전환 시 필요한 판넬 오브젝트 이미지 컴포넌트")]
    private Image faidPanelObjImageComponent;

    [HideInInspector]
    public Player Player;

    [HideInInspector]
    public GameObject Enemy;

    private NowGameState nowGameState;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool nowGamePauseObjSetActive = (nowGameState == NowGameState.Playing);
            gamePauseObj.SetActive(nowGamePauseObjSetActive);
            nowGameState = (nowGamePauseObjSetActive) ? NowGameState.Pausing : NowGameState.Playing;
            Time.timeScale = nowGamePauseObjSetActive ? 0 : 1;
        }
    }

    public void ChangeScene(string changeSceneName)
    {
        StartCoroutine(SceneChangeFaidOut(changeSceneName));
    }

    IEnumerator SceneChangeFaidOut(string changeSceneName)
    {
        Color color = new Color(0,0,0,0);
        WaitForSecondsRealtime faidDelay = new WaitForSecondsRealtime(0.01f);
        float nowAlphaPlusPerSecond = 0.025f;
        float nowFaidPanelObjAlpha = 0;

        faidPanelObjImageComponent.transform.SetAsLastSibling();
        while (nowFaidPanelObjAlpha < 3)
        {
            color.a = nowFaidPanelObjAlpha;
            faidPanelObjImageComponent.color = color;
            
            nowFaidPanelObjAlpha += nowAlphaPlusPerSecond;
            yield return faidDelay;
        }

        Time.timeScale = 1;
        SceneManager.LoadScene(changeSceneName);
    }
}
