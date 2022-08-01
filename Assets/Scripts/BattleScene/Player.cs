using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("���� ��Ÿ�� ���� ����")]
    [Tooltip("��Ÿ�� �� �̹���")]
    [SerializeField] private Image nullActionCoolTimeImage;
    [SerializeField] private Image ActionCoolTimeImage;
    [Tooltip("��Ÿ�� �� Y�� ��ġ ����")]
    [SerializeField] private float ActionCoolTimeImageYPos;

    [SerializeField] private float nowActionCoolTime;
    [SerializeField] private float maxActionCoolTime;


    Camera Cam;

    private void Awake()
    {
        StartSetting();
    }

    // Start is called before the first frame update
    void Start()
    {
        nowActionCoolTime = maxActionCoolTime;
    }

    // Update is called once per frame
    void Update()
    {
        UISetting();
    }
    private void StartSetting()
    {
        Cam = Camera.main;
    }
    private void UISetting()
    {
        nullActionCoolTimeImage.transform.position = Cam.WorldToScreenPoint(transform.position + new Vector3(0, ActionCoolTimeImageYPos, 0));
        ActionCoolTimeImage.fillAmount =  nowActionCoolTime / maxActionCoolTime;
        nowActionCoolTime -= Time.deltaTime;
    }
}
