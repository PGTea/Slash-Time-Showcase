using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;

public class Katana : MonoBehaviour
{
    [SerializeField] private Transform basePoint;
    [SerializeField] private Transform tipPoint;
    
    private bool isAttacking = false;
    
    [Header("Hitbox Settings")]
    [SerializeField] private float lingeringTime = 0.12f;

    private float bladeThickness = 0.05f;

    private LayerMask hitLayer;

    private HashSet<Collider> hitThisFrame = new HashSet<Collider>();

    public event System.Action<Collider> OnHit;

    private float bladeLength;

    private struct SwordState
    {
        public Vector3 center;
        public Quaternion rotation;
        public float time;
    }
    private List<SwordState> swordHistory = new List<SwordState>();


    void Start()
    {
        hitLayer = UpdateCollisionMask();
        bladeLength = Vector3.Distance(basePoint.position, tipPoint.position);
    }

    public void SetAttack(bool isAttack)
    {
        this.isAttacking = isAttack;
        if (!isAttack) 
        {
            hitThisFrame.Clear();
            swordHistory.Clear();
        }
        else
        {
            swordHistory.Add(new SwordState { center = (basePoint.position + tipPoint.position) / 2f, rotation = GetRotation(), time = Time.time });
        }
    }

    public int UpdateCollisionMask()
    {
        int currentHitMask = 0;
        int bulletLayer = this.gameObject.layer;
        for (int i = 0; i < 32; i++)
        {
            if (!Physics.GetIgnoreLayerCollision(bulletLayer, i))
            {
                currentHitMask |= (1 << i);
            }
        }

        return currentHitMask;
    }

    void LateUpdate()
    {
        if(!isAttacking) return;

        Vector3 currentCenter = (basePoint.position + tipPoint.position) / 2f;
        Quaternion currentRotation = GetRotation();

        swordHistory.Add(new SwordState { center = currentCenter, rotation = currentRotation, time = Time.time });

        while (swordHistory.Count > 1 && Time.time - swordHistory[0].time > lingeringTime)
        {
            swordHistory.RemoveAt(0);
        }

        Vector3 halfExtents = new Vector3(bladeThickness, bladeThickness, bladeLength) / 2f;

        for (int i = 1; i < swordHistory.Count; i++)
        {
            Vector3 prev = swordHistory[i - 1].center;
            Quaternion prevRot = swordHistory[i - 1].rotation;
            Vector3 curr = swordHistory[i].center;

            Vector3 sweepDirection = curr - prev;
            float sweepDistance = sweepDirection.magnitude;
            
            if (sweepDistance < 0.001f) continue;

            RaycastHit[] hits = Physics.BoxCastAll(
                prev, halfExtents, sweepDirection.normalized, prevRot, sweepDistance, hitLayer, QueryTriggerInteraction.Collide);

            foreach (RaycastHit hit in hits)
            {
                if (!hitThisFrame.Contains(hit.collider))
                {
                    hitThisFrame.Add(hit.collider);
                    OnHit?.Invoke(hit.collider);
                }
            }
        }
    }

    private Quaternion GetRotation()
    {
        Vector3 forward = tipPoint.position - basePoint.position;
        if (forward == Vector3.zero) return Quaternion.identity;

        return Quaternion.LookRotation(forward, transform.up);
    }
}
