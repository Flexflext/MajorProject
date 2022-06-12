using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderGameLogic : MonoBehaviour
{
    [SerializeField] private int doorOpenerLayer = 8;
    [SerializeField] private float timeTillOpenDoor = 1f;
    [SerializeField] private float range = 1f;
    [SerializeField] private KeyCode doorOpenerKey = KeyCode.Mouse0;
    [SerializeField] private LayerMask rayLayerMask;

    private RaycastHit hit;

    private ProzeduralAnimationLogic animationLogic;

    private void Start()
    {
        animationLogic = GetComponentInChildren<ProzeduralAnimationLogic>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(doorOpenerKey))
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, range, rayLayerMask))
            {
                StartCoroutine(C_OpenDoor(hit.collider.GetComponent<DoorOpener>()));
            }
        }
    }

    private IEnumerator C_OpenDoor(DoorOpener _dooropener)
    {
        if (_dooropener != null)
        {
            ProzeduralAnimationLogic.LegParams leg = animationLogic.GetFrontRightLeg();
            leg.stopLegAnimationFlag = true;


            float curtime = 0;

            while (curtime < timeTillOpenDoor)
            {
                //leg.ikTarget.position = Vector3.Lerp(leg.ikTarget.position, _dooropener.transform.position, curtime / timeTillOpenDoor);
                Debug.Log("HUHU");

                curtime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            _dooropener.OpenDoor();
        }

        
    }
}
