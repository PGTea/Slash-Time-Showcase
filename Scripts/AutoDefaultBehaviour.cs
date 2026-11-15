using UnityEngine;

public class AutoTriggerDefault : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetTrigger("Default");
    }
}
