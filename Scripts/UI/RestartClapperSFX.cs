using UnityEngine;
using UnityEngine.UI;

public class RestartClapperSFX : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip clapClip;
    [SerializeField] private Image levelImageDigit1;
    [SerializeField] private Image levelImageDigit2;
    [SerializeField] private Image shotImageDigit1;
    [SerializeField] private Image shotImageDigit2;
    [SerializeField] private Sprite[] NumSprites;
    [Range(0f, 1f)][SerializeField] private float volume = 1f;

    public static RestartClapperSFX Instance;
    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void levelImageUpdate(int num)
    {
        levelImageDigit1.sprite = NumSprites[0];
        levelImageDigit2.sprite = NumSprites[num % 10];
    }

    public void shotImageUpdate(int num)
    {
        if(num > 99)
        {
            shotImageDigit1.sprite = NumSprites[9];
            shotImageDigit2.sprite = NumSprites[9];
        }
        else
        {
            shotImageDigit1.sprite = NumSprites[num / 10 % 10];
            shotImageDigit2.sprite = NumSprites[num % 10];
        }
    }

    public void PlayClap()
    {
        if (source == null || clapClip == null) return;
        source.PlayOneShot(clapClip, volume);
    }

    private void Reset()
    {
        source = GetComponent<AudioSource>();
    }
}
