using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderPlayGroundManager : MonoBehaviour
{
    public Transform followObj;
    public Vector3 offset;

    private ThirdPersonSpiderMovement movementController;

    private void Start()
    {
        movementController = GetComponent<ThirdPersonSpiderMovement>();
        PlaygroundManager.Instance.Subscribe(this);
    }

    public void StartStopSpider(bool _tosetto)
    {
        if (_tosetto)
        {
            movementController.SetPlayerStartControll();
        }
        else
        {
            movementController.SetPlayerStopControll();
        }

    }

    private void OnDestroy()
    {
        PlaygroundManager.Instance.Subscribe(this);
    }
}
