using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// Movement Class for the Spider to Handle Input
/// </summary>
[RequireComponent(typeof(SpiderBodyRotationController))]
public class ThirdPersonSpiderMovement : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 20.0f;

    [SerializeField] private float spiderMovementSpeed;
    [SerializeField] private float camSens;
    [SerializeField] private float predictionMultiplier;
    [SerializeField] private float predictionSmoothing = 20;
    [SerializeField] private Transform rayOriginsAndHints;
    [SerializeField] private Transform camFollowTransform;


    private NavMeshAgent agent;

    private SpiderBodyRotationController controller;

    private float mouseInput;
    private Vector3 input;

    private bool useCameraMovement = true;
    private bool useMovement = true;

    private bool iscontrolled = true;
    private float xRotation;


    private Vector3 originLocalStartPos;

    // Start is called before the first frame update
    void Awake()
    {
        agent= GetComponent<NavMeshAgent>();
        controller = GetComponent<SpiderBodyRotationController>();
        originLocalStartPos = rayOriginsAndHints.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale < 1)
        {
            return;
        }

        if (!iscontrolled)
        {
            if (agent)
            {
                Vector3 dir = transform.InverseTransformDirection(agent.velocity);
                dir.Normalize();

                rayOriginsAndHints.localPosition = Vector3.Lerp(rayOriginsAndHints.localPosition, originLocalStartPos + new Vector3(dir.x, 0, dir.z) * predictionMultiplier, predictionSmoothing * Time.deltaTime);
            }

            controller.SetPlayerMovementInput(Vector2.zero);
            return;
        }

        HandlePlayerInput();
        RotateSpider();
        MoveSpider();

        if (Input.GetKeyDown(KeyCode.M))
        {
            useCameraMovement = !useCameraMovement;
        }
    }

    /// <summary>
    /// Stop User Input
    /// </summary>
    public void StopUserInput()
    {
        useMovement = false;
        useCameraMovement = false;
    }

    /// <summary>
    /// Stop Player Controller
    /// </summary>
    public void SetPlayerStopControll()
    {
        //rayOriginsAndHints.localPosition = new Vector3(0, 0, 0.5f) + originLocalStartPos;
        iscontrolled = false;
    }

    /// <summary>
    /// Start Player Controller
    /// </summary>
    public void SetPlayerStartControll()
    {
        //rayOriginsAndHints.localPosition = originLocalStartPos + new Vector3(0, 0, 0.5f);
        iscontrolled = true;
    }

    /// <summary>
    /// Start User Input to Controller
    /// </summary>
    public void StartUserInput()
    {
        useMovement = true;
        useCameraMovement = true;
    }

    /// <summary>
    /// Rotate Spider Controller
    /// </summary>
    private void RotateSpider()
    {
        if (!useCameraMovement)
        {
            controller.SetPlayerInputRotation(0);
            return;
        }

        controller.SetPlayerInputRotation(mouseInput * rotationSpeed);
    }

    /// <summary>
    /// Move Spider Controller
    /// </summary>
    private void MoveSpider()
    {
        if (!useMovement || !iscontrolled)
        {
            return;
        }

        rayOriginsAndHints.localPosition = Vector3.Lerp(rayOriginsAndHints.localPosition, originLocalStartPos + new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * predictionMultiplier, predictionSmoothing * Time.deltaTime);
        controller.SetPlayerMovementInput(input * spiderMovementSpeed);
    }

    /// <summary>
    /// Handle Player Movement Input
    /// </summary>
    private void HandlePlayerInput()
    {
        input = Vector3.zero;

        input += Input.GetAxis("Horizontal") * transform.right;
        input += Input.GetAxis("Vertical") * transform.forward;

        HandleCamControll();

        input.Normalize();
    }

    /// <summary>
    /// Handle Cameara Input
    /// </summary>
    private void HandleCamControll()
    {
        xRotation += Input.GetAxis("Mouse Y") * camSens * Time.deltaTime;
        mouseInput = Input.GetAxis("Mouse X");
        camFollowTransform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }

    private void OnDisable()
    {
        rayOriginsAndHints.localPosition = originLocalStartPos;
    }
}
