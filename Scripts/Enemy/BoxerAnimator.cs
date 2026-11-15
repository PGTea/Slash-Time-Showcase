using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class BoxerAnimator : MonoBehaviour
{

    [Header("Transform Offset")]
    [SerializeField] private float attackRotationYOffset;

    private Animator animator;
    private readonly int idleHash = Animator.StringToHash("Idle");
    private readonly int runHash = Animator.StringToHash("Run");
    private readonly int attackHash = Animator.StringToHash("Attack");
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int dieHash1 = Animator.StringToHash("Die1");
    private readonly int dieHash2 = Animator.StringToHash("Die2");

    public event System.Action OnStartAttack;
    public event System.Action OnEndAttack;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void TriggerIdle(){
        animator.SetTrigger(idleHash);
        animator.applyRootMotion = false;
        this.gameObject.transform.localRotation = Quaternion.identity;
        this.gameObject.transform.localPosition = new Vector3(0,0,0);
    }
    public void TriggerAttack(){
        animator.SetTrigger(attackHash);
        this.gameObject.transform.localRotation = Quaternion.Euler(0, attackRotationYOffset, 0);
    }

    public void TriggerDie()
    {
        int randomDie = UnityEngine.Random.Range(0, 2);
        if (randomDie == 0)
        {
            animator.SetTrigger(dieHash1);
        } else {
            animator.SetTrigger(dieHash2);
        }
        animator.applyRootMotion = true;
        this.gameObject.transform.localPosition = new Vector3(0, 0, 0);
    }

    public void SetRootMotion(bool istrue) => animator.applyRootMotion = istrue;

    public void SetSpeed(float speed) => animator.SetFloat(speedHash, speed);

    public void StartAttack()
    {
        OnStartAttack?.Invoke();
    }

    public void EndAttack()
    {
        OnEndAttack?.Invoke();
    }
}
