using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spider Animation Lerping Script for a more Mechanical Correct Lerping
/// </summary>
public class BodyMovementAnimation : MonoBehaviour
{
    [Tooltip("Target to Lerp to in Update")]
    [SerializeField] private Transform target;
    [Tooltip("Multiplier on the Angle the Body Rotates to")]
    [SerializeField] private float rotationMultiplier;

    [Tooltip("Speed the System will Respond to a Change")]
    [SerializeField] private float frequency;
    [Tooltip("Speed in wich the Vibration of the Frequency stops")]
    [SerializeField] private float damping;
    [Tooltip("Speed in wich the System responds to changes in the Motion")]
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        Initialize();
    }
#endif


    private void Update()
    {
        if (Time.timeScale < 1) return;

        AnimateBody();
    }

    /// <summary>
    /// Animates the Body Position and Rotation
    /// </summary>
    private void AnimateBody()
    {
        AnimateRotation();

        AnimatePosition();
    }

    /// <summary>
    /// Animates the Position of this Object to the Position the System Calculated In LocalSpace
    /// </summary>
    private void AnimatePosition()
    {
        newPos = GetAnimatedPosition(Time.deltaTime, target.position, null);
        transform.InverseTransformVector(newPos);
        transform.localPosition = new Vector3(newPos.x, 0, newPos.z);
    }

    /// <summary>
    /// Animates the Rotation of this Object based of the current local velocity
    /// </summary>
    private void AnimateRotation()
    {
        localVelo = transform.InverseTransformDirection(velocity);
        localVelo.Set(localVelo.z * rotationMultiplier, 0, -localVelo.x * rotationMultiplier);

        if (float.IsNaN(localVelo.x) || float.IsNaN(localVelo.y) || float.IsNaN(localVelo.z))
        {
            return;
        }

        transform.localEulerAngles = localVelo;
    }

    /// <summary>
    /// Initializes the Koefficients k1, k2, k3 and the Initial Positions
    /// </summary>
    private void Initialize()
    {
        k1 = damping / (Mathf.PI * frequency);
        k2 = 1 / ((2* Mathf.PI *frequency) * (2* Mathf.PI * frequency));
        k3 = systemResponse * damping / (2 / Mathf.PI * frequency);

        previousTargetPosition = transform.position;
        currentPosition = transform.position;
        velocity = Vector3.zero;
    }


    /// <summary>
    /// Animate a given Bodyposition for a new Timestep with the Semi Implicit Euler Method
    /// </summary>
    /// <param name="_deltatime"></param>
    /// <param name="_inputtargetposition"></param>
    /// <param name="_inputvelocity"></param>
    /// <returns>New Position Based on the Lerping-System</returns>
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
