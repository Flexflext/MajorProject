using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum ERandomSound
{
    Static,
    Random,
}

public enum EPossibleSounds
{
    Attack,
    Hit,
    Die,
    Walk,
    WeaponDraw,
    Dash,
    Block,
    Taunt,
    Dialog,
    MagicFire,
    MagicEarth,
    MagicWater,
    MagicWind,
    Regen,
    Foley,
    Fire,
    MagicSelection,
    UI,
    sfxPrev,
    dialogPrev,
    hover,
    voiceLine,
}


[System.Serializable]
public struct SourceSettings
{
    public EAudioMixer MixerGroup;

    [Space]
    public float Volume;
    [Space]
    [Range(0, 1)]
    public float Pitch;
    [Space]
    public bool Loop;
    [Space]
    [Range(0, 1)]
    public float SpatialBlend;
    [Space]
    public bool UseCustomCurve;
    [Space]
    public Vector2 RollOffDistance;
    [Space]
    public AnimationCurve RollOffCurve;
    [Space]
    public bool PlayOnAwake;
}

[System.Serializable]
public struct Sound
{
    public EPossibleSounds SoundType;
    [Space]
    public SourceSettings source;
    [Space]
    public AudioClip staticSound;
    [Space]
    public AudioClip[] randomSound;
}

/// <summary>
/// Personal Audio Manager
/// </summary>
public class PersonelAudioManager : MonoBehaviour
{

    //All Sounds
    [SerializeField] private Sound[] allSounds;

    //Disctionary of All Sources
    private Dictionary<int, AudioSource> allSources = new Dictionary<int, AudioSource>();


    private AudioSource source;
    private AudioClip clip;
    private Sound sound;

    private void Start()
    {
        //initialize
        Initialize();
    }

    /// <summary>
    /// Create AudioSource with give Source Settings
    /// </summary>
    /// <param name="_sourcesettings"></param>
    /// <returns></returns>
    private AudioSource CreateAudioSource(SourceSettings _sourcesettings)
    {
        //Add AudioSource Component
        source = gameObject.AddComponent<AudioSource>();

        //Set AudioSource Settings
        source.pitch = _sourcesettings.Pitch;
        source.volume = _sourcesettings.Volume;
        source.spatialBlend = _sourcesettings.SpatialBlend;
        source.loop = _sourcesettings.Loop;
        source.playOnAwake = _sourcesettings.PlayOnAwake;

        //Check if to use a Custom Curve
        if (_sourcesettings.UseCustomCurve)
        {
            source.rolloffMode = AudioRolloffMode.Custom;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, _sourcesettings.RollOffCurve);
            source.minDistance = _sourcesettings.RollOffDistance.x;
            source.maxDistance = _sourcesettings.RollOffDistance.y;
        }

        //The the AudioMixerGroup for the AudioSource
        AudioMixerManager.Instance.SetMixerGroup(_sourcesettings.MixerGroup, ref source);


        return source;
    }

    /// <summary>
    /// Play a Sound from a give AudioSource
    /// </summary>
    /// <param name="_soundtype"></param>
    /// <param name="_random"></param>
    /// <param name="_override"></param>
    public void Play(EPossibleSounds _soundtype, ERandomSound _random, bool _override)
    {
        //Null Check for the AudioSource
        if (GetSource(_soundtype) == null)
            return;


        clip = null;

        sound = GetSound(_soundtype);

        source = GetSource(_soundtype);


        if (source.isPlaying && !_override)
        {
            return;
        }

        if (_random == ERandomSound.Random)
        {
            clip = sound.randomSound[Random.Range(0, sound.randomSound.Length - 1)];
        }
        else
        {
            clip = sound.staticSound;
        }

        source.clip = clip;
        source.Play();
    }

    /// <summary>
    /// Play Specific AudioClip
    /// </summary>
    /// <param name="_soundtype"></param>
    /// <param name="_clip"></param>
    public void PlaySpecific(EPossibleSounds _soundtype, AudioClip _clip)
    {
        source = null;
        source = GetSource(_soundtype);
        source.clip = _clip;
        source.Play();
    }

    /// <summary>
    /// Stop the play of a given AudioSource
    /// </summary>
    /// <param name="_soundtype"></param>
    public void StopPlay(EPossibleSounds _soundtype)
    {
        //Stop the Sound after Nullcheck
        if (GetSource(_soundtype) != null)
            GetSource(_soundtype).Stop();
    }

    /// <summary>
    /// Initialize All AudioSources
    /// </summary>
    public void Initialize()
    {
        allSources.Clear();

        foreach (Sound curSound in allSounds)
        {
            AudioSource source = CreateAudioSource(curSound.source);
            source.clip = curSound.staticSound;
            allSources.Add((int)curSound.SoundType, source);
        }
    }

    /// <summary>
    /// Get a given AudioSource from a SoundType
    /// </summary>
    /// <param name="_soundtype"></param>
    /// <returns></returns>
    private AudioSource GetSource(EPossibleSounds _soundtype)
    {
        //Go through the dictionary to find the correct AudioSource with the coinsiding value
        foreach (KeyValuePair<int, AudioSource> cur in allSources)
        {
            if (cur.Key == (int)_soundtype)
            {
                return cur.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Get Specific Sound from SoundType
    /// </summary>
    /// <param name="_soundtype"></param>
    /// <returns></returns>
    private Sound GetSound(EPossibleSounds _soundtype)
    {
        //Go through Sound Array to find correct Sound
        foreach (Sound curSound in allSounds)
        {
            //Check Soundtype
            if (curSound.SoundType == _soundtype)
            {
                return curSound;
            }
        }

        return allSounds[0];
    }
}
