using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.015f;
    [SerializeField] private float frequency = 10.0f;
    [SerializeField] private float focusDistance = 15.0f;

    [Space]
    [SerializeField] private float shakeamplitude = 0.015f;
    [SerializeField] private float shakefrequency = 10.0f;
    [SerializeField] private float shaketime = 15.0f;
    private float curtime;

    [SerializeField] private Transform camHolder;
    [SerializeField] private Transform cam;

    [SerializeField] private float playerSpeed;
    private Vector3 startPos;

    private bool playerIsControlling;

    private void Awake()
    {
        
        startPos = cam.transform.localPosition;
    }

    private void Update()
    {
        if (!playerIsControlling) return;

        CheckMotion();
        ResetCamPosition();
        cam.LookAt(FocusTarget());

        if (curtime > 0)
        {
            ShakeScreen();
            curtime -= Time.deltaTime;
        }
    }

    private void CheckMotion()
    {
        PlayMotion(FootStepMotion() * playerSpeed);
    }

    private void PlayMotion(Vector3 _vec)
    {
        cam.localPosition += _vec;
    }

    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;

        pos.y += Mathf.Sin(Time.time * frequency) * amplitude;
        pos.x += Mathf.Sin(Time.time * frequency / 2) * amplitude * 2;

        return pos;
    }

    private void ResetCamPosition()
    {
        if (cam.localPosition != startPos)
        {
            cam.localPosition = Vector3.Lerp(cam.localPosition, startPos, 1 * Time.deltaTime);
        }
    }

    private Vector3 FocusTarget()
    {
        Vector3 pos = new Vector3(transform.position.x, transform.position.y + camHolder.localPosition.y, transform.position.z);
        pos += camHolder.forward * focusDistance;
        return pos;
    }

    public void SetPlayerSpeed(float _speed)
    {
        playerSpeed = _speed;
    }

    public void SetScreenShake()
    {
        curtime = shaketime;
    }
    private void ShakeScreen()
    {
        Vector3 pos = Vector3.zero;

        pos.y += Mathf.Sin(Time.time * shakefrequency) * shakeamplitude;
        pos.x += Mathf.Cos(Time.time * shakefrequency) * shakeamplitude;

        PlayMotion(pos);
    }

    public void SetPlayerControll(bool _controll)
    {
        playerIsControlling = _controll;
    }
} 
