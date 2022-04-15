using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonSpiderMovement : MonoBehaviour
{
    [SerializeField] private float spiderMovementSpeed;
    [SerializeField] private float predictionMultiplier;
    [SerializeField] private float predictionSmoothing = 20;
    [SerializeField] private Transform rayOriginsAndHints;

    private Vector3 input;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandlePlayerInput();
    }

    private void FixedUpdate()
    {
        MoveSpider();
    }

    private void MoveSpider()
    {
        rayOriginsAndHints.position = Vector3.Lerp(rayOriginsAndHints.position, transform.position + input * predictionMultiplier, predictionSmoothing * Time.deltaTime);

        transform.position += input * Time.fixedDeltaTime * spiderMovementSpeed;
    }

    private void HandlePlayerInput()
    {
        input = Vector3.zero;

        input += Input.GetAxis("Horizontal") * transform.right;
        input += Input.GetAxis("Vertical") * transform.forward;

        input.Normalize();
    }
}
