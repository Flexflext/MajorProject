using UnityEngine;
using UnityEngine.Audio;

public enum EAudioMixer
{
    Ambient,
    SFX,
}

/// <summary>
/// Audio Mixer Manager to Handle Audio
/// </summary>
public class AudioMixerManager : MonoBehaviour
{
    public static AudioMixerManager Instance;

    [Header("AllMixer")]
    [SerializeField] private AudioMixerGroup masterMixer;
    [SerializeField] private AudioMixerGroup ambientMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

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

        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// Set the Master Volume
    /// </summary>
    /// <param name="_volume"></param>
    public void SetMasterVolume(float _volume)
    {
        //Set Volume
        if (_volume > 0)
            masterMixer.audioMixer.SetFloat("MasterVolume", (Mathf.Log10(_volume) * 20));
        else
            masterMixer.audioMixer.SetFloat("MasterVolume", (Mathf.Log10(_volume) * 0));

    }

    /// <summary>
    /// Set the Ambient Music Volume
    /// </summary>
    /// <param name="_volume"></param>
    public void SetAmbientVolume(float _volume)
    {
        //Set Volume
        if (_volume > 0)
            ambientMixerGroup.audioMixer.SetFloat("AmbientVolume", (Mathf.Log10(_volume) * 20));
        else
            ambientMixerGroup.audioMixer.SetFloat("AmbientVolume", (Mathf.Log10(_volume) * 0));
    }

    /// <summary>
    /// Set VFX Volume
    /// </summary>
    /// <param name="_volume"></param>
    public void SetSFXVolume(float _volume)
    {
        //Set Volume
        if (_volume > 0)
            sfxMixerGroup.audioMixer.SetFloat("SFXVolume", (Mathf.Log10(_volume) * 20));
        else
            sfxMixerGroup.audioMixer.SetFloat("SFXVolume", (Mathf.Log10(_volume) * 0));
    }

    /// <summary>
    /// Set the AudioMixer at an AudioSource
    /// </summary>
    /// <param name="_mixertype"></param>
    /// <param name="_source"></param>
    public void SetMixerGroup(EAudioMixer _mixertype, ref AudioSource _source)
    {
        //Check What Mixer to Set
        switch (_mixertype)
        {
            case EAudioMixer.Ambient:
                _source.outputAudioMixerGroup = ambientMixerGroup;
                break;
            case EAudioMixer.SFX:
                _source.outputAudioMixerGroup = sfxMixerGroup;
                break;
            default:
                break;
        }
    }
}
