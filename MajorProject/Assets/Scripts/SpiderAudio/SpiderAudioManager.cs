using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAudioManager : MonoBehaviour
{

    [SerializeField] private LegAudioState[] spiderLegStates;
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

        spiderLegStates[_leg].legSource.clip = audioLegStates[arrayIndex].audioStateClips[Random.Range(0, audioLegStates[arrayIndex].audioStateClips.Length)];
        spiderLegStates[_leg].legSource.Play();

        
    }

    private void PlayStateChangeSound(int _leg, ELegStates _legstate)
    {
        spiderLegStates[_leg].spiderLegState = _legstate;
    }

    private void PlayDeathSound()
    {
        source.volume = 1;
        source.PlayOneShot(deathSound);
    }

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
