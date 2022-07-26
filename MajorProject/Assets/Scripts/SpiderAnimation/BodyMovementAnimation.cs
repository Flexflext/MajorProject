using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyMovementAnimation : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float rotationMultiplier;

    [SerializeField] private float frequency; // Speed the System will Respond to a Change
    [SerializeField] private float damping;
    [SerializeField] private float systemResponse;

    private float k1;
    private float k2;
    private float k3;

    private Vector3 previousTargetPosition;
    private Vector3 currentPosition;
    private Vector3 velocity;
    private Vector3 localVelo;
    private Vector3 newPos;

    private void Awake()
    {
        Initialize();
    }

    private void OnValidate()
    {
        Initialize();
    }


    private void Update()
    {
        if (Time.timeScale < 1) return;

        localVelo = transform.InverseTransformDirection(velocity);
        localVelo.Set(localVelo.z * rotationMultiplier, 0, -localVelo.x * rotationMultiplier);

        if (float.IsNaN(localVelo.x) || float.IsNaN(localVelo.y) || float.IsNaN(localVelo.z))
        {
            return;
        }

        transform.localEulerAngles = localVelo;

        newPos = GetAnimatedPosition(Time.deltaTime, target.position, null);
        transform.position = newPos;
        transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);

    }


    private void Initialize()
    {
        k1 = damping / (Mathf.PI * frequency);
        k2 = 1 / ((2* Mathf.PI *frequency) * (2* Mathf.PI * frequency));
        k3 = systemResponse * damping / (2 / Mathf.PI * frequency);

        previousTargetPosition = transform.position;
        currentPosition = transform.position;
        velocity = Vector3.zero;
    }

    //--> Semi Implicity Euler Method
    private Vector3 GetAnimatedPosition(float _deltatime, Vector3 _inputtargetposition, Vector3? _inputvelocity = null)
    {
        if (_inputvelocity == null) // Estimate the Input Velocity -> Averrage Velo since previous Sample
        {
            _inputvelocity = ((_inputtargetposition - previousTargetPosition) / _deltatime).normalized;

            previousTargetPosition = _inputtargetposition;
        }

        float k2_stable = Mathf.Max(k2, _deltatime * _deltatime / 2 + _deltatime * k1 / 2, _deltatime * k1);
        currentPosition = currentPosition + _deltatime * velocity; // Update Current Position with velocity mult by timestep


        

        velocity = velocity + _deltatime * (_inputtargetposition + k3 * (Vector3)_inputvelocity - currentPosition - k1 * velocity) / k2_stable; // Velocity  =  velocity + accelearation
        return currentPosition;
    }

    
}
