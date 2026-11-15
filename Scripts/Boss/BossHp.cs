using System.Linq;
using UnityEngine;

public class BossHp : MonoBehaviour
{
    [SerializeField] private float hitDisplayTime;
    [SerializeField] private float slashActiveDuration;
    private BossHpCell[] allHpCells;


    void Start()
    {
        allHpCells = this.GetComponentsInChildren<BossHpCell>();
    }

    public void Hit(int currentHealth)
    {
        for(int i = currentHealth; i < allHpCells.Length; i++)
        {
            allHpCells[i].Hit(hitDisplayTime,slashActiveDuration);
        }
    }
}
