using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderStatusUI : MonoBehaviour
{
    [SerializeField] private SpiderSingleStatusUI[] spiderLegStatus;
    [SerializeField] private SpiderSingleStatusUI spiderBodyStatus;

    public void SetLegStatus(int _leg, ELegStates _legstate)
    {
        if (_leg == -1)
        {
            spiderBodyStatus.SetIndikator(_legstate);
            return;
        }

        spiderLegStatus[_leg].SetIndikator(_legstate);
    }
}
