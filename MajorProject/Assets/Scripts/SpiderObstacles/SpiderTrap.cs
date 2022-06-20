using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderTrap : MonoBehaviour
{
    [SerializeField] private int legDamage;
    [SerializeField] private int spiderLayer;

    [SerializeField] private bool constantlyOn;
    [SerializeField] private float onOffTimer;

    [SerializeField] private GameObject laser;


    private bool onOff = true;

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

        if (dmgSys != null) dmgSys.TakeDamage();
        StopAllCoroutines();
        onOff = false;
        TurnOnOff(false);
        StartCoroutine(C_WaitTillOnOff());
    }

    private IEnumerator C_WaitTillOnOff()
    {
        yield return new WaitForSeconds(onOffTimer);
        onOff = !onOff;
        TurnOnOff(onOff);
        StartCoroutine(C_WaitTillOnOff());
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
        }
    }
}
