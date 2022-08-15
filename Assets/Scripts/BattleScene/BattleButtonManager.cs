using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleButtonManager : MonoBehaviour
{
    #region 행동 버튼모음
    [Header("행동 버튼모음")]
    [Tooltip("기본공격 버튼")]
    [SerializeField]
    private Button BasicAttackButton;

    [Tooltip("스킬선택 버튼")]
    [SerializeField]
    private Button SkillChooseButton;

    [Tooltip("스킬버튼들")]
    [SerializeField]
    private Button[] SkillButtons = new Button[7];

    [Tooltip("휴식 버튼")]
    [SerializeField]
    private Button RestButton;
    #endregion

    [Header("플레이어 오브젝트")]
    [SerializeField]
    private GameObject Player;

    // Start is called before the first frame update
    void Start()
    {
        ButtonsSetting();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ButtonsSetting()
    {
        BasicAttackButton.onClick.AddListener(() => Player.GetComponent<Player>().CloseAttackStart());
        SkillChooseButton.onClick.AddListener(() => print("Choose"));
        //SkillButtons[0].onClick.AddListener(() => print("Skills"));
        RestButton.onClick.AddListener(() => Player.GetComponent<Player>().RestStart());
    }
}
