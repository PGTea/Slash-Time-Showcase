using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    Loading,
    Playing,
    Dialogue,
    Paused,
    EndScene
}

public class GameManager : MonoBehaviour
{
    [Header("Dialog")]
    [SerializeField] private TextAsset[] dialogJson;
    private bool[] dialogPlayedList;
    private Dialog dialog;
    [SerializeField] private float dialogCooldown = 0.3f;

    [Header("PauseMenu")]
    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private GameObject PauseControlMap;

    private PlayerController playercontroller;
    private PlayerInput playerInput;
    private bool dialogClickCD = false;

    public GameState State { get; private set; }

    public static event Action<GameState> OnGameStateChanged;
    public static event Action OnControlSchemeChanged;

    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        playercontroller = PlayerController.Instance;
        playerInput = this.GetComponent<PlayerInput>();
        playerInput.SwitchCurrentActionMap("Player");
        playerInput.actions.FindActionMap("UI");
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
        dialog = Dialog.Instance;
        dialog.OnDialogEnd += EndDialogue;
        InitializeGameEntryPoint();
    }

    public void TransitionToLevel(string level)
    {
        UpdateGameState(GameState.Loading);

        switch (level)
        {
            case "MainScene":
                SceneChanger.Instance.LoadScene("MainScene", 1f,1f, 0f,() =>
                {
                    InitializeGameEntryPoint();

                });
                break;
            case "Tutorial_Level01_Blockout":
                SceneChanger.Instance.LoadScene("Tutorial_Level01_Blockout", 1f,1f, 0f,() =>
                {
                    this.StartDialogue(0);
                    playercontroller.HandlePaused();
                });
                break;
            case "Level3_01":
                SceneChanger.Instance.LoadScene("Level3_01", 1f,1f, 0f,() =>
                {
                    UpdateGameState(GameState.Playing);
                    playercontroller.HandlePaused();
                });
                break;
            case "EndScene":
                SceneChanger.Instance.LoadScene("EndScene", 1f, 1f, 0.5f, () =>
                {
                    UpdateGameState(GameState.EndScene);
                });
                break;
            default:
                SceneChanger.Instance.LoadScene(level, 1f,1f, 0.5f,() =>
                {
                    UpdateGameState(GameState.Playing);
                    playercontroller.HandlePaused();
                });
                break;
        }

    }

public void TransitionToLevelRestart(string level)
{
    UpdateGameState(GameState.Loading);
    this.CancelPause();
    SceneChanger.Instance.LoadScene(level, 0f, 0.5f, 0f, () =>
    {
        UpdateGameState(GameState.Playing);
    });
}

    private void InitializeGameEntryPoint()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        dialogPlayedList = new bool[dialogJson.Length];

        if (currentScene == "MainScene")
        {
            UpdateGameState(GameState.MainMenu);
        }
        else if(currentScene == "MainScene")
        {
            UpdateGameState(GameState.EndScene);
        }
        else
        {
            UpdateGameState(GameState.Playing);
        }
    }

    public void TransitionToCheckpoint(Transform checkpoint)
    {
        UpdateGameState(GameState.Loading);
        Vector3 checkPointPosition = checkpoint.position;
        SceneChanger.Instance.LoadScene("Tutorial_Level01_Blockout", 0f, 0.3f, 0f,() =>
        {
            StartCoroutine(PlacePlayerAfterSceneStart(checkPointPosition));
        });
    }

    private IEnumerator PlacePlayerAfterSceneStart(Vector3 checkPointPosition)
    {
        
        Transform player = PlayerController.Instance.transform;
        PlayerController.Instance.GetComponent<CharacterController>().enabled = false;
        

        Vector3 pos = player.position;
        pos.x = checkPointPosition.x;
        pos.z = checkPointPosition.z;
        player.position = pos;

        GameOverRestart.Instance.Checkpoint = player;
        PlayerController.Instance.GetComponent<CharacterController>().enabled = true;
        UpdateGameState(GameState.Playing);
        yield return null;
    }  
    
    public void UpdateGameState(GameState newState)
    {
        if (State == newState) return;
        
        State = newState;
        Debug.Log($"GameState changed to: {newState}");
        
        OnGameStateChanged?.Invoke(newState);
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        playercontroller = PlayerController.Instance;
    }

    void OnRestartLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    #region INPUT
    //Inputs
    void OnMove(InputValue movementValue)
    {
        if(State == GameState.Playing) playercontroller.Move(movementValue);
    }

    void OnSlowMotion(InputValue inputValue)
    {
        if(State == GameState.Playing) playercontroller.SlowMotion(inputValue);
    }

    void OnAttack(InputValue inputValue)
    {
        if(State == GameState.Playing) playercontroller.Attack(inputValue);
    }

    void OnDash(InputValue inputValue)
    {
        if(State == GameState.Playing) playercontroller.Dash(inputValue);
    }

    void OnInteract(InputValue inputValue)
    {
        if(State == GameState.Playing) playercontroller.Interact(inputValue);
    }

    void OnPause()
    {
        if(State is GameState.MainMenu or GameState.Dialogue or GameState.Loading or GameState.EndScene || playercontroller.getDead()) return;
        if(State == GameState.Paused) {
            Time.timeScale = 1f;
            PauseMenu.SetActive(false);
            PauseControlMap.SetActive(false);
            UpdateGameState(GameState.Playing);
        }
        else
        {
            PauseMenu.SetActive(true);
            UpdateGameState(GameState.Paused);
            playercontroller.HandlePaused();
            Time.timeScale = 0f;
        }
    }

    /// <summary>
    /// Cancel Pause Menu without Changing GameState to Paused
    /// </summary>
    public void CancelPause()
    {
        Time.timeScale = 1f;
        PauseMenu.SetActive(false);
        PauseControlMap.SetActive(false);
    }

    void OnControlsChanged()
    {
        OnControlSchemeChanged?.Invoke();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        PauseMenu.SetActive(false);
        PauseControlMap.SetActive(false);
        UpdateGameState(GameState.Playing);
        GameOverRestart.Instance.DeathCountClear();
        TransitionToLevel("MainScene");
    }



    void OnSubmit(InputValue inputValue)
    {
        if(State == GameState.Dialogue){
            if (inputValue.isPressed)
            {
                
                if(!dialogClickCD) dialog.TryNextDialogue();
                StartCoroutine(clickCooldown());
            }
        }
    }

    void OnClick(InputValue inputValue)
    {
        if(State == GameState.Dialogue){
            if (inputValue.isPressed)
            {
                
                if(!dialogClickCD) dialog.TryNextDialogue();
                StartCoroutine(clickCooldown());
            }
        }
    }
    #endregion

    private IEnumerator clickCooldown()
    {
        dialogClickCD = true;
        yield return new WaitForSeconds(dialogCooldown);
        dialogClickCD = false;
    }

    public void StartDialogue(int index)
    {
        if(dialogPlayedList[index]) return;
        dialogPlayedList[index] = true;
        playercontroller.HandlePaused();
        UpdateGameState(GameState.Dialogue);
        playerInput.SwitchCurrentActionMap("UI");
        dialog.StartDialogue(dialogJson[index].text);
    }

    void EndDialogue()
    {
        UpdateGameState(GameState.Playing);
        playerInput.SwitchCurrentActionMap("Player");
    }
}
