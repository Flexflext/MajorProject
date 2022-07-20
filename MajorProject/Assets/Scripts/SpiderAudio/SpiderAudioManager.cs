using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAudioManager : MonoBehaviour
{
    [SerializeField] private ELegStates[] spiderLegStates;
    [SerializeField] private LegStateAudio[] audioLegStates;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip takeDmgSoundSound;

    private AudioSource source;
    private ProzeduralAnimationLogic animationLogic;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        animationLogic = GetComponentInChildren<ProzeduralAnimationLogic>();
    }

    private void Start()
    {
        animationLogic.AddLegMoveEventListener(PlayWalkSound);
        animationLogic.AddLegStateChangeEventListener(PlayStateChangeSound);
        animationLogic.AddDeathEventListener(PlayDeathSound);
        animationLogic.AddLegTakeDmgEventListener(PlayTakeDmgSound);
    }

    [System.Serializable]
    private struct LegStateAudio
    {
        public ELegStates legState;
        public AudioClip[] audioStateClips;
    }

    private void PlayWalkSound(int _leg)
    {
        int arrayIndex = 0;

        for (int i = 0; i < audioLegStates.Length; i++)
        {
            if (audioLegStates[i].legState == spiderLegStates[_leg])
            {
                arrayIndex = i;
                break;
            }
        }

        source.PlayOneShot(audioLegStates[arrayIndex].audioStateClips[Random.Range(0, audioLegStates[arrayIndex].audioStateClips.Length)]);
    }

    private void PlayStateChangeSound(int _leg, ELegStates _legstate)
    {
        spiderLegStates[_leg] = _legstate;
    }

    private void PlayDeathSound()
    {

    }

    private void PlayTakeDmgSound(int _leg)
    {

    }

    private void OnDestroy()
    {
        animationLogic.RemoveLegMoveEventListener(PlayWalkSound);
        animationLogic.RemoveLegStateChangeEventListener(PlayStateChangeSound);
        animationLogic.RemoveDeathEventListener(PlayDeathSound);
        animationLogic.RemoveLegTakeDmgEventListener(PlayTakeDmgSound);
    }
}
