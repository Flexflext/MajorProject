using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Basic Player Controller with Running, Walking, Jumping and Shooting
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float playerSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCoyoteTimer;
    [SerializeField] private float deceleration = 3f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckOffset = 1f;
    [SerializeField] private float dmg = 5f;
    [SerializeField] private LayerMask hittableLayers;
    [SerializeField] private Transform shootTransform;
    [SerializeField] private Transform shotVFX;
    [SerializeField] private Transform impactVFX;
    [SerializeField] private Transform[] shotVFXPos;
    [SerializeField] private VisualEffect muzzleFlash;
    [SerializeField] private GameObject fpsGun;
    [SerializeField] private GameObject thirdPersonPlayer;
    private float curJumpCoyoteTimer;
    private float timeBetweenWalkSounds = 0.1f;

    private bool isControllingPlayer = true;
    private Vector3 input;
    private Rigidbody rb;
    private PlayerCameraController playerCameraController;
    private float currentSpeed;
    private float shootdelay = 0.6f;
    private float curshootdelay = 0;

    private bool isGrounded = true;

    [SerializeField] private Animator weaponAnimator;
    private PersonelAudioManager audioManager;

    private void Awake()
    {
        //weaponAnimator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        playerCameraController = GetComponent<PlayerCameraController>();
        audioManager = GetComponent<PersonelAudioManager>();
        rb.maxDepenetrationVelocity = maxSpeed;
    }

    private void Start()
    {
        LevelManager.Instance.PlayerSubscribe(this);
    }

    private void Update()
    {
        if (Time.timeScale < 1) return;
        if (!isControllingPlayer) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if (curshootdelay <= 0)
	    {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Shoot();
            }
        }
        else
        {
            curshootdelay -= Time.deltaTime;
        }

        

        GroundCheck();
        GetPlayerInput();

        if (isGrounded)
        {
            playerCameraController.HeadBobbing(currentSpeed);
        }
        else
        {
            playerCameraController.HeadBobbing(0);
        }

        MovePlayer();
    }

    private void GetPlayerInput()
    {
        input = Vector3.zero;

        input += transform.right * Input.GetAxisRaw("Horizontal");
        input += transform.forward * Input.GetAxisRaw("Vertical");

        if (input == Vector3.zero && isGrounded)
        {
            rb.velocity -= (rb.velocity) * Time.deltaTime * deceleration;
        }

        input.Normalize();

        input *= (playerSpeed);
    }

    private void MovePlayer()
    {
        if (curJumpCoyoteTimer <= 0 && !isGrounded)
        {
            return;
        }
        else if (!isGrounded)
        {
            curJumpCoyoteTimer -= Time.deltaTime;
        }

        
        

        rb.velocity += input * Time.deltaTime;

        //if (!isGrounded) return;
        currentSpeed = rb.velocity.magnitude;

        if (currentSpeed > maxSpeed)
        {
            rb.velocity -= (rb.velocity) * Time.deltaTime * deceleration;
        }

        if (currentSpeed <= 0.2f)
        {
            return;
        }

        if (timeBetweenWalkSounds <= 0)
        {
            audioManager.Play(EPossibleSounds.Walk, ERandomSound.Random, true);
            timeBetweenWalkSounds = 0.35f;
        }
        else
        {
            timeBetweenWalkSounds -= Time.deltaTime;
        }
    }

    private void Jump()
    {
        if (!isGrounded) return;

        isGrounded = false;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        curJumpCoyoteTimer = jumpCoyoteTimer;
    }

    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.position - Vector3.down * groundCheckOffset, 0.2f, groundLayer);
    }

    private void Shoot()
    {
        audioManager.Play(EPossibleSounds.Attack, ERandomSound.Static, true);
        weaponAnimator.SetTrigger("Shoot");
        muzzleFlash.Play();
        playerCameraController.ShootShake();
        playerCameraController.AddRecoil(0, -2.5f);
        curshootdelay = shootdelay;

        if (Physics.SphereCast(shootTransform.position,0.2f, shootTransform.forward, out RaycastHit hit, float.MaxValue, hittableLayers))
        {
            IDamageable damageObj = hit.collider.GetComponent<IDamageable>();

            if (damageObj == null)
            {
                damageObj = hit.collider.GetComponentInParent<IDamageable>();
            }

            if (damageObj != null)
            {
                damageObj.TakeDamage(dmg, transform.forward);

                HUD.Instance.HitObjAnim();
            }

            impactVFX.gameObject.SetActive(true);
            impactVFX.position = hit.point;
            Vector3 fromTo = shotVFX.transform.position - hit.point;

            shotVFX.gameObject.SetActive(true);
            shotVFXPos[0].position = shotVFX.position;
            shotVFXPos[1].position = shotVFX.transform.position + fromTo * (1 / 3);
            shotVFXPos[2].position = shotVFX.transform.position + fromTo * (2 / 3);
            shotVFXPos[3].position = hit.point;

            StartCoroutine(C_WaitForShotEnd());         
        }
    }

    private IEnumerator C_WaitForShotEnd()
    {
        yield return new WaitForSeconds(0.2f);
        shotVFX.gameObject.SetActive(false);
        impactVFX.gameObject.SetActive(false);
    }

    public Transform GetCamParent()
    {
        return playerCameraController.GetCamPosition();
    }

    public void SetPlayerControlling(bool _tosetto)
    {
        audioManager.Play(EPossibleSounds.Foley, ERandomSound.Static, true);
        isControllingPlayer = _tosetto;
        rb.velocity = Vector3.zero;
        playerCameraController.SetPlayerControll(_tosetto);

        if (!_tosetto)
        {
            fpsGun.SetActive(false);
            thirdPersonPlayer.SetActive(true);
        }
        else
        {
            fpsGun.SetActive(true);
            thirdPersonPlayer.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position - Vector3.down * groundCheckOffset, 0.2f);
    }
}
