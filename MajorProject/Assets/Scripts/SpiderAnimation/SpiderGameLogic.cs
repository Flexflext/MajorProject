using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderGameLogic : MonoBehaviour
{
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
            //Debug.DrawLine(Camera.main.transform.position, Camera.main.transform.position + transform.forward * range, Color.red, 1);
            if (Physics.Raycast(Camera.main.transform.position, transform.forward, out hit, range, rayLayerMask))
            {
                StartCoroutine(C_OpenDoor(hit.collider.GetComponent<DoorOpener>()));
            }
        }
    }

    private IEnumerator C_OpenDoor(DoorOpener _dooropener)
    {
            Debug.Log("HUHU");
        if (_dooropener != null)
        {
            int leg = animationLogic.GetFrontLeg();
            animationLogic.StartStoplegAnimation(leg, true);

            Vector3 newPos = Vector3.zero;
            Vector3 originalPos = animationLogic.GetLegTargetPosition(leg);
            float curtime = 0;

            while (curtime < timeTillOpenDoor)
            {
                newPos = Vector3.Lerp(originalPos, _dooropener.transform.position, curtime / timeTillOpenDoor);

                animationLogic.SetLegTargetPosition(newPos, leg);

                curtime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            curtime = 0;
            _dooropener.OpenDoor();

            while (curtime < timeTillOpenDoor)
            {
                newPos = Vector3.Lerp(_dooropener.transform.position, originalPos, curtime / timeTillOpenDoor);

                animationLogic.SetLegTargetPosition(newPos, leg);

                curtime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            animationLogic.StartStoplegAnimation(leg, false);
        }

        
    }
}
