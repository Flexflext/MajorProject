using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Confirm-Panel Script for UI
/// </summary>
public class ConfirmPanel : MonoBehaviour
{
    //TtitleText to Set
    [SerializeField] private TMPro.TMP_Text titleText;

    private System.Action<bool> onPressAction;

    /// <summary>
    /// Sets the Ui panel and adds the Callback
    /// </summary>
    /// <param name="_title">Title to set on the Panel</param>
    /// <param name="_callback">Callback Method</param>
    public void SetPanel(string _title, System.Action<bool> _callback)
    {
        if (!string.IsNullOrEmpty(_title))
        {
            titleText.text = _title;
        }
        else
        {
            titleText.text = "Are u Sure?";
        }

        onPressAction += _callback;
    }

    /// <summary>
    /// Calls Callback to Either Confirm or Stop the Process
    /// </summary>
    /// <param name="_confirm"></param>
    public void SetCallback(bool _confirm)
    {
        if (onPressAction != null)
        {
            onPressAction.Invoke(_confirm);
            onPressAction = null;
            this.gameObject.SetActive(false);
        }
    }
}
