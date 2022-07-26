using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            StartCoroutine(C_WaitTillOnOff());
        }
    }

    private void DoDamage(Collider _legtodmg)
    {
        LegDamageSystem dmgSys = _legtodmg.GetComponentInParent<LegDamageSystem>();


        if (dmgSys != null)
        {
            dmgSys.TakeDamage(transform.position);
        }
        StopAllCoroutines();
        onOff = false;
        TurnOnOff(false);

        source.clip = randomAudioClips[Random.Range(0, randomAudioClips.Length)];

        source.Play();
    }

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

    private IEnumerator C_PlayImpactEffect(Vector3 _pos)
    {
        impactVFX.transform.position = _pos;
        impactVFX.SetActive(true);

        yield return new WaitForSeconds(0.75f);

        impactVFX.SetActive(false);
    }

    private void TurnOnOff(bool _onoff)
    {
        laser.SetActive(_onoff);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!onOff) return;

        if (other.gameObject.layer == spiderLayer)
        {
            DoDamage(other);

            StartCoroutine(C_PlayImpactEffect(col.ClosestPoint(other.transform.position)));
        }
    }
}
