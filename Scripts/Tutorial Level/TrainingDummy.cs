using UnityEngine;
using System.Collections;

public class TrainingDummy : MonoBehaviour, IDamageable
{
    [Header("Hit Settings")]
    public int hitsToDestroy = 3;
    private int currentHits = 0;

    [Header("References")]
    public Animator animator;
    public GameObject curtainObject;
    public GameObject dummyVisual;

    [Header("Animator Parameter")]
    public string hitTriggerName = "HitTrigger";

    [Header("Destroy Delay")]
    public float destroyDelay = 0.4f;

    private bool isDestroyed = false;

    public bool TakeDamage(GameObject other)
    {
        if (isDestroyed) return false;

        currentHits++;

        if (animator != null)
        {
            animator.SetTrigger(hitTriggerName);
        }

        Debug.Log("Dummy hit count: " + currentHits);

        if (currentHits >= hitsToDestroy)
        {
            isDestroyed = true;
            StartCoroutine(DestroyAfterDelay());
        }

        return true;
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);

        if (dummyVisual != null)
            dummyVisual.SetActive(false);
        else
            gameObject.SetActive(false);

        if (curtainObject != null)
            curtainObject.SetActive(false);
    }
}