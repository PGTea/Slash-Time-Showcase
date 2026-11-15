using UnityEngine;

public class MachineGunerAnimation : MonoBehaviour
{

    private Animator animator;
    private readonly int fireHash = Animator.StringToHash("Fire");
    private readonly int idleHash = Animator.StringToHash("Idle");
    private readonly int reloadHash = Animator.StringToHash("Reload");
    private readonly int hitHash = Animator.StringToHash("Hit");
    private readonly int dieHash = Animator.StringToHash("Die");

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void TriggerFire() => animator.SetTrigger(fireHash);
    public void TriggerIdle() => animator.SetTrigger(idleHash);
    public void TriggerReload() => animator.SetTrigger(reloadHash);
    
    public void TriggerReloadTime(float normalizedTime) => animator.Play(reloadHash,0,normalizedTime);
    public void TriggerHit() => animator.SetTrigger(hitHash);
    public void TriggerDie()
    {
        animator.SetTrigger(dieHash);
        animator.applyRootMotion = true;
        this.gameObject.transform.localPosition = new Vector3(0, 0, 0);
    }
}
