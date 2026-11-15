using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private int dialogIndex = -1;
    private bool isTirggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if(isTirggered) return;
        if (other.CompareTag("Player"))
        {
            isTirggered = true;
            GameOverRestart.Instance.Checkpoint = this.transform;
            PlayerController.Instance.HandlePaused();
            PlayerController.Instance.CancelDash();
            if (dialogIndex != -1) GameManager.Instance.StartDialogue(dialogIndex);
        }
    }
}
