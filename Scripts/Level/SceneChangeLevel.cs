using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChangeLevel : MonoBehaviour
{

    [Header("Scene Change Condition")]
    [Tooltip("Change Scene after clears all the enemy")]
    [SerializeField] private bool eliminateAllEnemy = false;

    [Tooltip("Change Scene after enter the zone")]
    [SerializeField] private bool enterZone = false;

    [Header("Transition Scene")]
    [SerializeField] private string sceneName;

    private bool isTransitioning = false;
    private float checkTimer = 0f;

    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;
    }

    private void Update()
    {
        if (eliminateAllEnemy && !isTransitioning && !PlayerController.Instance.getDead())
        {
            checkTimer += Time.deltaTime;
            if (checkTimer >= 0.5f)
            {
                checkTimer = 0f;
                if (GameObject.FindGameObjectWithTag("Enemy") == null && GameObject.FindGameObjectWithTag("EnemyShooter") == null)
                {
                    PlayerController.Instance.levelPass = true;
                    PlayerController.Instance.HandlePaused();
                    gameManager.TransitionToLevel(sceneName);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enterZone && other.CompareTag("Player") && !isTransitioning && !PlayerController.Instance.getDead())
        {
            PlayerController.Instance.levelPass = true;
            PlayerController.Instance.HandlePaused();
            gameManager.TransitionToLevel(sceneName);
        }
    }
}
