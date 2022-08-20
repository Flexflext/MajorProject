using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spider Audio Manager
/// </summary>
public class SpiderAudioManager : MonoBehaviour
{
    [Tooltip("State and Sources for all Legs")]
    [SerializeField] private LegAudioState[] spiderLegStates;
    [Tooltip("State and Clips for all Leg-States")]
    [SerializeField] private LegStateAudioCues[] audioLegStates;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip takeDmgSoundSound;

    private ProzeduralAnimationLogic animationLogic;
    private AudioSource source;

    [System.Serializable]
    private struct LegStateAudioCues
    {
        public ELegStates legState;
        public AudioClip[] audioStateClips;
    }

    [System.Serializable]
    private struct LegAudioState
    {
        public ELegStates spiderLegState;
        public AudioSource legSource;
    }

    private void Awake()
    {
        animationLogic = GetComponentInChildren<ProzeduralAnimationLogic>();
        source = GetComponent<AudioSource>();
    }

    private void Start()
    {
        animationLogic.AddLegMoveEventListener(PlayWalkSound);
        animationLogic.AddLegStateChangeEventListener(PlayStateChangeSound);
        animationLogic.AddDeathEventListener(PlayDeathSound);
        animationLogic.AddLegTakeDmgEventListener(PlayTakeDmgSound);
    }

    /// <summary>
    /// Play Walk Sound
    /// </summary>
    /// <param name="_leg"></param>
    private void PlayWalkSound(int _leg)
    {
        int arrayIndex = 0;

        for (int i = 0; i < audioLegStates.Length; i++)
        {
            if (audioLegStates[i].legState == spiderLegStates[_leg].spiderLegState)
            {
                arrayIndex = i;
                break;
            }
        }

        if (!spiderLegStates[_leg].legSource.isActiveAndEnabled)
        {
            return;
        }


        spiderLegStates[_leg].legSource.clip = audioLegStates[arrayIndex].audioStateClips[Random.Range(0, audioLegStates[arrayIndex].audioStateClips.Length)];
        spiderLegStates[_leg].legSource.Play();

        
    }

    /// <summary>
    /// Chnage the Sound of the Leg Movement
    /// </summary>
    /// <param name="_leg"></param>
    /// <param name="_legstate"></param>
    private void PlayStateChangeSound(int _leg, ELegStates _legstate)
    {
        spiderLegStates[_leg].spiderLegState = _legstate;
    }

    /// <summary>
    /// Play a Death Sound and sets Volume to 1
    /// </summary>
    private void PlayDeathSound()
    {
        source.volume = 1;
        source.PlayOneShot(deathSound);
    }

    /// <summary>
    /// Play a Take Damage Sound
    /// </summary>
    /// <param name="_leg"></param>
    private void PlayTakeDmgSound(int _leg)
    {
        source.PlayOneShot(takeDmgSoundSound);
    }

    private void OnDestroy()
    {
        animationLogic.RemoveLegMoveEventListener(PlayWalkSound);
        animationLogic.RemoveLegStateChangeEventListener(PlayStateChangeSound);
        animationLogic.RemoveDeathEventListener(PlayDeathSound);
        animationLogic.RemoveLegTakeDmgEventListener(PlayTakeDmgSound);
    }
}
