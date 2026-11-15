using UnityEngine;

public interface IDamageable
{
    //Returns true if the damage applys, false otherwise
    bool TakeDamage(GameObject other);
}
