using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu()]
public class ScriptableEvent : ScriptableObject, ISerializationCallbackReceiver
{
    private List<ScriptableEventListener> listeners = new List<ScriptableEventListener>();

    public void AddListener(ScriptableEventListener _toAdd)
    {
        listeners.Add(_toAdd);
    }

    public void RemoveListener(ScriptableEventListener _toremove)
    {
        listeners.Remove(_toremove);
    }

    public void Raise()
    {
        for (int i = 0; i < listeners.Count; i++)
        {
            listeners[i].RaiseEvent();
        }
    }

    public void OnBeforeSerialize()
    {
        
        
    }

    public void OnAfterDeserialize()
    {
        Debug.Log("After");
        listeners = new List<ScriptableEventListener>();
    }
}
