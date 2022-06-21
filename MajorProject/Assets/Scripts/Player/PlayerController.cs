using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float playerSpeed;

    private bool isControllingPlayer = true;
    private Vector3 input;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!isControllingPlayer) return;

        GetPlayerInput();
    }

    private void FixedUpdate()
    {
        if (!isControllingPlayer) return;

        MovePlayer();
    }

    private void GetPlayerInput()
    {
        input = Vector3.zero;

        input += transform.right * Input.GetAxisRaw("Horizontal");
        input += transform.forward * Input.GetAxisRaw("Vertical");

        input.Normalize();

        input *= (playerSpeed) * Time.deltaTime;
    }

    private void MovePlayer()
    {
        rb.MovePosition(transform.position + input);
    }
}
