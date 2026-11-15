using System;
using System.Collections;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.Image;

public class EnemyShooterController : MonoBehaviour, IDamageable
{
    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;

    [Header("Animator")]
    [SerializeField] private EnemyShooterAnimator enemyShooterAnimator;
    [SerializeField] private float deathDuration = 0.5f;

    [Header("Movement")]
    [SerializeField] private float turnSpeed = 12f;
    [Tooltip("Enemy will approach the player when its {forwardRange} or more from player")]
    [SerializeField] private float forwardRange = 12f;
    [Tooltip("Enemy will leave the player when its {backwardRange} or lesser from player")]
    [SerializeField] private float backwardRange = 6f;
    

    [Header("SFX")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float footstepVolume = 1f;
    [SerializeField] private AudioClip hurtScreamClip;
    [SerializeField] private float hurtScreamVolume = 1f;
    [SerializeField] private AudioClip huhClip;
    [SerializeField] private float huhVolume = 1f;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private float shootVolume = 1f;

    [Header("Aim")]
    [SerializeField] private float range = 20f;
    [SerializeField] private float aimWindUp = 1.0f;
    [SerializeField] private float aimTelegraph = 0.2f;
    [SerializeField] private GameObject bulletSpawnPoint;
    [SerializeField] private Transform raycastStartPoint;

    [Header("Blood")]
    [SerializeField] private GameObject decalBlood;

    private bool isDead = false;

    private Collider col;

    private float timer;
    private GameObject player;
    private Transform enemy;
    private NavMeshAgent navMeshAgent;
    private EnemyTelegraph telegraph;
    private bool playerInSight = false;
    private CharacterController playerController;
    private PlayerController playerControl;
    private float bulletSpeed;

    private float footstepTimer = 0f;
    private float footstepInterval = 0.4f;
    private bool isHuhPlayed = false;

    public bool TakeDamage(GameObject other)
    {
        if(isDead)
            return true;
        Die();
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

        if (col != null)
        {
            col.enabled = false;
        }

        enemyShooterAnimator.TriggerDie();

        decalBlood.SetActive(true);

        telegraph.HideTelegraph();

        StartCoroutine(DisableAfterDeath());
    }

    private IEnumerator DisableAfterDeath()
    {
        yield return new WaitForSeconds(deathDuration);

        gameObject.SetActive(false);
    }

    void Start()
    {
        enemy = this.GetComponent<Transform>();
        playerControl = PlayerController.Instance;
        player = playerControl.gameObject;
        playerController = player.GetComponent<CharacterController>();
        bulletSpeed = bulletPrefab.GetComponent<Bullet>().speed;
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        telegraph = this.GetComponentInChildren<EnemyTelegraph>();
        col = GetComponent<Collider>();
        decalBlood.SetActive(false);
    }

    /// <summary>
    /// Use Player's CharacterController and BulletPrefab's speed, calculate the Predicted Position of the player
    /// </summary>
    private Vector3 GetPredictedAimPosition()
    {
        Vector3 playerVelocity = playerControl.GetNonDashVelocity(); // Use the non-dash velocity from PlayerController

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        float timeToIntercept = distanceToPlayer / bulletSpeed;

        Vector3 predictedPosition = player.transform.position + (playerVelocity * timeToIntercept);

        return predictedPosition;
    }

    void Update()
    {
        if(isDead) return;

        if (player != null)
        {
            //The vector from this object to player, ignore Y axis
            Vector3 toPlayer = player.transform.position - enemy.position;
            toPlayer.y = 0;

            //Ignore Bullet and Enemy Layer
            LayerMask mask = LayerMask.GetMask("Default","Player");
            if(Physics.Raycast(raycastStartPoint.position, toPlayer.normalized, out RaycastHit hitInfo, range, mask))
            {
                if(hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    if (!isHuhPlayed)
                    {
                        if (huhClip != null) AudioSource.PlayClipAtPoint(huhClip, transform.position, huhVolume);
                        isHuhPlayed = true;
                    }

                    Debug.DrawRay(raycastStartPoint.position, toPlayer.normalized * 20f, Color.red);
                    if(!playerInSight) {
                        enemyShooterAnimator.TriggerRun();
                        enemyShooterAnimator.TriggerAim();
                    }
                    playerInSight = true;
                    telegraph.HideTelegraph();

                    //Rotate to Player when in range

                    Quaternion targetRot = Quaternion.LookRotation(toPlayer, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);

                    //UI show exclamation mark
                    timer += Time.deltaTime;
                    if(timer > aimWindUp - aimTelegraph)
                    {
                        
                        telegraph.ShowExclamation();
                        telegraph.SetFillPercentage((timer - (aimWindUp - aimTelegraph)) / aimTelegraph);
                    }
                        
                    //Try Attack
                    if (timer > aimWindUp)
                    {
                        //Fire a bullet
                        if (shootClip != null)
                            AudioSource.PlayClipAtPoint(shootClip, transform.position, shootVolume);

                        Vector3 spawnPos = bulletSpawnPoint != null ? bulletSpawnPoint.transform.position : enemy.position;

                        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
                        Bullet bullet = bulletObj.GetComponent<Bullet>();
                        if (bullet != null)
                        {
                            Vector3 shootDirection = GetPredictedAimPosition() - spawnPos;
                            shootDirection.Normalize();
                            enemyShooterAnimator.TriggerShoot();
                            bullet.Initialize(shootDirection, this.gameObject.layer, LayerMask.NameToLayer("Player"));
                        }
                        timer = 0;
                    }
                }
                else
                {
                    timer = 0;
                    Debug.DrawRay(raycastStartPoint.position, toPlayer.normalized * 20f, Color.green);
                    telegraph.HideTelegraph();
                    if (playerInSight)
                    {
                        //UI show question mark
                        telegraph.ShowQuestion();

                    }
                }
            }
            
            

            //EnemyShooter Movement
            if(playerInSight)
            {
                if (toPlayer.sqrMagnitude > forwardRange * forwardRange)
                {
                    navMeshAgent.isStopped = false;
                    enemyShooterAnimator.SetSpeed(Math.Clamp(navMeshAgent.velocity.magnitude / navMeshAgent.speed, 0, 1));
                    navMeshAgent.SetDestination(player.transform.position);
                }
                else if (toPlayer.sqrMagnitude < backwardRange * backwardRange)
                {
                    navMeshAgent.isStopped = false;
                    navMeshAgent.updateRotation = false;
                    enemyShooterAnimator.SetSpeed(Math.Clamp(-navMeshAgent.velocity.magnitude / navMeshAgent.speed, -1, 0));
                    navMeshAgent.SetDestination(-toPlayer.normalized * 30);
                }
                else
                {
                    enemyShooterAnimator.SetSpeed(0);
                    navMeshAgent.isStopped = true;
                }
            }
        }

        if (navMeshAgent != null && !navMeshAgent.isStopped && navMeshAgent.velocity.sqrMagnitude > 0.1f)
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
