using UnityEngine;

public class MachineGunnerController : MonoBehaviour, IDamageable
{
    [Header("Animator")]
    [SerializeField] private MachineGunerAnimation machineGunerAnimatior;
    
    [Header("UI")]
    [SerializeField] private BossUI bossUI;
    [SerializeField] private BossHp bossHP;

    [Header("VFX")]
    [SerializeField] private ParticleSystem FireEffect;

    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;
    [SerializeField] private float deathDuration = 0.5f;

    [Header("Targeting")]
    [SerializeField] private PlayerController player;
    private PlayerTelegraph playerTelegraph;

    [Header("Bullet")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject bulletSpawnPoint;

    [Header("Movement")]
    [SerializeField] private float turnSpeed = 12f;

    [Header("Aim")]
    [SerializeField] private float range = 20f;
    [SerializeField] private float aimWindUp = 0.2f;
    [SerializeField] private float aimTelegraph = 0.2f;
    [SerializeField] private float aimOffsetA = 0f;
    [SerializeField] private float aimOffsetB = 0f;

    [Header("SFX")]
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private float shootVolume = 1f;
    [SerializeField] private float reloadVolume = 1f;
    [SerializeField] private AudioSource reloadSource;
    [SerializeField] private AudioClip reloadClip;
    

    [Header("Machine Gunner Settings")]
    [SerializeField] private int burstCount = 12;
    [SerializeField] private float burstFireRate = 0.1f;
    [SerializeField] private float reloadTime = 6.5f;
    [SerializeField] private float reloadHitTimerAddPercentage = 0.3f;
    [SerializeField] private float viewAngle = 60f;
    

    private float timer;
    private Transform enemy;

    private bool canShoot = true;
    private bool isFiring = false;
    private bool isReloading = false;
    private float reloadTimer = 0f;
    private bool isDead = false;

    private float bulletSpeed;

    private CharacterController playerController;

    public bool TakeDamage(GameObject other)
    {
        if (isDead) return true;
        if(other.layer == LayerMask.NameToLayer("Bullet")) return true;
        
        currentHealth--;
        bossHP.Hit(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return true;
        }
        else machineGunerAnimatior.TriggerHit();

        if (isReloading && other.layer == LayerMask.NameToLayer("Player"))
        {
            reloadTimer += reloadTime * Mathf.Clamp(reloadHitTimerAddPercentage,0,1);
        }
        return true;
    }

    private void Die()
    {
        isDead = true;
        StopAllCoroutines();
        machineGunerAnimatior.TriggerDie();
        bossUI.HideTelegraph();
        StartCoroutine(DisableAfterDeath());
    }

    private System.Collections.IEnumerator DisableAfterDeath()
    {
        yield return new WaitForSeconds(deathDuration);
        gameObject.SetActive(false);
    }

    void Start()
    {
        enemy = this.GetComponent<Transform>();
        playerTelegraph = player.GetComponentInChildren<PlayerTelegraph>();
        bulletSpeed = bulletPrefab.GetComponent<Bullet>().speed;
        playerController = player.GetComponent<CharacterController>();
        reloadSource.volume = reloadVolume; // Set volume for reloadSource
        reloadSource.clip = reloadClip;
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) {
            machineGunerAnimatior.TriggerDie();
            FireEffect.Stop();
            return;
        }
        if (player == null) return;

        Vector3 toPlayer = player.transform.position - enemy.position;
        Vector3 aimToPlayer = new Vector3(toPlayer.x, 0, toPlayer.z);

        if (!canShoot)
        {
            timer = 0;
            return;
        }

        if (isFiring)
        {
            RotateTowardsPlayer(aimToPlayer);
        }
        else if (!isReloading)
        {
            RotateTowardsPlayer(aimToPlayer);
            HandleAimAndFire(aimToPlayer);
        }
        else HandleReloadTimer();
    }

    private void RotateTowardsPlayer(Vector3 aimDirection)
    {
        if (aimDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(aimDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
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

    private void HandleAimAndFire(Vector3 aimDirection)
    {

        timer += Time.deltaTime;

        if (timer > aimWindUp)
        {
            TriggerFire();
        }
        else if(!isDead) machineGunerAnimatior.TriggerIdle();
    }

    private void TriggerFire()
    {
        timer = 0;
        StartCoroutine(FireBurstRoutine());
    }

    private System.Collections.IEnumerator FireBurstRoutine()
    {
        isFiring = true;

        FireEffect.Play();

        bossUI.ShowMachineGunFire();

        machineGunerAnimatior.TriggerFire();

        for (int i = 0; i < burstCount; i++)
        {
            if (player == null) break;
            
            Vector3 spawnPos = bulletSpawnPoint != null ? bulletSpawnPoint.transform.position : enemy.position;

            if (shootClip != null)
                AudioSource.PlayClipAtPoint(shootClip, transform.position, shootVolume);

            GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                Vector3 shootDirection = GetPredictedAimPosition() - spawnPos;
                //RandomOffset
                shootDirection += Random.Range(aimOffsetA, aimOffsetB) * Vector3.Cross(shootDirection, Vector3.up).normalized;
                shootDirection.Normalize();
                bullet.Initialize(shootDirection, this.gameObject.layer, LayerMask.NameToLayer("Player"));
            }
            
            float percentage = (float)(burstCount-i) / (float)burstCount;
            bossUI.SetCDFillPercentage(percentage);

            yield return new WaitForSeconds(burstFireRate);
        }

        isFiring = false;

        FireEffect.Stop();
        
        ReloadRoutine();
    }

    private void HandleReloadTimer()
    {
        reloadTimer += Time.deltaTime;
        bossUI.SetCDFillPercentage(reloadTimer / reloadTime);
        if(reloadTimer > reloadTime) {
            isReloading = false;
            reloadSource.Stop();
        }
    }

    private void ReloadRoutine()
    {
        isReloading = true;
        machineGunerAnimatior.TriggerReload();
        reloadSource.Play();
        reloadTimer = 0f;
        bossUI.ShowMachineGunReload();
    }
}
