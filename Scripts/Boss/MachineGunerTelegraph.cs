using System;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset;

    [Header("MachineGun")]
    [SerializeField] private Image machineGunHP;
    [SerializeField] private Image statusImage;
    [SerializeField] private Image cdImage;
    [SerializeField] private RectTransform pointerRootRect;
    [SerializeField] private RectTransform pointerRect;
    private Transform[] allTransforms;

    [Header("Icon Image")]
    [SerializeField] private Sprite reloadSprite;
    [SerializeField] private Sprite fireSprite;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        // allTransforms = targetParent.GetComponentsInChildren<Transform>();
    }

    public void ShowMachineGunFire()
    {
        if (statusImage.sprite != fireSprite)
        {
            statusImage.sprite = fireSprite;
        }
        statusImage.gameObject.SetActive(true);
        SetCDFillPercentage(1);
    }

        public void ShowMachineGunReload()
    {
        if (statusImage.sprite != reloadSprite)
        {
            statusImage.sprite = reloadSprite;
        }
        statusImage.gameObject.SetActive(true);
        SetCDFillPercentage(0);
    }


    public void HideTelegraph()
    {
        if (pointerRootRect.gameObject.activeSelf)
        {
            pointerRootRect.gameObject.SetActive(false);
        }
    }

    public void SetCDFillPercentage(float fillAmount)
    {
        cdImage.fillAmount = fillAmount;
    }

    void LateUpdate()
    {
        if(target == null) return;

        Vector3 realPos = target.position + targetOffset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(realPos);

        if(screenPos.x <= 0 || screenPos.x >= Screen.width || 
            screenPos.y <= 0 || screenPos.y >= Screen.height)
        {
            float angle = Mathf.Atan2(screenPos.y - Screen.height / 2f, screenPos.x- Screen.width / 2f) * Mathf.Rad2Deg;
            
            pointerRootRect.position = new Vector3(Math.Clamp(screenPos.x, 0 , Screen.width), Math.Clamp(screenPos.y, 0 , Screen.height),0);
            pointerRootRect.localEulerAngles = new Vector3(0, 0, angle - 90);
            pointerRect.localEulerAngles = new Vector3(0, 0, 90 - angle);
        }
        else
        {
            pointerRootRect.position = screenPos;
            pointerRootRect.localEulerAngles = new Vector3(0, 0, 180);
            pointerRect.localEulerAngles = new Vector3(0, 0, 180);
        }
    }
}
