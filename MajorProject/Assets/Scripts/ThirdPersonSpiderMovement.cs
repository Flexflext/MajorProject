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

    private Vector3 originLocalStartPos;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<SpiderBodyRotationController>();
        Cursor.lockState = CursorLockMode.Locked;
        originLocalStartPos = rayOriginsAndHints.localPosition;
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
        rayOriginsAndHints.localPosition = Vector3.Lerp(rayOriginsAndHints.localPosition, originLocalStartPos + new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * predictionMultiplier, predictionSmoothing * Time.deltaTime);
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
