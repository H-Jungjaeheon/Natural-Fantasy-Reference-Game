using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleButtonManager : MonoBehaviour
{
    #region �ൿ ��ư����
    [Header("�ൿ ��ư����")]
    [Tooltip("�⺻���� ��ư")]
    [SerializeField]
    private Button BasicAttackButton;

    [Tooltip("��ų���� ��ư")]
    [SerializeField]
    private Button SkillChooseButton;

    [Tooltip("��ų��ư��")]
    [SerializeField]
    private Button[] SkillButtons = new Button[7];

    [Tooltip("�޽� ��ư")]
    [SerializeField]
    private Button RestButton;
    #endregion

    [Header("�÷��̾� ������Ʈ")]
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
