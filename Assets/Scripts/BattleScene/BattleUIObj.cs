using UnityEngine;

public enum ChangeBattleUIAnim
{
    Rest,
    Faint,
    SlowDebuff
}

public class BattleUIObj : MonoBehaviour
{
    [SerializeField]
    [Tooltip("현재 상태 표기 시 변경할 UI 크기")]
    private Vector2[] changeObjScale;

    [SerializeField]
    [Tooltip("현재 상태 표기 시 변경할 UI Y축 값")]
    private float[] changeYPos;

    private Vector2 changePosToSetActive;

    public void BattleUIObjSetActiveTrue(ChangeBattleUIAnim nowChangeBattleUIAnim)
    {
        changePosToSetActive.x = transform.position.x;
        changePosToSetActive.y = changeYPos[(int)nowChangeBattleUIAnim];
        transform.position = changePosToSetActive;
        gameObject.SetActive(true);
        transform.localScale = changeObjScale[(int)nowChangeBattleUIAnim];
    }

    public void BattleUIObjSetActiveFalse() => gameObject.SetActive(false);
}
