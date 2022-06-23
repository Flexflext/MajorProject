using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private float camSensitivity = 100.0f;
    [SerializeField] private float swaySpeed = 20;

    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCam;
    [SerializeField] private Transform camPosition;
    [SerializeField] private Transform weaponSwayTransdorm;
    private CinemachineBrain cinemachineBrain;
    private Camera mainCam;
    private HeadBob motionBob;

    private Vector2 mouseInput;

    private void Start()
    {
        mainCam = Camera.main;
        cinemachineBrain = mainCam.GetComponent<CinemachineBrain>();
        motionBob = GetComponent<HeadBob>();
    }

    private void Update()
    {
        RotateCamera();
    }

    private void LateUpdate()
    {
        weaponSwayTransdorm.rotation = Quaternion.Lerp(weaponSwayTransdorm.rotation, Quaternion.Euler(mouseInput.y, mouseInput.x, 0), swaySpeed * Time.deltaTime);
    }

    private void RotateCamera()
    {
        mainCam.transform.position = camPosition.position;

        mouseInput.x += Input.GetAxis("Mouse X") * camSensitivity * Time.deltaTime;
        mouseInput.y -= Input.GetAxis("Mouse Y") * camSensitivity * Time.deltaTime;

        mouseInput.y = Mathf.Clamp(mouseInput.y, -85, 85);

        camPosition.localEulerAngles = new Vector3(mouseInput.y,0,0);
        transform.localRotation = Quaternion.Euler(0, mouseInput.x, 0);
    }

    public void HeadBobbing(float _intensity)
    {
        motionBob.SetPlayerSpeed(_intensity);
    }

    /// <summary>
    /// Add Recoil to x and y rotation
    /// </summary>
    /// <param name="_xrecoil"></param>
    /// <param name="_yrecoil"></param>
    public void AddRecoil(float _xrecoil, float _yrecoil)
    {
        mouseInput.x -= _xrecoil;
        mouseInput.y += _yrecoil;
    }
}
