using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace MonoGame2026_Heb;

public static class AudioManager
{
    private static List<SoundEffectInstance> _soundEffectsInstance = new();
    
    public static Action<string> PlaySFX = (name) => PlaySoundEffect(name);

    private static float prevVolSong = 1;
    public static void AddSong(string name, string fileName)
    {
        ResourcesManager<Song>.LoadResource(name, fileName);
    }

    public static void AddSoundEffect(string name, string fileName)
    {
        ResourcesManager<SoundEffect>.LoadResource(name, fileName);
    }

    public static string CurrentSongName { get; private set; }
    
    public static float MusicVolume { get; private set; } = 0.5f;
    public static float SFXVolume { get; private set; } = 0.5f;
    
    private static float currentSongRequestedVolume = 1f;

    public static void SetMusicVolume(float volume)
    {
        MusicVolume = MathHelper.Clamp(volume, 0f, 1f);
        if (!MediaPlayer.IsMuted)
        {
            MediaPlayer.Volume = currentSongRequestedVolume * MusicVolume;
        }
    }

    public static void SetSFXVolume(float volume)
    {
        SFXVolume = MathHelper.Clamp(volume, 0f, 1f);
        // Optionally update currently playing sound effects
        foreach (var effect in _soundEffectsInstance)
        {
            if (effect.State == SoundState.Playing)
            {
                // We don't track original requested volume for SFX, so we just set it to SFXVolume
                // (assuming requested volume was 1f)
                effect.Volume = SFXVolume; 
            }
        }
    }

    public static void PlaySong(string name, float volume = 1)
    {
        if (CurrentSongName == name && MediaPlayer.State == MediaState.Playing)
            return;

        Song song = ResourcesManager<Song>.GetResource(name);

        if (song == null) return;

        if (MediaPlayer.State == MediaState.Playing)
            MediaPlayer.Stop();
        
        CurrentSongName = name;
        currentSongRequestedVolume = volume;
        MediaPlayer.Volume = volume * MusicVolume;
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(song);
    }
    
    public static void PlaySoundEffect(string name, bool isLooping = false, float volume = 1, float pitch = 0, float pan = 0)
    {
        SoundEffect effect = ResourcesManager<SoundEffect>.GetResource(name);

        if (effect == null) return;

        SoundEffectInstance instance = effect.CreateInstance();

        _soundEffectsInstance.Add(instance);
        
        instance.Pan = pan;
        instance.Pitch = pitch;
        instance.Volume = volume * SFXVolume;
        instance.IsLooped = isLooping;

        instance.Play();
    }

    public static bool IsMuted
    {
        get
        {
            return MediaPlayer.IsMuted;
        }
        set
        {
            MediaPlayer.IsMuted = value;
            
            foreach (var effect in _soundEffectsInstance)
            {
                if (value == true)
                {
                    prevVolSong = effect.Volume;
                    effect.Volume = 0;
                }
                else
                {
                    effect.Volume= prevVolSong;
                }
            }
        }
    }
    
    public static bool IsPaused
    {
        get
        {
            return MediaPlayer.State == MediaState.Paused;
        }
        set
        {
            if (value == true)
                MediaPlayer.Pause();
            else
                MediaPlayer.Resume();
            
            if (value == true)
                _soundEffectsInstance.ForEach(effect => effect.Pause());
            else
                _soundEffectsInstance.ForEach(effect => effect.Resume());
        }
    }
}