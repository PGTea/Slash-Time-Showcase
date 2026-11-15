using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuUI : MonoBehaviour, IPointerEnterHandler
{
    public Button button;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    [SerializeField] private float hoverVolume = 1f;
    public AudioClip clickSound;
    [SerializeField] private float clickVolume = 1f;

    private void Start()
    {
        button.onClick.AddListener(OnStartGame);
    }

   
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
        { // Now hoverVolume is declared.
            audioSource.PlayOneShot(hoverSound, hoverVolume);
        }
    }

 
    public void OnStartGame()
    {
        if (clickSound != null)
        { // Now clickVolume is declared.
            audioSource.PlayOneShot(clickSound, clickVolume);
        }

        Invoke(nameof(LoadLevel), 0.2f);
    }

    private void LoadLevel()
    {
        GameManager.Instance.TransitionToLevel("Tutorial_Level01_Blockout");
    }
}