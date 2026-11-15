using UnityEngine.UI;
using UnityEngine;
using System;
using Unity.Mathematics;

public class MainUI : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField] private Image dashCooldown1;
    [SerializeField] private Image dashCooldown2;
    [Header("SlowMotion")]
    [SerializeField] private Image slowMotionCooldown;

    public void updateDashUI(float fillAmount)
    {
        if(fillAmount > 1) dashCooldown1.gameObject.SetActive(true);
        else dashCooldown1.gameObject.SetActive(false);
        if(fillAmount == 2) dashCooldown2.gameObject.SetActive(true);
        else dashCooldown2.gameObject.SetActive(false);
    }

    public void updateSlowMotionUI(float fillAmount)
    {
        slowMotionCooldown.fillAmount = Math.Clamp(fillAmount,0,1);;
    }
}
