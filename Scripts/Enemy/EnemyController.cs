using System;
using System.Collections;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;

public class EnemyController : MonoBehaviour,IDamageable
{
    [Header("Enemy Animator")]
    [SerializeField] private EnemyAnimator enemyAnimator;
    [SerializeField] private float deathDuration = 0.5f;

    [Header("Enemy Movement")]
    [SerializeField] private float turnSpeed = 12f;
    [SerializeField] private float stopDistance = 1.0f;

    [Header("SFX")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioClip hurtScreamClip;
    [SerializeField] private float hurtScreamVolume = 1f;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private float attackVolume = 1f;
    [SerializeField] private float footstepVolume = 1f;

    [Header("Enemy Sight")]
    [SerializeField] private float range = 20f;
    [SerializeField] private Transform raycastStartPoint;

    [Header("Attack Setting")]
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private float attackRange = 2.2f;

    [Header("Attack Collider")]
    [SerializeField] private Vector3 boxSize = new Vector3(1f, 1f, 1f);
    [SerializeField] private float castDistance = 2f;
    [SerializeField] private Vector3 offset = new Vector3(0, 1f, 0);
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Throwable Drop")]
    [SerializeField] private GameObject throwableDrop;

    [Header("Blood")]
    [SerializeField] private GameObject decalBlood;

    //Player Attack
    private bool isAttacking = false;

    private bool isDead = false;
    private Collider col;

    private float nextAttackTime = 0.0f;
    private bool CanAttack => Time.time >= nextAttackTime;

    private Animator animator;
    private bool playerInSight = false;
    private PlayerController playerController;
    private Transform player;
    private Transform enemy;
    private NavMeshAgent navMeshAgent;

    private EnemyTelegraph telegraph;

    private Vector3 toPlayer;

    private Vector3 toEnemy;

    private float footstepTimer = 0f;
    private float footstepInterval = 0.4f;

    void Start()
    {
        enemy = this.GetComponent<Transform>();
        playerController = PlayerController.Instance;
        player = playerController.gameObject.transform;
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        telegraph = this.GetComponentInChildren<EnemyTelegraph>();
        col = GetComponent<Collider>();
        enemyAnimator.OnEndAttack += EndAttack;
        enemyAnimator.OnStartAttack += StartAttack;
        enemyAnimator.TriggerIdle();
        decalBlood.SetActive(false);
    }

    public bool TakeDamage(GameObject other)
    {
        if (isDead) return true;
        
        Die();
        if(throwableDrop != null) Instantiate(throwableDrop, this.transform.position, Quaternion.LookRotation(this.transform.forward));
        decalBlood.SetActive(true);
        return true;
    }

    private void Die()
    {
        isDead = true;

        if (hurtScreamClip != null)
            AudioSource.PlayClipAtPoint(hurtScreamClip, transform.position, hurtScreamVolume);

        if (navMeshAgent != null)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }

        if (col != null) col.enabled = false;

        enemyAnimator.TriggerDie();
        if (telegraph != null) telegraph.HideTelegraph();

        StartCoroutine(DisableAfterDeath());
    }

    private IEnumerator DisableAfterDeath()
    {
        yield return new WaitForSeconds(deathDuration);
        gameObject.SetActive(false);
    }

    void onSwitchBladeHit(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var dmg))
        {
            Debug.Log("dmg.TakeDamage();");
            dmg.TakeDamage(this.gameObject);
        }
    }

    private void handleMovement()
    {
        if (player != null)
        {
            toPlayer = player.transform.position - enemy.position;
            toPlayer.y = 0;

            toEnemy = -toPlayer;

            if (playerInSight)
            {
                telegraph.ShowExclamation();
                
                Vector3 desti = player.position + toEnemy.normalized * stopDistance;
                NavMesh.SamplePosition(desti, out NavMeshHit hit, 5, NavMesh.AllAreas);
                navMeshAgent.SetDestination(hit.position);
            }


            LayerMask mask = ~LayerMask.GetMask("Enemy", "Bullet", "Katana", "PlayerAccessories","EnemyWeapon");
            Physics.Raycast(raycastStartPoint.position, toPlayer.normalized, out RaycastHit hitInfo, range, mask);
            if (hitInfo.collider != null && hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Debug.DrawRay(raycastStartPoint.position, toPlayer.normalized * 20f, Color.red);
                playerInSight = true;
            }
            else
            {
                Debug.DrawRay(raycastStartPoint.position, toPlayer.normalized * 20f, Color.green);
            }

            //Rotate to Player when in range
            if (playerInSight && toPlayer.sqrMagnitude < attackRange * attackRange)
            {
                Quaternion targetRot = Quaternion.LookRotation(-toEnemy, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
                if (!isAttacking && CanAttack)
                {
                    nextAttackTime = Time.time + attackCooldown;
                    isAttacking = true;
                    enemyAnimator.TriggerAttack();
                    enemyAnimator.SetRootMotion(true);
                }
            }

        }
    }

    
    private void StartAttack()
    {
        if (isAttacking)
        {
            if (attackClip != null)
                AudioSource.PlayClipAtPoint(attackClip, transform.position, attackVolume);

            if(CheckPlayerInFront()) {
                playerController.TakeDamage(this.gameObject);
            }
        }
    }

    private void EndAttack()
    {
        enemyAnimator.SetRootMotion(false);
        enemyAnimator.TriggerIdle();
        Debug.Log("EndAttack");
        isAttacking = false;
    }

    public bool CheckPlayerInFront()
    {
        Vector3 origin = transform.position + transform.TransformDirection(offset);

        Vector3 direction = transform.forward;

        Vector3 halfExtents = boxSize / 2f;

        RaycastHit hit;
        bool isHit = Physics.BoxCast(
            origin,
            halfExtents,
            direction,
            out hit,
            transform.rotation, 
            castDistance,
            playerLayer
        );

        if (isHit)
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.green;

        Vector3 origin = transform.position + transform.TransformDirection(offset);
        Vector3 endPosition = origin + transform.forward * castDistance;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(origin, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);

        Gizmos.matrix = Matrix4x4.TRS(endPosition, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);

        Gizmos.matrix = oldMatrix;

        Gizmos.DrawLine(origin, endPosition);
    }

    void Update()
    {
        if (isDead) return;

        if(!isAttacking) handleMovement();
        else {
            toPlayer = player.transform.position - enemy.position;
            toPlayer.y = 0;

            toEnemy = -toPlayer;
            Quaternion targetRot = Quaternion.LookRotation(-toEnemy, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }
        enemyAnimator.SetSpeed(Math.Clamp(navMeshAgent.velocity.magnitude / navMeshAgent.speed,0,1));

        if (navMeshAgent != null && navMeshAgent.velocity.sqrMagnitude > 0.1f)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            { // Now footstepVolume is declared.
                if (footstepClip != null) AudioSource.PlayClipAtPoint(footstepClip, transform.position, footstepVolume);
                footstepTimer = footstepInterval;
            }
        }
    }
}
