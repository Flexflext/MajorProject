using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spider Trap Logic
/// </summary>
public class SpiderTrap : MonoBehaviour
{
    [SerializeField] private int legDamage;
    [SerializeField] private int spiderLayer;

    [SerializeField] private bool constantlyOn;
    [SerializeField] private float onOffTimer;

    [SerializeField] private AudioClip[] randomAudioClips;

    [SerializeField] private GameObject laser;
    [SerializeField] private GameObject impactVFX;


    private AudioSource source;

    private Collider col;
    private bool onOff = true;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        col = GetComponent<Collider>();
    }

    private void Start()
    {
        if (!constantlyOn)
        {
            StopAllCoroutines();
            StartCoroutine(C_WaitTillOnOff());
        }
    }

    private void OnEnable()
    {
        if (!constantlyOn)
        {
            StopAllCoroutines();
            StartCoroutine(C_WaitTillOnOff());
        }
    }

    /// <summary>
    /// Do Damage to Spider
    /// </summary>
    /// <param name="_legtodmg"></param>
    /// <returns></returns>
    private bool DoDamage(Collider _legtodmg)
    {
        //Get Damage Sys for Leg
        LegDamageSystem dmgSys = _legtodmg.GetComponentInParent<LegDamageSystem>();


        //Check to Do Damage
        if (dmgSys != null)
        {
            dmgSys.TakeDamage(transform.position);
        }
        else
        {
            return false;
        }

        StopAllCoroutines();


        onOff = false;
        TurnOnOff(false);
        StartCoroutine(C_WaitTillOnOff());

        source.clip = randomAudioClips[Random.Range(0, randomAudioClips.Length)];

        source.Play();

        return true;
    }


    /// <summary>
    /// Coroutine to turn the Trap on and off
    /// </summary>
    /// <returns></returns>
    private IEnumerator C_WaitTillOnOff()
    {
        yield return new WaitForSeconds(onOffTimer);

        onOff = !onOff;
        
        TurnOnOff(onOff);

        if (!constantlyOn)
        {
            StartCoroutine(C_WaitTillOnOff());
        }
    }

    /// <summary>
    /// Play Impact Effect at the closest Position 
    /// </summary>
    /// <param name="_pos"></param>
    /// <returns></returns>
    private IEnumerator C_PlayImpactEffect(Vector3 _pos)
    {
        impactVFX.transform.position = _pos;
        impactVFX.SetActive(true);

        yield return new WaitForSeconds(0.75f);

        impactVFX.SetActive(false);
    }

    /// <summary>
    /// Turn the Laser On and Off
    /// </summary>
    /// <param name="_onoff"></param>
    private void TurnOnOff(bool _onoff)
    {
        laser.SetActive(_onoff);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!onOff) return;

        if (other.gameObject.layer == spiderLayer)
        {
            if (DoDamage(other))
            {
                StartCoroutine(C_PlayImpactEffect(col.ClosestPoint(other.transform.position)));
            }       
        }
    }
}
