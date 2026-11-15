using UnityEngine;

public class SniperAnimation : MonoBehaviour
{

    private Animator animator;
    private readonly int fireHash = Animator.StringToHash("Fire");
    private readonly int aimHash = Animator.StringToHash("Aim");
    private readonly int hitHash = Animator.StringToHash("Hit");
    private readonly int jumpUpHash = Animator.StringToHash("Jump_Up");
    private readonly int jumpDownHash = Animator.StringToHash("Jump_Down");
    private readonly int isDizzyHash = Animator.StringToHash("IsDizzy");
    private readonly int isJumpingHash = Animator.StringToHash("IsJumping");
    private readonly int dieHash = Animator.StringToHash("Die");

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void TriggerFire() => animator.SetTrigger(fireHash);
    public void TriggerAim() => animator.SetTrigger(aimHash);
    public void TriggerHit() => animator.SetTrigger(hitHash);
    public void TriggerJumpUp() => animator.SetTrigger(jumpUpHash);
    public void TriggerJumpDown() => animator.SetTrigger(jumpDownHash);
    public void TriggerDie()
    {
        SetDizzy(false);
        SetJumping(false);
        animator.SetTrigger(dieHash);
        animator.applyRootMotion = true;
        this.gameObject.transform.localPosition = new Vector3(0, 0, 0);
    }


    public void SetDizzy(bool isDizzy) => animator.SetBool(isDizzyHash, isDizzy);
    public void SetJumping(bool isJumping) => animator.SetBool(isJumpingHash, isJumping);
}
