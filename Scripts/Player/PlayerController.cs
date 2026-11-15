using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Player UI")]
    [SerializeField] private MainUI mainUI;

    [Header("Slash Effect")]
    [SerializeField] private ParticleSystem slashEffect1;
    [SerializeField] private ParticleSystem slashEffect2;
    [SerializeField] private ParticleSystem slashEffect3;

    [Header("Player Camera")]
    [SerializeField] private Transform playercam;

    [Header("Movement")]
    [SerializeField] private float turnSpeed = 12f;
    [SerializeField] private float speed = 0;
    private float currentSpeed;
    [SerializeField] private float slowmotionspeed = 0;
    [SerializeField] private bool horizontalMovementOnly = false;
    private float horizontalMovePosition;

    [Header("Slow Motion")]
    [SerializeField] private float slowMotionFactor = 0.05f;
    [SerializeField] private float maxSlowMotionEnergy = 5f;      
    [SerializeField] private float slowMotionRecoverRate = 1f;
    private float currentSlowMotionEnergy;
    private bool isSlowMotionActive = false;

    private bool inputBuffer = false;
    private int currentComboStep = 1;
    private bool canReceiveInput = false;


    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private int maxDashCharges = 2;
    private int currentDashCharges;
    private float dashRechargeTimer = 0f;

    [Header("Auto Aim")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Cheat")]
    [SerializeField] private bool GodMode = false;


    //Player Attack
    private bool isAttacking = false;
    private float nextAttackTime = 0.0f;
    private bool CanAttack => Time.time >= nextAttackTime;

    private Rigidbody rb;
    private Vector3 movement;
    private Transform playerTransform;
    private CharacterController cc;
    private PlayerAnimator playerAnimator;
    private PlayerAudioController audioCtrl;
    private float movementspeed;


    private bool isDashing;
    private float dashEndTime;
    private bool Invincible = false;
    public bool levelPass = false;

    private bool hasThrowable = false;

    private Katana katana;
    private PlayerPickupRange playerPickupRange;
    private Outline nearestOutline;

    private HashSet<GameObject> Throwables;
    private GameObject nearest = null;
    private GameObject objectPickedup = null;

    private TrailRenderer tr;

    private bool isDead = false;

    public static PlayerController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        playerTransform = this.transform;
        movementspeed = speed;
        playerAnimator = this.GetComponentInChildren<PlayerAnimator>();
        rb = this.GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
        katana = GetComponentInChildren<Katana>();
        playerPickupRange = GetComponentInChildren<PlayerPickupRange>();
        tr = GetComponent<TrailRenderer>();

        playerPickupRange.OnEnter += OnThrowableEnterRange;
        playerPickupRange.OnExit += OnThrowableExitRange;
        Throwables = new HashSet<GameObject>();
        audioCtrl = GetComponent<PlayerAudioController>();
        currentDashCharges = maxDashCharges;
        currentSlowMotionEnergy = maxSlowMotionEnergy;
        playerAnimator.SetAnimSpeed(1);

        if (horizontalMovementOnly) horizontalMovePosition = this.transform.position.z;
    }

    public bool getDead()
    {
        return isDead;
    }

    public bool TakeDamage(GameObject other)
    {
        if (!GodMode)
        {
            if (!Invincible && !levelPass)
            {
                this.gameObject.SetActive(false);
                GameManager.Instance.CancelPause();
                this.isDead = true;
                return true;
            }
            return false;
        }
        return true;
    }
    
    public void Move(InputValue movementValue)
    {
        if (isDead) return;
        Vector2 movementVector = movementValue.Get<Vector2>();

        if (horizontalMovementOnly) movementVector.y = 0f;

        //Movement Base on PlayerCamera direction
        Vector3 camForward = playercam.forward;
        Vector3 camRight = playercam.right;

        //Ignore Y axis
        camForward.y = 0f;
        camRight.y = 0f;

        //Normalized Vectors
        camForward.Normalize();
        camRight.Normalize();

        movement = camRight * movementVector.x + camForward * movementVector.y;
    }

    public void SlowMotion(InputValue inputValue)
    {
        if (isDead) return;
        if (inputValue.isPressed && currentSlowMotionEnergy > 0)
        {
            isSlowMotionActive = true;
            Time.timeScale = slowMotionFactor;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
            movementspeed = slowmotionspeed;
            playerAnimator.SetAnimSpeed(slowmotionspeed/speed);
            if (audioCtrl != null) audioCtrl.PlaySlowMotion();
        }
        else
        {
            isSlowMotionActive = false;
            Time.timeScale = 1f;
            movementspeed = speed;
            playerAnimator.SetAnimSpeed(1);
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
            if (audioCtrl != null) audioCtrl.StopSlowMotion();
        }
    }

    private void UpdateSlowMotionState()
    {
        if (isSlowMotionActive)
        {
            currentSlowMotionEnergy -= Time.unscaledDeltaTime;
            if (currentSlowMotionEnergy <= 0)
            {
                isSlowMotionActive = false;
                currentSlowMotionEnergy = 0;
                Time.timeScale = 1f;
                movementspeed = speed;
                playerAnimator.SetAnimSpeed(1);
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
                if (audioCtrl != null) audioCtrl.StopSlowMotion();
            }
        }
        else
        {
            currentSlowMotionEnergy = Mathf.Min(currentSlowMotionEnergy + Time.deltaTime * slowMotionRecoverRate, maxSlowMotionEnergy);
        }

        if (mainUI != null)
        {
            mainUI.updateSlowMotionUI(currentSlowMotionEnergy / maxSlowMotionEnergy);
        }
    }

    public void HandlePaused()
    {
        isSlowMotionActive = false;
        movementspeed = speed;
        playerAnimator.SetAnimSpeed(1);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        if (audioCtrl != null) audioCtrl.StopSlowMotion();
        movement = Vector3.zero;
    }


    public void Attack(InputValue inputValue)
    {
        //Press Parry
        if (isDead) return;

        if (CanAttack)
        {
            if (inputValue.isPressed)
            {
                
                if (!isAttacking)
                {

                    SetIsAttacking(true);
                    switch (currentComboStep)
                    {
                        case 1:
                            playerAnimator.TriggerAttack1();
                            break;
                        case 2:
                            playerAnimator.TriggerAttack2();
                            break;
                        case 3:
                            playerAnimator.TriggerAttack3();
                            break;
                    }
                }
                else if(canReceiveInput) this.inputBuffer = true;
            }
        }
    }

    public void CheckComboNext()
    {
        currentComboStep++;
        SetIsAttacking(false);
        if (inputBuffer)
        {
            if (currentComboStep > 3)
            {
                EndCombo();
                return;
            }
            inputBuffer = false;
            canReceiveInput = false;
            SetIsAttacking(true);
            switch (currentComboStep)
            {
                case 1:
                    playerAnimator.TriggerAttack1();
                    break;
                case 2:
                    playerAnimator.TriggerAttack2();
                    break;
                case 3:
                    playerAnimator.TriggerAttack3();
                    break;
            }
        }
    }

    private void SetIsAttacking(bool IsAttacking)
    {
        if(IsAttacking) {
            isAttacking = true;
            katana.SetAttack(true);
            katana.OnHit += onKatanaHit;
            if (audioCtrl != null) audioCtrl.PlayAttack();
            switch (currentComboStep)
            {
                case 1:
                    if(slashEffect1 != null) slashEffect1.Play();
                    break;
                case 2:
                    if(slashEffect2 != null)slashEffect2.Play();
                    break;
                case 3:
                    if(slashEffect3 != null)slashEffect3.Play();
                    break;
            }
        }
        else
        {
            isAttacking = false;
            katana.SetAttack(false);
            katana.OnHit -= onKatanaHit;
        }
    }

    public void SetCanReceiveInput()
    {
        this.canReceiveInput = true;
    }

    public void EndCombo()
    {
        currentComboStep = 1;
        inputBuffer = false;
        SetIsAttacking(false);
        canReceiveInput = false;
        Debug.Log("End");
    }


    void onKatanaHit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            if (other.gameObject.TryGetComponent<Bullet>(out Bullet component))
            {
                component.Reverse();
            }
        }
        if (other.TryGetComponent<IDamageable>(out var dmg))
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                if (audioCtrl != null) audioCtrl.PlayHitEnemy();
                dmg.TakeDamage(this.gameObject);
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("EnemyShooter"))
            {
                if (audioCtrl != null) audioCtrl.PlayHitEnemy();
                dmg.TakeDamage(this.gameObject);
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Crates"))
            {
                if (audioCtrl != null) audioCtrl.PlayHitCrate();
                dmg.TakeDamage(this.gameObject);
            }
            else dmg.TakeDamage(this.gameObject);
        }
    }

    public void Dash(InputValue inputValue)
    {
        if (isDead) return;
        if (currentDashCharges > 0 && !isDashing)
        {
            isDashing = true;
            tr.emitting = true;
            playerAnimator.TriggerDash();
            currentDashCharges--;
            if (audioCtrl != null) audioCtrl.PlayDash();
            dashEndTime = Time.time + dashDuration;
        }
    }

    private void UpdateDashRecharge()
    {
        if (currentDashCharges < maxDashCharges)
        {
            dashRechargeTimer += Time.deltaTime;
            if (dashRechargeTimer >= dashCooldown)
            {
                currentDashCharges++;
                dashRechargeTimer = 0f;
            }
        }
        else
        {
            dashRechargeTimer = 0f;
        }

        if (mainUI != null)
        {
            float totalProgress = currentDashCharges + (dashRechargeTimer / dashCooldown);
            mainUI.updateDashUI(totalProgress);
        }
    }

    /// <summary>
    /// Find the nearest Enemy in sector.
    /// </summary>
    /// <returns>Returns the vector toward the nearest enemy in sector, return the player's forward vector if none enemy detected,</returns>
    private Vector3 GetNearestEnemyInSector()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        Transform nearestTarget = null;
        float minDistanceSqr = Mathf.Infinity;

        foreach (Collider target in targets)
        {
            Transform targetTransform = target.transform;

            Vector3 dirToTarget = targetTransform.position - transform.position;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float dstToTargetSqr = dirToTarget.sqrMagnitude;

                if (dstToTargetSqr < minDistanceSqr)
                {
                    if (!Physics.Raycast(transform.position, dirToTarget.normalized, (target.transform.position - this.transform.position).magnitude , LayerMask.NameToLayer("Default")))
                    minDistanceSqr = dstToTargetSqr;
                    nearestTarget = targetTransform;
                }
            }
        }
        if(nearestTarget != null) {
            Vector3 nearestDir = nearestTarget.transform.position - this.transform.position;
            nearestDir.y = 0;
            return nearestDir;
        }
        return this.transform.forward;
    }

    public void Interact(InputValue inputValue)
    {
        if (isDead) return;
        bool pickup = false;
        if (inputValue.isPressed)
        {
            //Pickup Throwable
            if (!hasThrowable && nearest != null)
            {
                objectPickedup = nearest;
                Throwables.Remove(nearest);
                nearest = null;
                objectPickedup.TryGetComponent<Throwable>(out Throwable component);
                component.Pickup();
                pickup = true;
            }

            //Throw Throwable
            if (hasThrowable)
            {
                objectPickedup.TryGetComponent<Throwable>(out Throwable component);
                objectPickedup.transform.position = this.transform.position;
                Collider[] playerCollider = {this.GetComponent<CharacterController>(), this.GetComponent<Collider>(), playerPickupRange.GetComponent<Collider>()};
                component.Throw(GetNearestEnemyInSector(), playerCollider);
                pickup = false;
                hasThrowable = false;
            }
        }
        if (pickup) hasThrowable = true;
    }

    void OnThrowableEnterRange(Collider other)
    {
        if(other.gameObject.TryGetComponent<Throwable>(out Throwable component))
        {
            Throwables.Add(other.gameObject);
        }
        
    }

    void OnThrowableExitRange(Collider other)
    {
        if (Throwables.Contains(other.gameObject))
        {
            Throwables.Remove(other.gameObject);
            nearestOutline.enabled = false;
        }
    }

    /// <summary>
    /// Handles distance detection and outline highlighting for pickable throwables.
    /// </summary>
    private void UpdateNearestThrowable()
    {
        if (Throwables.Count == 0)
        {
            nearest = null;
            nearestOutline = null;
        }
        else
        {
            foreach (GameObject i in Throwables)
            {
                if (nearest == null) nearest = i;
                else if (Mathf.Abs((i.transform.position - this.transform.position).sqrMagnitude) < Mathf.Abs((nearest.transform.position - this.transform.position).sqrMagnitude))
                {
                    nearest = i;
                }
            }
        }

        if (nearestOutline != null) nearestOutline.enabled = false;

        if (nearest != null && !hasThrowable)
        {
            nearest.gameObject.TryGetComponent<Outline>(out Outline component);
            nearestOutline = component;
            if(SceneManager.GetActiveScene().name == "Level3_002") nearestOutline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
            nearestOutline.enabled = true;
        }
    }


    /// <summary>
    /// Handles character rotation to face the movement direction.
    /// </summary>
    private void HandleRotation()
    {
        if (movement.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }
    }


    /// <summary>
    /// Handles player dash state, invincibility frames, and ignoring collision layers.
    /// </summary>
    private void UpdateDashState()
    {
        if (isDashing)
        {
            if (Time.time >= dashEndTime)
            {
                isDashing = false;
                playerAnimator.TriggerIdle();
                Physics.IgnoreLayerCollision(this.gameObject.layer, LayerMask.NameToLayer("Enemy"), false);
                Physics.IgnoreLayerCollision(this.gameObject.layer, LayerMask.NameToLayer("EnemyShooter"), false);
                Physics.IgnoreLayerCollision(this.gameObject.layer, LayerMask.NameToLayer("Bullet"), false);
                Invincible = false;
            }
            else
            {
                Physics.IgnoreLayerCollision(this.gameObject.layer, LayerMask.NameToLayer("Enemy"), true);
                Physics.IgnoreLayerCollision(this.gameObject.layer, LayerMask.NameToLayer("EnemyShooter"), true);
                Physics.IgnoreLayerCollision(this.gameObject.layer, LayerMask.NameToLayer("Bullet"), true);
                currentSpeed = dashSpeed;
                Invincible = true;
            }
        }
    }

    public void CancelDash()
    {
        dashEndTime = Time.time;
    }

    private void UpdateTrailEmit(float speed)
    {
        if(speed <= movementspeed) tr.emitting = false;
    }

    /// <summary>
    /// Executes the final physical movement and triggers footstep audio.
    /// </summary>
    private void ExecuteMovement(float finalSpeed)
    {
        if(isDashing) cc.Move(transform.forward * finalSpeed * Time.deltaTime);
        else cc.Move(movement * finalSpeed * Time.deltaTime);
        playerAnimator.UpdateMovementAnimation(Math.Clamp(movement.magnitude,0,1));
        if (horizontalMovementOnly)
        {
            var position = this.transform.position;
            position.z = horizontalMovePosition;
            this.transform.position = position;
        }
        if (audioCtrl != null) audioCtrl.TickFootsteps(movement.sqrMagnitude > 0.01f);
    }

    void Update()
    {
        // Keep your original way of getting the Transform
        playerTransform = this.GetComponent<Transform>();

        HandleRotation();
        UpdateNearestThrowable();

        UpdateSlowMotionState();
        if(SceneVolume.Instance != null) SceneVolume.Instance.SetSlowMotion(isSlowMotionActive);
        UpdateDashRecharge();

        // Handle dash speed and state
        currentSpeed = movementspeed;
        UpdateDashState();

        UpdateTrailEmit(currentSpeed);
        ExecuteMovement(currentSpeed);
        // UpdateAttackState();
    }
    
    public Vector3 GetNonDashVelocity()
    {
        return movement * movementspeed;
    }
}
