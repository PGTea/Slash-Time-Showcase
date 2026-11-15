using UnityEngine;
using UnityEngine.UI;

public class EnemyTelegraph : MonoBehaviour
{
    [Header("Icon and UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject uiContainer;

    [Header("Icon Image")]
    [SerializeField] private Sprite questionMarkSprite;
    [SerializeField] private Sprite exclamationMarkSprite;

    private Transform _camTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            _camTransform = Camera.main.transform;
        }
        HideTelegraph();
    }

    public void ShowQuestion()
    {
        if (iconImage.sprite != questionMarkSprite)
        {
            iconImage.sprite = questionMarkSprite;
        }
        uiContainer.SetActive(true);
        SetFillPercentage(1);
    }

    public void ShowExclamation()
    {
        if (iconImage.sprite != exclamationMarkSprite)
        {
            iconImage.sprite = exclamationMarkSprite;
        }
        uiContainer.SetActive(true);
    }

    public void HideTelegraph()
    {
        if (uiContainer.activeSelf)
        {
            uiContainer.SetActive(false);
        }
    }

    public void SetFillPercentage(float fillAmount)
    {
        iconImage.fillAmount = fillAmount;
    }

    void LateUpdate()
    {
        if (_camTransform != null)
        {
            transform.forward = _camTransform.forward;
        }
    }
}
