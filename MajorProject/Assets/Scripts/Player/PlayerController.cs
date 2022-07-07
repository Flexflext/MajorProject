using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private float curJumpCoyoteTimer;

    private bool isControllingPlayer = true;
    private Vector3 input;
    private Rigidbody rb;
    private PlayerCameraController playerCameraController;
    private float currentSpeed;
    private float shootdelay = 0.6f;
    private float curshootdelay = 0;

    private bool isGrounded = true;

    private Animator weaponAnimator;

    private void Awake()
    {
        weaponAnimator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        playerCameraController = GetComponent<PlayerCameraController>();
        rb.maxDepenetrationVelocity = maxSpeed;
    }

    private void Start()
    {
        LevelManager.Instance.PlayerSubscribe(this);
    }

    private void Update()
    {
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
            rb.velocity -= (1/ currentSpeed * rb.velocity) * Time.deltaTime * deceleration;
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
        weaponAnimator.SetTrigger("Shoot");
        playerCameraController.ShootShake();
        playerCameraController.AddRecoil(0, -2.5f);
        curshootdelay = shootdelay;

        if (Physics.Raycast(shootTransform.position, shootTransform.forward, out RaycastHit hit, float.MaxValue, hittableLayers))
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
        }
    }

    public Transform GetCamParent()
    {
        return playerCameraController.GetCamPosition();
    }

    public void SetPlayerControlling(bool _tosetto)
    {
        isControllingPlayer = _tosetto;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position - Vector3.down * groundCheckOffset, 0.2f);
    }
}
