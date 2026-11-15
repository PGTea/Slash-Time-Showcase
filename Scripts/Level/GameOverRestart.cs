using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverRestart : MonoBehaviour
{
    private PlayerController player;
    private GameObject restartUI;
    private Animator restartAnimator;
    private RestartClapperSFX restartClapperSFX;
    public int[] deathCount { get; private set; }
    public Transform Checkpoint;

    [Header("Settings")]
    [SerializeField] private float animationDuration = 0.8f;

    private bool restarting = false;

    public static GameOverRestart Instance;
    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        player = PlayerController.Instance;
        restartUI = RestartClapperSFX.Instance.gameObject;
        restartAnimator = restartUI.GetComponent<Animator>();
        restartClapperSFX = restartUI.GetComponent<RestartClapperSFX>();
        restartUI.SetActive(false);
        restarting = false;
        deathCount = Enumerable.Repeat(1, SceneManager.sceneCountInBuildSettings).ToArray();
        SceneManager.sceneLoaded += HandleStateChanged;
    }

    private void Update()
    {
        if (restarting) return;
        if (player == null) return;

        if (player.getDead())
        {
            StartCoroutine(RestartFlow());
        }
    }

    private void HandleStateChanged(Scene scene, LoadSceneMode mode)
    {
        player = PlayerController.Instance;
        restartUI = RestartClapperSFX.Instance.gameObject;
        restartAnimator = restartUI.GetComponent<Animator>();
        restartUI.SetActive(false);
        restarting = false;
    }

    private IEnumerator RestartFlow()
    {
        restarting = true;

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        string sceneName = SceneManager.GetActiveScene().name;
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        restartUI.SetActive(true);

        restartClapperSFX.levelImageUpdate(sceneIndex);
        restartClapperSFX.shotImageUpdate(deathCount[sceneIndex]);
        deathCount[sceneIndex]++;

        string result = string.Join(", ", deathCount);
        Debug.Log(result);

        restartAnimator.Play("Restart_Clapper");

        yield return new WaitForSecondsRealtime(animationDuration);
        restartAnimator.Play("Idle");
        

        if (sceneName == "Tutorial_Level01_Blockout")
        {
            if(Checkpoint == null)GameManager.Instance.TransitionToLevelRestart(SceneManager.GetActiveScene().name);
            else GameManager.Instance.TransitionToCheckpoint(Checkpoint);
        }
        GameManager.Instance.TransitionToLevelRestart(SceneManager.GetActiveScene().name);
    }

    public void DeathCountClear()
    {
        deathCount = Enumerable.Repeat(1, SceneManager.sceneCountInBuildSettings).ToArray();
    }
}