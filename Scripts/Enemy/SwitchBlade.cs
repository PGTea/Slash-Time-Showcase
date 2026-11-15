using UnityEngine;

public class SwitchBlade : MonoBehaviour
{
    public System.Action<Collider> OnHit;

    void OnTriggerStay(Collider other)
    {
        OnHit?.Invoke(other);
    }
}
