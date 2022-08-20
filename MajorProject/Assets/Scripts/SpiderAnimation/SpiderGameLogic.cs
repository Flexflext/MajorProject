using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for InGame Logic
/// </summary>
public class SpiderGameLogic : MonoBehaviour
{
    [Tooltip("Animation time till spider opens the door")]
    [SerializeField] private float timeTillOpenDoor = 1f;
    [Tooltip("Raycast Length from Camera")]
    [SerializeField] private float range = 1f;
    [Tooltip("Open-Door Button")]
    [SerializeField] private KeyCode doorOpenerKey = KeyCode.Mouse0;
    [Tooltip("Door Ray Mask")]
    [SerializeField] private LayerMask rayLayerMask;
    [Tooltip("Spider Damage VFX")]
    [SerializeField] private GameObject dmgVFX;
    [Tooltip("Spider Damage VFX Time")]
    [SerializeField] private float dmgVFXTime;

    [Tooltip("Animation Start Sound")]
    [SerializeField] private AudioClip startLeg;
    [Tooltip("Animation End Sound")]
    [SerializeField] private AudioClip endLeg;
    private AudioSource source;


    private RaycastHit hit;

    private ProzeduralAnimationLogic animationLogic;
    private ThirdPersonSpiderMovement spiderMovement;

    private void Start()
    {
        source = GetComponent<AudioSource>();
        spiderMovement = GetComponent<ThirdPersonSpiderMovement>();
        animationLogic = GetComponentInChildren<ProzeduralAnimationLogic>();
        animationLogic.AddLegTakeDmgEventListener(OnSpiderTakeDmg);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale < 1) return;

        if (Input.GetKeyDown(doorOpenerKey))
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, range, rayLayerMask))
            {
                StartCoroutine(C_OpenDoor(hit.collider.GetComponent<DoorOpener>()));
            }
        }
    }

    /// <summary>
    /// Coroutine to Play an Open Door Animation
    /// </summary>
    /// <param name="_dooropener"></param>
    /// <returns></returns>
    private IEnumerator C_OpenDoor(DoorOpener _dooropener)
    {
        if (_dooropener != null)
        {
            spiderMovement.StopUserInput();

            int leg = animationLogic.GetFrontLeg();
            animationLogic.StartStoplegAnimation(leg, true);

            Vector3 newPos = Vector3.zero;
            Vector3 originalPos = animationLogic.GetLegTargetPosition(leg);
            float curtime = 0;

            source.PlayOneShot(startLeg);

            while (curtime < timeTillOpenDoor)
            {
                newPos = Vector3.Lerp(originalPos, _dooropener.transform.position, curtime / timeTillOpenDoor);

                animationLogic.SetLegTargetPosition(newPos, leg);

                curtime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            curtime = 0;
            _dooropener.OpenDoor();

            source.PlayOneShot(endLeg);

            while (curtime < timeTillOpenDoor)
            {
                newPos = Vector3.Lerp(_dooropener.transform.position, originalPos, curtime / timeTillOpenDoor);

                animationLogic.SetLegTargetPosition(newPos, leg);

                curtime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            animationLogic.StartStoplegAnimation(leg, false);
            spiderMovement.StartUserInput();
        }
        
    }

    /// <summary>
    /// Event when Spider takes Dmg
    /// </summary>
    /// <param name="_leg"></param>
    private void OnSpiderTakeDmg(int _leg)
    {
        StartCoroutine(C_TurnOnOffVFX());
    }

    /// <summary>
    /// Coroutine to turn the Take Dmg VFX on and Off
    /// </summary>
    /// <returns></returns>
    private IEnumerator C_TurnOnOffVFX()
    {
        dmgVFX.SetActive(true);

        yield return new WaitForSeconds(dmgVFXTime);

        dmgVFX.SetActive(false);
    }
}
