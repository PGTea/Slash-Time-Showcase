using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Throwable : MonoBehaviour
{
    [Header("Throwing")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Audio")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip throwClip;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private float throwVolume = 1f;
    [SerializeField] private float hitVolume = 1f;

    private Rigidbody rb;
    private Vector3 direction;
    private int senderlayer;

    private bool hasHit = false;
    private bool isPickedUp = false;
    private bool isThrowed = false;
    private new Collider collider;

    private Outline outline;

    private Vector3 randomRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        outline = gameObject.GetComponent<Outline>();
        outline.enabled = false;
        collider = this.gameObject.GetComponent<Collider>();
        if (source == null) source = GetComponent<AudioSource>();
    }

    public void Pickup()
    {
        isPickedUp = true;
        this.gameObject.SetActive(false);
        this.outline.enabled = false;
    }


    public void Throw(Vector3 direction, Collider[] playerCollider)
    {
        if (isPickedUp) 
        {
            isThrowed = true;
            foreach (Collider playercollider in playerCollider)
            {
                Physics.IgnoreCollision(this.collider, playercollider, true);
            }

            this.gameObject.SetActive(true);
            if (source != null && throwClip != null)
                source.PlayOneShot(throwClip, throwVolume);


            this.direction = direction.normalized;
            this.transform.forward = this.direction;
            
            collider.isTrigger = true;
        }
    }

    void OnTriggerStay(Collider other)
    {

        if (isThrowed)
        {
            if (hasHit) { return; }
            hasHit = true;
            if (source != null && hitClip != null)
                source.PlayOneShot(hitClip, hitVolume);


            if (other.TryGetComponent<IDamageable>(out var dmg))
            {
                dmg.TakeDamage(this.gameObject);
            }

            Destroy(gameObject, 0.1f);

        }
    }

    void Update()
    {
        if (isThrowed)
        {
            rb.linearVelocity = this.direction * speed;

            Vector3 rotationDirection = this.transform.right.normalized;
            rb.angularVelocity = rotationDirection * rotationSpeed;
        }

    }


}
