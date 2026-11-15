using UnityEngine;

public class PlayerPickupRange : MonoBehaviour
{
    public System.Action<Collider> OnEnter;
    public System.Action<Collider> OnExit;

    void OnTriggerEnter(Collider other)
    {
        OnEnter?.Invoke(other);
    }

    void OnTriggerExit(Collider other)
    {
        OnExit?.Invoke(other);
    }
}
