using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;

    private bool open = false;

    public void OpenDoor()
    {
        if (open) return;

        open = true;
        print("Door Opened");
    }
}
