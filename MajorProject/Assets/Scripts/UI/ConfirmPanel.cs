using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmPanel : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text titleText;

    private System.Action<bool> onPressAction;

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
