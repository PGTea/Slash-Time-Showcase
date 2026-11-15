using UnityEngine;
using UnityEngine.UI;

public class PlayerTelegraph : MonoBehaviour
{
    [Header("Icon and UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject uiContainer;

    [Header("Icon Image")]
    [SerializeField] private Sprite aimSprite;

    private Transform _camTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            _camTransform = Camera.main.transform;
        }
        HideTelegraph();
    }

    public void ShowAim()
    {
        if (iconImage.sprite != aimSprite)
        {
            iconImage.sprite = aimSprite;
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
