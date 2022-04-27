using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpiderBodyRotationController))]
public class ThirdPersonSpiderMovement : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 20.0f;

    [SerializeField] private float spiderMovementSpeed;
    [SerializeField] private float predictionMultiplier;
    [SerializeField] private float predictionSmoothing = 20;
    [SerializeField] private Transform rayOriginsAndHints;

    private SpiderBodyRotationController controller;

    private float mouseInput;
    private Vector3 input;

    private bool DebugUseCameraMovement = true;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<SpiderBodyRotationController>();
        Cursor.lockState = CursorLockMode.Locked;
        
    }

    // Update is called once per frame
    void Update()
    {
        HandlePlayerInput();
        if (DebugUseCameraMovement) RotateSpider();
        MoveSpider();

        if (Input.GetKeyDown(KeyCode.M))
        {
            DebugUseCameraMovement = !DebugUseCameraMovement;
        }
    }

    private void RotateSpider()
    {
        controller.SetPlayerInputRotation(mouseInput * rotationSpeed);
    }

    private void MoveSpider()
    {
        rayOriginsAndHints.position = Vector3.Lerp(rayOriginsAndHints.position, transform.position + input * predictionMultiplier, predictionSmoothing * Time.deltaTime);
        controller.SetPlayerMovementInput(input * spiderMovementSpeed);
    }

    private void HandlePlayerInput()
    {
        input = Vector3.zero;

        input += Input.GetAxis("Horizontal") * transform.right;
        input += Input.GetAxis("Vertical") * transform.forward;

        mouseInput = Input.GetAxis("Mouse X");

        input.Normalize();
    }
}
