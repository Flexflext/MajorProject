using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraInfos : MonoBehaviour
{
    [SerializeField] private GameObject infoPanel;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            infoPanel.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            infoPanel.SetActive(false);
        }
    }
}
