using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for the Door-Functionality
/// </summary>
public class Door : MonoBehaviour
{
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Vector3 localEndPosLeft;
    [SerializeField] private Transform rightDoor;
    [SerializeField] private Vector3 localEndPosRight;

    [SerializeField] private Light onDoorLight;
    [SerializeField] private Light onRecieverLight;

    [SerializeField] private float timeTillOpenDoor = 1;
    [SerializeField] private AudioSource toggleSource;
    [SerializeField] private AudioSource doorSource;

    private bool open = false;

    /// <summary>
    /// Opens the Sliding Doors and Sets the open paramgter
    /// </summary>
    public void OpenDoor()
    {
        if (open) return;


        onDoorLight.color = Color.green;
        onRecieverLight.color = Color.red;
        

        toggleSource.Play();
        open = true;
        StartCoroutine(C_OpenSliderDoor());
    }

    /// <summary>
    /// Coroutine to Open the Slide Door
    /// </summary>
    /// <returns></returns>
    private IEnumerator C_OpenSliderDoor()
    {
        float curTime = 0;

        Vector3 startPosLeft = leftDoor.localPosition;
        Vector3 startPosRight = rightDoor.localPosition;
        doorSource.Play();
        while (curTime < timeTillOpenDoor)
        {

            leftDoor.localPosition = Vector3.Lerp(startPosLeft, localEndPosLeft, curTime / timeTillOpenDoor);

            rightDoor.localPosition = Vector3.Lerp(startPosRight, localEndPosRight, curTime / timeTillOpenDoor);

            curTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
