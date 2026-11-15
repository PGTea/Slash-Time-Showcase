using System.Collections.Generic;
using UnityEngine;

public class SniperController : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private BossHp bossHP;
    private int currentHealth;

    [Header("Targeting")]
    [SerializeField] private PlayerController player;
    private PlayerTelegraph playerTelegraph;

    [Header("Animation")]
    [SerializeField] private SniperAnimation sniperAnim;
    [SerializeField] private float deathDuration = 0.5f;

    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject bulletSpawnPoint;

    [Header("Movement & Jump")]
    [SerializeField] private float turnSpeed = 12f;
    [SerializeField] private Crates startSpot;
    [SerializeField] private Crates[] sniperSpots;
    [SerializeField] private float jumpDuration = 1.0f; 
    [SerializeField] private float jumpHeight = 5f;

    [Header("Aim")]
    [SerializeField] private float range = 20f;
    [SerializeField] private float aimWindUp = 1.0f;
    [SerializeField] private float aimTelegraph = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private float shootVolume = 1f;

    [Header("Sniper Settings")]
    [SerializeField] private float reloadTime = 2.0f;
    [SerializeField] private float stunTime = 5f;

    [Header("VFX")]
    [SerializeField] private GameObject jumpEndEffect;

    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip aimClip;
    [SerializeField] private float aimVolume = 0.5f;
    [SerializeField] private AudioClip targetLockClip;
    [SerializeField] private float targetLockVolume = 1f;
    [SerializeField] private AudioClip attackClip;
    private bool aimClipPlayed;
    private bool targetLockPlayed;
    [SerializeField] private float aimClipTime = 0.5f;


    private float timer;
    private Transform enemy;

    private bool canShoot = true;
    private bool isReloading = false;

    private bool isJumping = false;

    private Crates currentCrate;
    private bool isStunned = false;
    private float stunTimer = 0f;
    private bool isDead = false;

    //Collision LayerMask
    private int bulletLayerMask;
    private RaycastHit[] hitResults = new RaycastHit[20];

    private float bulletSpeed;

    private CharacterController playerController;

    private int damageAfterStun = 0;

    public bool TakeDamage(GameObject other)
    {
        if (isDead) return true;
        
        if(!currentCrate.IsDestroyed) return false;
        if(other.gameObject.tag == "Bullet") return true;

        currentHealth--;
        bossHP.Hit(currentHealth);
        damageAfterStun++;
        if(damageAfterStun >= 2) {
            damageAfterStun = 0;
            stunTimer = stunTime;
        }

        if (sniperAnim != null) sniperAnim.TriggerHit();
        
        Debug.Log(currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
        
        return true;
    }

    private void Die()
    {
        isDead = true;
        playerTelegraph.HideTelegraph();

        StopAllCoroutines();
        
        if (isJumping)
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 50f, LayerMask.GetMask("Default")))
            {
                transform.position = hit.point;
            }
            else
            {
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            }
            isJumping = false;
        }

        if (sniperAnim != null) sniperAnim.TriggerDie();
        StartCoroutine(DisableAfterDeath());
    }

    private System.Collections.IEnumerator DisableAfterDeath()
    {
        yield return new WaitForSeconds(deathDuration);
        gameObject.SetActive(false);
    }

    void Start()
    {
        currentHealth = maxHealth;
        enemy = this.GetComponent<Transform>();
        playerTelegraph = player.GetComponentInChildren<PlayerTelegraph>();
        bulletSpeed = bulletPrefab.GetComponent<Bullet>().speed;
        playerController = player.GetComponent<CharacterController>();
        audioSource.volume = targetLockVolume; // Set volume for the AudioSource playing targetLockClip
        audioSource.clip = targetLockClip;
        if (sniperAnim == null) sniperAnim = GetComponentInChildren<SniperAnimation>();

        if (startSpot != null)
        {
            UpdateCurrentCrate(startSpot);
        }

        // Automatically calculate the layer mask based on the bullet's physics collision matrix
        if (bulletPrefab != null)
        {
            bulletLayerMask = 0;
            int bulletLayer = bulletPrefab.layer;
            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(bulletLayer, i))
                {
                    bulletLayerMask |= (1 << i);
                }
            }
        }
        else
        {
            bulletLayerMask = ~0; // Fallback to all layers if bullet is missing
        }
    }

    void Update()
    {
        if (isDead) return;

        // If in a stunned state, pause all actions and start the timer
        if (isStunned)
        {
            stunTimer += Time.deltaTime;
            if (stunTimer >= stunTime)
            {
                isStunned = false;
                if (sniperAnim != null) sniperAnim.SetDizzy(false);
                stunTimer = 0f;
                JumpToRandomSpot();
            }
            return;
        }

        if (isJumping) return;

        if (player != null)
        {
            Vector3 toPlayer = player.transform.position - enemy.position;
            Vector3 aimToPlayer = toPlayer;
            aimToPlayer.y = 0;

            if (canShoot && !isReloading)
            {
                Quaternion targetRot = Quaternion.LookRotation(aimToPlayer, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);


                timer += Time.deltaTime;
                if(timer > aimWindUp - aimTelegraph)
                {
                    playerTelegraph.ShowAim();
                    playerTelegraph.SetFillPercentage((timer - (aimWindUp - aimTelegraph)) / aimTelegraph);
                    if (!aimClipPlayed)
                    {
                        aimClipPlayed = true;
                        AudioSource.PlayClipAtPoint(aimClip, transform.position, aimVolume);
                    } 
                }

                if(timer > aimWindUp - aimClipTime)
                {
                    if (!targetLockPlayed)
                    {
                        targetLockPlayed = true;
                        audioSource.Play();
                    }
                }

                if (timer > aimWindUp)
                {
                    audioSource.Stop();
                    targetLockPlayed = false;
                    aimClipPlayed = false;
                    if (shootClip != null)
                        AudioSource.PlayClipAtPoint(shootClip, transform.position, shootVolume);

                    if (sniperAnim != null) sniperAnim.TriggerFire();

                    Vector3 spawnPos = bulletSpawnPoint != null ? bulletSpawnPoint.transform.position : enemy.position;

                    GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
                    Bullet bullet = bulletObj.GetComponent<Bullet>();
                    if (bullet != null)
                    {
                        Vector3 shootDirection = GetPredictedAimPosition() - spawnPos;
                        shootDirection.Normalize();
                        bullet.Initialize(shootDirection, this.gameObject.layer, LayerMask.NameToLayer("Player"));
                    }

                    playerTelegraph.HideTelegraph();
                    timer = 0;

                    StartCoroutine(ReloadRoutine());
                }
            }
            else if (!canShoot)
            {
                timer = 0;
                Debug.DrawRay(enemy.position, toPlayer.normalized * 20f, Color.green);
            }

            // Draw debug lines for all available spots during gameplay
            if (sniperSpots != null)
            {
                foreach (Crates crate in sniperSpots)
                {
                    if (crate == null || crate.IsDestroyed) continue;

                    Vector3 origin = crate.LandingSpot.position;
                    Vector3 target = player.transform.position;
                    Vector3 direction = target - origin;

                    bool canHit = HasLineOfSight(origin, target, crate);
                    Vector3 endPoint = direction.magnitude < range ? target : origin + direction.normalized * range;
                    
                    Debug.DrawLine(origin, endPoint, canHit ? Color.red : Color.green);
                }
            }
        }
    }

    /// <summary>
    /// Use Player's CharacterController and BulletPrefab's speed, calculate the Predicted Position of the player
    /// </summary>
    private Vector3 GetPredictedAimPosition()
    {
        Vector3 playerVelocity = player.GetNonDashVelocity(); // Use the non-dash velocity from PlayerController

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        float timeToIntercept = distanceToPlayer / bulletSpeed;

        Vector3 predictedPosition = player.transform.position + (playerVelocity * timeToIntercept);

        return predictedPosition;
    }

    private System.Collections.IEnumerator ReloadRoutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        isReloading = false;
        if (!isStunned)
        {
            JumpToRandomSpot();
        }
    }

    private void JumpToRandomSpot()
    {
        if (sniperSpots == null || sniperSpots.Length == 0) return;

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < sniperSpots.Length; i++)
        {
            // Only filter out unbroken crates
            if (!sniperSpots[i].IsDestroyed)
            {
                availableIndices.Add(i);
            }
        }

        // If no crates remain on the field, stop jumping
        if (availableIndices.Count == 0)
        {
            return;
        }

        List<int> losIndices = new List<int>(availableIndices);
        while (losIndices.Count > 0)
        {
            int randomIndex = Random.Range(0, losIndices.Count);
            int spotIndex = losIndices[randomIndex];
            Crates potentialCrate = sniperSpots[spotIndex];

            if (HasLineOfSight(potentialCrate.LandingSpot.position, player.transform.position, potentialCrate))
            {
                JumpTo(potentialCrate);
                return;
            }
            else
            {
                losIndices.RemoveAt(randomIndex);
            }
        }

        Debug.Log("Sniper Random Jump");
        int fallbackIndex = availableIndices[Random.Range(0, availableIndices.Count)];
        JumpTo(sniperSpots[fallbackIndex]);
    }

    /// <summary>
    /// Checks if there is line of sight from originPoint to targetPoint
    /// </summary>
    private bool HasLineOfSight(Vector3 originPoint, Vector3 targetPoint, Crates originCrate = null)
    {
        Vector3 origin = originPoint;
        Vector3 target = targetPoint;
        Vector3 direction = target - origin;

        int hitCount = Physics.RaycastNonAlloc(origin, direction.normalized, hitResults, range, bulletLayerMask);
        float closestDistance = float.MaxValue;
        Transform closestValidHit = null;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hitResults[i];
            
            if (originCrate != null && hit.transform.IsChildOf(originCrate.transform)) 
                continue;

            if (hit.distance < closestDistance)
            {
                closestDistance = hit.distance;
                closestValidHit = hit.transform;
            }
        }

        if (closestValidHit != null)
        {
            if (closestValidHit == player.transform || closestValidHit.GetComponent<PlayerController>() != null)
            {
                return true;
            }
        }

        return false;
    }

    public void JumpTo(Crates targetCrate)
    {
        if (!isJumping)
        {
            if (currentCrate != targetCrate) 
            {
                StartCoroutine(JumpRoutine(targetCrate));
                UpdateCurrentCrate(targetCrate);
            }
        }
    }

    private System.Collections.IEnumerator JumpRoutine(Crates targetCrate)
    {
        isJumping = true;
        timer = 0;

        Vector3 startPos = transform.position;
        Vector3 endPos = targetCrate.LandingSpot.position;
        float timeElapsed = 0f;

        if (sniperAnim != null) sniperAnim.TriggerJumpUp();
        if (sniperAnim != null) sniperAnim.SetJumping(true);

        while (timeElapsed < jumpDuration)
        {
            float t = timeElapsed / jumpDuration;

            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);

            currentPos.y += jumpHeight * Mathf.Sin(Mathf.PI * t);

            transform.position = currentPos;

            Vector3 lookDir = endPos - startPos;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), turnSpeed * Time.deltaTime);
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;

        //JumpEndVFX
        GameObject effectInstance = Instantiate(jumpEndEffect,this.transform.position, Quaternion.identity);
        ParticleSystem ps = effectInstance.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            float totalDuration = main.duration + main.startLifetime.constantMax;
            Destroy(effectInstance, totalDuration);
        }

        isJumping = false;
        
        if (sniperAnim != null) sniperAnim.SetJumping(false);
        if (sniperAnim != null) sniperAnim.TriggerJumpDown();
        if (sniperAnim != null) sniperAnim.TriggerAim();
        if (currentCrate.IsDestroyed)
        {
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 50f, LayerMask.GetMask("Default")))
            {
                transform.position = hit.point;
            }
        }
    }

    private void UpdateCurrentCrate(Crates newCrate)
    {
        if (currentCrate != null)
            currentCrate.OnDestroyed -= HandleCrateDestroyed;

        currentCrate = newCrate;

        if (currentCrate != null)
            currentCrate.OnDestroyed += HandleCrateDestroyed;
    }

    private void HandleCrateDestroyed(Crates crate)
    {
        isStunned = true;
        stunTimer = 0f;
        timer = 0f;

        audioSource.Stop();
        targetLockPlayed = false;
        aimClipPlayed = false;
        
        if (sniperAnim != null) sniperAnim.SetDizzy(true);

        // Temporarily disable the crate's collider for the raycast passes through it and hits the real ground
        playerTelegraph.HideTelegraph();
        Collider crateCollider = crate.GetComponent<Collider>();
        if (crateCollider != null) crateCollider.enabled = false;

        // Sniper falls to the ground (use raycast to detect the ground)
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 50f, LayerMask.GetMask("Default")))
        {
            transform.position = hit.point;
        }
        else
        {
            transform.position = new Vector3(transform.position.x, 0, transform.position.z); // Default drop to Y=0
        }
    }

    private void OnDestroy()
    {
        if (currentCrate != null)
            currentCrate.OnDestroyed -= HandleCrateDestroyed;
    }
}
