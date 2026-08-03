using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace MonoGame2026_Heb;

public static class AudioManager
{
    private static List<SoundEffectInstance> _soundEffectsInstance = new();

    private static float prevVolSong = 1;
    public static void AddSong(string name, string fileName)
    {
        ResourcesManager<Song>.LoadResource(name, fileName);
    }

    public static void AddSoundEffect(string name, string fileName)
    {
        ResourcesManager<SoundEffect>.LoadResource(name, fileName);
    }

    public static void PlaySong(string name, float volume = 1)
    {
        Song song = ResourcesManager<Song>.GetResource(name);

        if (song == null) return;

        if (MediaPlayer.State == MediaState.Playing)
            MediaPlayer.Stop();
        
        MediaPlayer.Volume = volume;
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
        instance.Volume = volume;
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