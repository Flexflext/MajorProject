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

    /// <summary>
    /// Start Stop a Spider
    /// </summary>
    /// <param name="_tosetto"></param>
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

    /// <summary>
    /// Let the Spider Die
    /// </summary>
    public void Die()
    {
        PlaygroundManager.Instance.SetDeath();
    }

    private void OnDestroy()
    {
        PlaygroundManager.Instance.Subscribe(this);
    }
}
