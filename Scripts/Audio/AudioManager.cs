using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound currentMusic;

    public Sound[] menuSounds;
    public Sound[] combatSounds;
    public Sound[] musicSounds;

    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // Generate sounds from each array
        GenerateSounds();
    }

    void GenerateSounds()
    {
        foreach (Sound s in menuSounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.looping;
        }

        foreach (Sound s in combatSounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.looping;
        }

        foreach (Sound s in musicSounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.looping;
        }
    }

    // Play a sound from menuSounds or combatSounds. If it can't find the sound in either array, return.
    public void Play(string name)
    {
        Sound s = Array.Find(menuSounds, sound => sound.name == name);

        // If it's not a menu sound, look in combatSounds
        if (s == null)
        {
            s = Array.Find(combatSounds, sound => sound.name == name);

            if(s == null)
            {
                return;
            }
        }

        s.source.PlayOneShot(s.clip);
    }

    public void SetMusic(string name)
    {
        Sound newMusic = Array.Find(musicSounds, sound => sound.name == name);

        if (newMusic == null)
        {
            return;
        }

        if (currentMusic.name != "")
        {
            currentMusic.source.Stop();
        }

        currentMusic = newMusic;

        currentMusic.source.Play();
    }
}
