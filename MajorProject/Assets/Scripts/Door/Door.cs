using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Vector3 localEndPosLeft;
    [SerializeField] private Transform rightDoor;
    [SerializeField] private Vector3 localEndPosRight;

    [SerializeField] private float timeTillOpenDoor = 1;

    private bool open = false;

    public void OpenDoor()
    {
        if (open) return;

        open = true;
        StartCoroutine(C_OpenSliderDoor());
    }

    private IEnumerator C_OpenSliderDoor()
    {
        float curTime = 0;

        Vector3 startPosLeft = leftDoor.localPosition;
        Vector3 startPosRight = rightDoor.localPosition;

        while (curTime < timeTillOpenDoor)
        {
            leftDoor.localPosition = Vector3.Lerp(startPosLeft, localEndPosLeft, curTime / timeTillOpenDoor);

            rightDoor.localPosition = Vector3.Lerp(startPosRight, localEndPosRight, curTime / timeTillOpenDoor);

            curTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
