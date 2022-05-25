using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class ScriptableEventListener : MonoBehaviour
{
    [SerializeField] private ScriptableEvent scriptableEvent;
    [SerializeField] private UnityEvent myEvent;

    public void RaiseEvent()
    {
        myEvent.Invoke();
    }

    private void Awake()
    {
        scriptableEvent.AddListener(this);
    }

    private void OnDestroy()
    {
        scriptableEvent.RemoveListener(this);
    }
}
