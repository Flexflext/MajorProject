using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public static HUD Instance;

    [SerializeField] private GameObject canChangeObj;

    [SerializeField] private float healthFadeTime;
    [SerializeField] private TMPro.TMP_Text playerHealthText;
    [SerializeField] private Image playerHealthNum;
    [SerializeField] private Image playerFadeHealthBar;
    [SerializeField] private Animator hitAnimator;
    [SerializeField] private Animator killAnimator;

    private int hitHash;

    private SpiderStatusUI statusUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
        }

        statusUI = GetComponentInChildren<SpiderStatusUI>();
        hitHash = Animator.StringToHash("Hit");
    }

    public void SetSpiderLegStatus(int _leg, ELegStates _legstate)
    {
        statusUI.SetLegStatus(_leg, _legstate);
    }

    public void SetCanChange(bool _canchange)
    {
        if (canChangeObj.activeSelf != _canchange)
        {
            canChangeObj.SetActive(_canchange);
        }
        
    }

    public void HitObjAnim()
    {
        hitAnimator.SetTrigger(hitHash);
    }

    public void KillObjAnim()
    {
        killAnimator.SetTrigger(hitHash);
    }

    public void SetPlayerHealth(float _percent)
    {
        StopAllCoroutines();
        playerFadeHealthBar.fillAmount = playerHealthNum.fillAmount;
        playerHealthNum.fillAmount = _percent;
        playerHealthText.text = $"{(_percent * 100).ToString("0")}/100";
        StartCoroutine(C_HealthFade(playerFadeHealthBar.fillAmount, _percent));
    }

    private IEnumerator C_HealthFade(float _frompercent, float _topercent)
    {
        float curTime = 0;

        while (healthFadeTime > curTime)
        {
            playerFadeHealthBar.fillAmount = Mathf.Lerp(_frompercent, _topercent, curTime / healthFadeTime);

            curTime += Time.deltaTime;
            yield return null;
        }
    }
}
