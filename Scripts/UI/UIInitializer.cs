using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class UIInitializer : MonoBehaviour
{
    [SerializeField] private GameObject firstSelected;
    [SerializeField] private GameObject controlMap;
    [SerializeField] private GameObject credits;

    private GameObject controlButton;
    private GameObject creditsButton;

    private bool isInControlMap;

    void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(firstSelected);
        GameManager.OnControlSchemeChanged += OnControlSchemeChanged;
    }

    void OnControlSchemeChanged()
    {
        string scheme = GameManager.Instance.gameObject.GetComponent<PlayerInput>().currentControlScheme;

        if (scheme == "Gamepad")
        {
            if (isInControlMap) EventSystem.current.SetSelectedGameObject(credits);
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
    }

    public void ShowControlMap()
    {
        isInControlMap = true;
        if(controlMap == null) return;
        controlMap.SetActive(true);
        controlButton = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(controlMap);
    }

    public void CancelControlMap()
    {
        isInControlMap = false;
        if(controlMap == null) return;
        controlMap.SetActive(false);
        EventSystem.current.SetSelectedGameObject(controlButton);
    }

    public void ShowCredits()
    {
        if(credits == null) return;
        credits.SetActive(true);
        creditsButton = EventSystem.current.currentSelectedGameObject;
        EventSystem.current.SetSelectedGameObject(credits);
    }

    public void CancelCredits()
    {
        if(credits == null) return;
        credits.SetActive(false);
        EventSystem.current.SetSelectedGameObject(creditsButton);
    }

    public void RestartGame()
    {
        PlayerController.Instance.TakeDamage(this.gameObject);
    }
    
    public void QuitGame()
    {
        GameManager.Instance.RestartGame();
    }
    
}
