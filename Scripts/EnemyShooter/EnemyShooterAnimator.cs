using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyShooterAnimator : MonoBehaviour
{

    private Animator animator;
    private readonly int runHash = Animator.StringToHash("Run");
    private readonly int aimHash = Animator.StringToHash("Aim");
    private readonly int shootHash = Animator.StringToHash("Shoot");
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int dieHash = Animator.StringToHash("Die");


    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void TriggerRun() => animator.SetTrigger(runHash);
    
    public void TriggerAim() => animator.SetTrigger(aimHash);
    public void TriggerShoot() => animator.SetTrigger(shootHash);
    public void TriggerDie()
    {
        
        
        animator.SetTrigger(dieHash);
        animator.applyRootMotion = true;
        this.gameObject.transform.localPosition = new Vector3(0,0,0);
    } 


    public void SetSpeed(float speed) => animator.SetFloat(speedHash, speed);

}
