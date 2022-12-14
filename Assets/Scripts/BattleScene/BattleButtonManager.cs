using UnityEngine;
using UnityEngine.UI;

public enum ButtonPage
{
    FirstPage,
    SecondPage
}

public enum Skills
{
    FirstSkill
}

public class BattleButtonManager : Singleton<BattleButtonManager>
{
    [Tooltip("버튼 오브젝트")]
    public GameObject buttonObj;

    #region 행동 버튼모음
    [Header("행동 버튼모음")]
    [Tooltip("스킬선택 버튼")]
    [SerializeField]
    private Button SkillChooseButton;

    [Tooltip("스킬선택 나가기 버튼")]
    [SerializeField]
    private Button OutSkillChooseButton;

    [Tooltip("스킬버튼들")]
    [SerializeField]
    private Button[] SkillButtons = new Button[7];
    #endregion

    [Header("각 버튼들 페이지 오브젝트")]
    [SerializeField]
    private GameObject[] ButtonPageObjs = new GameObject[2];

    [SerializeField]
    [Tooltip("스킬 스크롤러 조절 오브젝트")]
    private GameObject content;

    public ButtonPage nowButtonPage;

    private Player playerComponent;

    public void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else if (isDontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
    }

    void Start()
    {
        StartSetting();
    }

    /// <summary>
    /// 시작 세팅 모음
    /// </summary>
    private void StartSetting()
    {
        playerComponent = BattleSceneManager.Instance.player;
        SkillChooseButton.onClick.AddListener(() => ButtonsPageChange(false, true));
        OutSkillChooseButton.onClick.AddListener(() => ButtonsPageChange(true, false));
        SkillButtons[(int)Skills.FirstSkill].onClick.AddListener(() => playerComponent.SkillUse(1, 5));
    }

    public void ActionButtonSetActive(bool setActive) => buttonObj.SetActive(setActive);

    /// <summary>
    /// 행동 버튼 비활성화 or 활성화
    /// </summary>
    /// <param name="firstPageSetActive"> 기본 버튼 페이지 활성화 여부(false면 스킬 버튼 페이지 활성화) </param>
    /// <param name="isScrollInitialization"> 스킬 버튼 스크롤 초기화 여부 </param>
    public void ButtonsPageChange(bool firstPageSetActive, bool isScrollInitialization) 
    {
        if (firstPageSetActive) //현재 페이지 상태 변경
        {
            nowButtonPage = ButtonPage.FirstPage;
        }
        else
        {
            nowButtonPage = ButtonPage.SecondPage;

            if (isScrollInitialization) //초기화 매개변수가 참일때 스킬 스크롤 초기화
            {
                content.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 20, 0);
            }
        }

        ButtonPageObjs[(int)ButtonPage.FirstPage].SetActive(firstPageSetActive); //기본 버튼 페이지 활성 or 비활성
        ButtonPageObjs[(int)ButtonPage.SecondPage].SetActive(!firstPageSetActive); //스킬 버튼 페이지 활성 or 비활성
    }
}
