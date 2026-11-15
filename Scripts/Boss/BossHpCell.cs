using System;
using System.Collections;
using NUnit.Framework;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BossHpCell : MonoBehaviour
{
    [Header("UI Component")]
    [SerializeField] private Image slashImage;
    [SerializeField] private Image slashInActiveImage;
    [SerializeField] private Image hpImage;
    [SerializeField] private Sprite HP_Active;
    [SerializeField] private Sprite HP_InActive;
    [SerializeField] private Sprite Slash_Active;
    [SerializeField] private Sprite Slash_InActive;

    [Header("Setting")]

    

    private bool isHit;

    

    void Start()
    {
        hpImage.sprite = HP_Active;
        slashImage.gameObject.SetActive(false);
        slashInActiveImage.gameObject.SetActive(false);
    }

    [ContextMenu("TestHit")]
    public void Hit()
    {
        if(isHit) return;
        isHit = true;

        slashImage.gameObject.SetActive(true);
        StartCoroutine(hitCoroutine(0.5f,1f));
    }

    public void Hit(float hitDisplayTime, float slashActiveDuration)
    {
        if(isHit) return;
        isHit = true;

        slashImage.gameObject.SetActive(true);
        slashImage.fillAmount = 0f;
        StartCoroutine(hitCoroutine(hitDisplayTime,slashActiveDuration));
    }

    private IEnumerator hitCoroutine(float slashDuration, float slashActiveDuration)
    {
        float elapsedTime = 0f;
        float t = elapsedTime / slashDuration;
        while(t < 1)
        {
            elapsedTime += Time.deltaTime;
            t = elapsedTime / slashDuration;
            slashImage.fillAmount = Mathf.Lerp(0,1,t);
            
            Color hpImageColor = hpImage.color;
            hpImageColor.a = Mathf.Lerp(1,0,t);
            hpImage.color = hpImageColor;

            yield return null;
        }
        
        slashInActiveImage.gameObject.SetActive(true);
        elapsedTime = 0f;
        t = elapsedTime / slashActiveDuration;
        while(t < 1)
        {
            elapsedTime += Time.deltaTime;
            t = elapsedTime / slashActiveDuration;

            Color slashImageColor = slashImage.color;
            slashImageColor.a = Mathf.Lerp(1,0,t);
            slashImage.color = slashImageColor;

            yield return null;
        }
    }
}
