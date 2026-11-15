using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Parrying")]
    [SerializeField] private bool canParry;
    [SerializeField] private bool parryDestory;

    [Header("Effects")]
    [SerializeField] private GameObject parryEffect;
    [SerializeField] private AudioClip parryClip;
    [SerializeField] private float parryVolume = 1f;

    [Header("Movement")]
    public float speed = 10f;

    private int senderLayerIndex;
    private int targetLayerIndex;
    private int currentHitMask;

    private Vector3 direction;
    private bool hasHit = false;
    private bool hasParried = false;

    public void Initialize(Vector3 direction, int senderlayer, int targetlayer)
    {
        this.senderLayerIndex = senderlayer;
        this.targetLayerIndex = targetlayer;

        UpdateCollisionMask();

        this.direction = direction.normalized;
        this.transform.forward = this.direction;
        
        Destroy(this.gameObject, 10f);
    }

    public void UpdateCollisionMask()
    {
        currentHitMask = 0;
        int bulletLayer = this.gameObject.layer;
        for (int i = 0; i < 32; i++)
        {
            if (!Physics.GetIgnoreLayerCollision(bulletLayer, i))
            {
                currentHitMask |= (1 << i);
            }
        }
    }

    public void Reverse()
    {
        if (!canParry) return;
        if (hasParried) return; 
        
        hasParried = true;
        hasHit = false;

        if (parryClip != null)
            AudioSource.PlayClipAtPoint(parryClip, transform.position, parryVolume);

        if (parryEffect != null)
        {
            GameObject effectInstance = Instantiate(parryEffect, this.transform.position, Quaternion.LookRotation(-this.transform.forward));
            ParticleSystem ps = effectInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                float totalDuration = main.duration + main.startLifetime.constantMax;
                Destroy(effectInstance, totalDuration);
            }
        }

        if (parryDestory)
        {
            Destroy(gameObject);
        }
        else
        {
            int temp = senderLayerIndex;
            senderLayerIndex = targetLayerIndex;
            targetLayerIndex = temp;

            this.direction = -this.direction;
            this.transform.forward = this.direction;
        }
        
    }

    void Update()
    {
        if (hasHit) return;

        HandleRaycastMovement();
    }

    /// <summary>
    /// Handle Bullet Collision with Player
    /// </summary>
    private void HandleRaycastMovement()
    {
        float stepDistance = speed * Time.deltaTime;

        Debug.DrawRay(transform.position, direction * stepDistance, Color.yellow);

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, stepDistance, currentHitMask))
        {
            hasHit = true;
            
            transform.position = hit.point; 

            if (hit.collider.TryGetComponent<IDamageable>(out var dmg))
            {
                if(!dmg.TakeDamage(this.gameObject)) {
                    hasHit = false;
                    return;
                }
            }

            Destroy(gameObject);
        }

        else
        {
            transform.position += direction * stepDistance;
        }
    }
}