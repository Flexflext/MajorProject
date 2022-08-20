using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Class for Spider Status UI
/// </summary>
public class SpiderSingleStatusUI : MonoBehaviour
{
    [SerializeField] private Color green;
    [SerializeField] private Color yellow;
    [SerializeField] private Color red;
    [SerializeField] private Color broken;
    [SerializeField] private string aliveText;
    [SerializeField] private string brokenText;


    [SerializeField] private Image indikatorImg;
    [SerializeField] private TMP_Text indikatorText;

    /// <summary>
    /// Set Status UI of a Single UI Instance
    /// </summary>
    /// <param name="_legstate"></param>
    public void SetIndikator(ELegStates _legstate)
    {
        switch (_legstate)
        {
            case ELegStates.LS_Normal:
                indikatorImg.color = green;
                indikatorText.text = aliveText;
                break;
            case ELegStates.LS_Limping:
                indikatorImg.color = yellow;
                indikatorText.text = aliveText;
                break;
            case ELegStates.LS_LimpingHalfLeg:
                indikatorImg.color = red;
                indikatorText.text = aliveText;
                break;
            case ELegStates.LS_Broken:
                indikatorImg.color = broken;
                indikatorText.text = brokenText;
                break;
        }
    }
}
