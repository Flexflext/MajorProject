using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(SpiderBodyRotationController))]
public class ThirdPersonSpiderMovement : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 20.0f;

    [SerializeField] private float spiderMovementSpeed;
    [SerializeField] private float predictionMultiplier;
    [SerializeField] private float predictionSmoothing = 20;
    [SerializeField] private Transform rayOriginsAndHints;


    private NavMeshAgent agent;

    private SpiderBodyRotationController controller;

    private float mouseInput;
    private Vector3 input;

    private bool useCameraMovement = true;
    private bool useMovement = true;

    private bool iscontrolled = true;


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
        if (!iscontrolled)
        {
            Vector3 dir = transform.InverseTransformDirection(agent.velocity);
            dir.Normalize();

            rayOriginsAndHints.localPosition = Vector3.Lerp(rayOriginsAndHints.localPosition, originLocalStartPos + new Vector3(dir.x, 0, dir.z) * predictionMultiplier, predictionSmoothing * Time.deltaTime);

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

    public void StopUserInput()
    {
        useMovement = false;
        useCameraMovement = false;
    }

    public void SetPlayerStopControll()
    {
        //rayOriginsAndHints.localPosition = new Vector3(0, 0, 0.5f) + originLocalStartPos;
        iscontrolled = false;
    }

    public void SetPlayerStartControll()
    {
        //rayOriginsAndHints.localPosition = originLocalStartPos + new Vector3(0, 0, 0.5f);
        iscontrolled = true;
    }

    public void StartUserInput()
    {
        useMovement = true;
        useCameraMovement = true;
    }

    private void RotateSpider()
    {
        if (!useCameraMovement)
        {
            controller.SetPlayerInputRotation(0);
            return;
        }

        controller.SetPlayerInputRotation(mouseInput * rotationSpeed);
    }

    private void MoveSpider()
    {
        if (!useMovement)
        {
            controller.SetPlayerMovementInput(Vector2.zero);
            return;
        }

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

    private void OnDisable()
    {
        rayOriginsAndHints.localPosition = originLocalStartPos;
    }
}
