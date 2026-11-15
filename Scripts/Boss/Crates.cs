using System;
using UnityEngine;

public class Crates : MonoBehaviour, IDamageable
{
    [Header("Settings")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int playerHitHealth = 3;
    private int currentHealth;

    [Header("Effects")]
    [SerializeField] private GameObject destoryEffect;
    [SerializeField] private AudioClip destroyedAudio;
    [SerializeField] private AudioClip hitAudio;

    [Header("Sniper Spot Transform")]
    public Transform LandingSpot;

    [Header("Throwable Drop")]
    [SerializeField] private GameObject throwableDrop;

    [SerializeField] public bool IsDestroyed;

    public event Action<Crates> OnDestroyed;
    private Material material;
    private Color color;

    void Start()
    {
        material = this.GetComponentInChildren<Renderer>().material;
        color = material.color;
        currentHealth = maxHealth;
        IsDestroyed = false;
    }

    public bool TakeDamage(GameObject other)
    {
        if (IsDestroyed) return false;

        if(other == PlayerController.Instance.gameObject) {
            currentHealth -= playerHitHealth;
            Debug.Log("PlayerHitCrate");
        }
        else currentHealth--;
        
        color.g = 1f/ maxHealth * currentHealth;
        color.b = 1f/ maxHealth * currentHealth;
        material.SetColor("_BaseColor", color);

        if (currentHealth <= 0)
        {
            if(throwableDrop!=null) Instantiate(throwableDrop, this.transform.position, Quaternion.LookRotation(this.transform.forward));
            DestroyCrate();
        }
        else
        {
            if (hitAudio != null)
            AudioSource.PlayClipAtPoint(hitAudio, transform.position);
        }

        return true;
    }

    private void DestroyCrate()
    {
        IsDestroyed = true;

        //JumpEndVFX
        GameObject effectInstance = Instantiate(destoryEffect,this.transform.position, Quaternion.Euler(90,0,0));
        ParticleSystem ps = effectInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            float totalDuration = main.duration + main.startLifetime.constantMax;
            Destroy(effectInstance, totalDuration);
        }

        OnDestroyed?.Invoke(this);

        if (destroyedAudio != null)
            AudioSource.PlayClipAtPoint(destroyedAudio, transform.position);

        gameObject.SetActive(false);
    }
}
