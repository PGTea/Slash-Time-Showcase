using UnityEngine;

public class PlayerAudioController : MonoBehaviour
{
    [SerializeField] private AudioSource source;

    [Header("Footsteps")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float stepInterval = 0.45f;
    [SerializeField] private float footstepVolume = 1f;

    [Header("Attack (Multiple)")]
    [SerializeField] private AudioClip[] attackClips;
    [SerializeField] private float attackVolume = 1f;

    [Header("Slow Motion (Single)")]
    [SerializeField] private AudioClip slowMotionClip;
    [SerializeField] private float slowMotionVolume = 1f;

    [Header("Dash")]
    [SerializeField] private AudioClip dashClip;
    [SerializeField] private float dashVolume = 1f;

    [Header("Hit Crates (Multiple)")]
    [SerializeField] private AudioClip[] hitCrateClips;
    [SerializeField] private float hitCrateVolume = 1f;

    [Header("Hit Enemies (Multiple)")]
    [SerializeField] private AudioClip[] hitEnemyClips;
    [SerializeField] private float hitEnemyVolume = 1f;

    [Header("Hit Boss")]
    [SerializeField] private AudioClip bossHpHitClip;
    [SerializeField] private float hitBossVolume = 1f;

    private float stepTimer;
    private AudioSource slowMoSource;

    private void Awake()
    {
        slowMoSource = gameObject.AddComponent<AudioSource>();
        slowMoSource.playOnAwake = false;
        slowMoSource.spatialBlend = source.spatialBlend;
    }

    public void TickFootsteps(bool isMoving)
    {
        if (!isMoving || footstepClips == null || footstepClips.Length == 0)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            int i = Random.Range(0, footstepClips.Length);
            source.PlayOneShot(footstepClips[i], footstepVolume);
            stepTimer = stepInterval;
        }
    }

    public void PlayAttack()
    {
        if (attackClips == null || attackClips.Length == 0)
            return;

        int i = Random.Range(0, attackClips.Length);
        source.PlayOneShot(attackClips[i], attackVolume);
    }

    public void PlaySlowMotion()
    {
        if (slowMotionClip != null)
        {
            slowMoSource.clip = slowMotionClip;
            slowMoSource.volume = slowMotionVolume;
            slowMoSource.Play();
        }
    }

    public void StopSlowMotion()
    {
        if (slowMoSource != null && slowMoSource.isPlaying)
        {
            slowMoSource.Stop();
        }
    }

    public void PlayHitCrate()
    {
        if (hitCrateClips == null || hitCrateClips.Length == 0) return;
        
        int i = Random.Range(0, hitCrateClips.Length);
        // source.PlayOneShot(hitCrateClips[i], hitCrateVolume);
    }

    public void PlayHitEnemy()
    {
        if (hitEnemyClips == null || hitEnemyClips.Length == 0) return;
        
        int i = Random.Range(0, hitEnemyClips.Length);
        source.PlayOneShot(hitEnemyClips[i], hitEnemyVolume);
    }
    public void PlayDash()
    {
        if (dashClip != null)
            source.PlayOneShot(dashClip, dashVolume);
    }
    public void PlayBossHpHit()
    {
        if (bossHpHitClip != null)
            source.PlayOneShot(bossHpHitClip, hitBossVolume);
    }


    private void Reset()
    {
        source = GetComponent<AudioSource>();
    }
}
