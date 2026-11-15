using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;

    [SerializeField]
    private PlayerController playerController;

    private readonly int defaultHash = Animator.StringToHash("Default");
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int attackHash1 = Animator.StringToHash("ATTACK1");
    private readonly int attackHash2 = Animator.StringToHash("ATTACK2");
    private readonly int attackHash3 = Animator.StringToHash("ATTACK3");
    private readonly int dashHash = Animator.StringToHash("QUICK SHIFT F");

    [Header("Animation Smoothing")]
    [SerializeField] private float dampTime = 0.1f; 

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        this.SetAnimSpeed(1f);
    }

    /// <summary>
    /// Trigger Dash Animation once
    /// </summary>
    public void TriggerIdle()
    {
        if (animator == null) return;
        animator.SetTrigger(defaultHash);
    }

    /// <summary>
    /// Trigger Attack Animation once
    /// </summary>
    public void TriggerAttack1()
    {
        if (animator == null) return;
        animator.SetTrigger(attackHash1);
    }

    /// <summary>
    /// Trigger Attack Animation once
    /// </summary>
    public void TriggerAttack2()
    {
        if (animator == null) return;
        animator.SetTrigger(attackHash2);
    }

    /// <summary>
    /// Trigger Attack Animation once
    /// </summary>
    public void TriggerAttack3()
    {
        if (animator == null) return;
        animator.SetTrigger(attackHash3);
    }

    /// <summary>
    /// Trigger Dash Animation once
    /// </summary>
    public void TriggerDash()
    {
        if (animator == null) return;
        animator.SetTrigger(dashHash);
    }

    /// <summary>
    /// Control Movement Animation (Idle -> Walk -> Run)
    /// </summary>
    /// <param name="targetSpeed">0 = idle, 0.3 = walk, 1 = run</param>
    public void UpdateMovementAnimation(float targetSpeed)
    {
        if (animator == null) return;
        animator.SetFloat(speedHash, targetSpeed, dampTime, Time.deltaTime);
    }

    public void SetAnimSpeed(float speed){
        animator.speed = speed;
    }

    public void CheckComboNext()
    {
        if (playerController != null)
            playerController.CheckComboNext();
    }

    public void SetCanReceiveInput()
    {
        if (playerController != null)
            playerController.SetCanReceiveInput();
    }

    public void EndCombo()
    {
        if (playerController != null)
            playerController.EndCombo();
    }
}
