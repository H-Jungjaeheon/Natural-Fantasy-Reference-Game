using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField] private Image ActionCoolTimeImage;
    [SerializeField] private float ActionCoolTimeImageYPos;
    Camera Cam;

    private void Awake()
    {
        StartSetting();
    }

    // Start is called before the first frame update
    void Start()
    {
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
        ActionCoolTimeImage.transform.position = Cam.WorldToScreenPoint(transform.position + new Vector3(0, ActionCoolTimeImageYPos, 0));
    }
}
