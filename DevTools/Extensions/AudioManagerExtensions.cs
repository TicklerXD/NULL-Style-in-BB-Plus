using System.Collections.Generic;
using MidiPlayerTK;
using PixelInternalAPI.Extensions;
using UnityEngine;
using static NULL.Manager.ModManager;

namespace DevTools.Extensions;

public static class AudioManagerExtensions
{
    static List<fluid_voice> heldVoices = [];
    ///<summary>
    /// Plays the audio clip in the AudioSource.
    ///</summary>
    ///<param name="source"></param>
    ///<param name="name"></param>
    ///<param name="loop"></param>
    public static void Play(this AudioSource source, string name, bool loop = false)
    {
        source.clip = m.Get<SoundObject>(name).soundClip;
        source.loop = loop;
        source.Play();
    }
    ///<summary>
    /// Plays the SoundObject in the AudioManager.
    ///</summary>
    ///<param name="audMan"></param>
    ///<param name="name"></param>
    ///<param name="loop"></param>
    public static void Play(this AudioManager audMan, string name, bool loop = false)
    {
        audMan.FlushQueue(true);
        audMan.QueueAudio(name, loop);
    }
    ///<summary>
    /// Queues the SoundObject in the AudioManager.
    ///</summary>
    ///<param name="audMan"></param>
    ///<param name="name"></param>
    ///<param name="loop"></param>
    public static void QueueAudio(this AudioManager audMan, string name, bool loop = false)
    {
        audMan.QueueAudio(m.Get<SoundObject>(name));
        audMan.SetLoop(loop);
    }

    public static void PlaySingle(this AudioManager man, string name) => man.PlaySingle(m.Get<SoundObject>(name));
    ///<summary>
    /// Queues the LoopingSoundObject in the MusicManager.
    ///</summary>
    ///<param name="mm"></param>
    ///<param name="name"></param>
    ///<param name="loop"></param>
    public static void QueueFile(this MusicManager mm, string name, bool loop = false) => mm.QueueFile(m.Get<LoopingSoundObject>(name), loop);
    ///<summary>
    /// Stops MIDI playback and kills the MIDI synth.
    ///</summary>
    ///<param name="mm">The MusicManager instance.</param>
    public static void KillMidi(this MusicManager mm)
    {
        mm.HangMidi(false);
        mm.StopMidi();
        mm.MidiPlayer.MPTK_StopSynth();
    }
    ///<summary>
    /// Hang MIDI on one note, keeping/not keeping drums
    ///</summary>
    ///<param name="mm">The MusicManager instance.</param>
    public static void HangMidi(this MusicManager mm, bool stop, bool keepDrums = false)
    {
        var voices = mm.MidiPlayer.ActiveVoices;
        foreach (fluid_voice fluid_voice in heldVoices)
        {
            if (fluid_voice != null)
            {
                fluid_voice.DurationTick = 0L;
            }
        }
        heldVoices.Clear();
        for (int i = 0; i < 16; i++)
        {
            if (i != 9)
            {
                mm.MidiPlayer.MPTK_ChannelEnableSet(i, !stop);
            }
            else
            {
                mm.MidiPlayer.MPTK_ChannelEnableSet(i, !stop || keepDrums);
            }
        }
        if (stop && voices != null)
        {
            foreach (fluid_voice fluid_voice2 in voices)
            {
                fluid_voice2.DurationTick = -1L;
                heldVoices.Add(fluid_voice2);
            }
        }
    }
    ///<summary>
    /// Plays a sound clip at the specified point.
    ///</summary>
    ///<param name="sound">Sound clip to play.</param>
    ///<param name="point">Point to play clip.</param>
    ///<param name="minDistance"></param>
    ///<param name="maxDistance"></param>
    ///<param name="volume"></param>
    ///<exception cref="KeyNotFoundException"></exception>
    public static void PlaySoundAtPoint(SoundObject sound, Vector3 point, float minDistance = 25, float maxDistance = 50, float volume = 1)
    {
        var audman = new GameObject("PlaySoundAtPoint").CreateAudioManager(minDistance, maxDistance);
        audman.audioDevice.volume = volume;
        audman.transform.position = point;
        audman.PlaySingle(sound);
        Object.Destroy(audman.gameObject, sound.soundClip.length * ((Time.timeScale < 0.01f) ? 0.01f : Time.timeScale));
    }
    ///<summary>
    /// Plays a sound clip at the specified point.
    ///</summary>
    ///<param name="sound">Name of sound.</param>
    ///<param name="point">Point to play clip.</param>
    ///<param name="minDistance"></param>
    ///<param name="maxDistance"></param>
    ///<param name="volume"></param>
    ///<exception cref="KeyNotFoundException"></exception>
    public static void PlaySoundAtPoint(string sound, Vector3 point, float minDistance = 25, float maxDistance = 50, float volume = 1) => PlaySoundAtPoint(m.Get<SoundObject>(sound), point, minDistance, maxDistance, volume);
}
