using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// On Impact Sound Effects
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class OnImpactSound : MonoBehaviour
{
    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
       source.Play();
    }
}
