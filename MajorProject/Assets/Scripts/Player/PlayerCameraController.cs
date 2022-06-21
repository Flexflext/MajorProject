using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private float camSensitivity = 100.0f;

    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCam;
    [SerializeField] private Transform camPosition;
    private CinemachineBrain cinemachineBrain;
    private Camera mainCam;

    private Vector2 mouseInput;

    private void Start()
    {
        mainCam = Camera.main;
        cinemachineBrain = mainCam.GetComponent<CinemachineBrain>();
    }

    private void Update()
    {
        RotateCamera();
    }

    private void RotateCamera()
    {
        mainCam.transform.position = camPosition.position;

        mouseInput.x += Input.GetAxis("Mouse X") * camSensitivity * Time.deltaTime;
        mouseInput.y -= Input.GetAxis("Mouse Y") * camSensitivity * Time.deltaTime;

        mouseInput.y = Mathf.Clamp(90, -90, mouseInput.y);

        mainCam.transform.localRotation = Quaternion.Euler(mouseInput.y, 0, 0);
        transform.localRotation = Quaternion.Euler(0, mouseInput.x, 0);
    }

    public void HeadBobbing(Vector3 _inputdir, float _intensity)
    {
        //transform.localPosition += in
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
