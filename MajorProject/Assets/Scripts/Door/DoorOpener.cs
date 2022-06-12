using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorOpener : MonoBehaviour
{
    [SerializeField] private Door myDoor;

    public void OpenDoor()
    {
        myDoor.OpenDoor();
    }
}
