using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")] 
    public AudioClip click;
    public AudioClip match;
    public AudioClip noMove;
    public AudioClip win;
    public AudioClip lose;
    public AudioClip bgmMenu;
    public AudioClip bgmGameplay;

    [Header("Settings")] 
    [Range(0f,1f)] public float musicVolume = 0.7f;
    [Range(0f,1f)] public float sfxVolume = 1f;
    public bool muteMusic = false;
    public bool muteSfx = false;

    private void Awake()
    {
        // Ensure sources exist
        if (musicSource == null)
        {
            var go = new GameObject("MusicSource");
            go.transform.SetParent(transform);
            musicSource = go.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        if (sfxSource == null)
        {
            var go = new GameObject("SfxSource");
            go.transform.SetParent(transform);
            sfxSource = go.AddComponent<AudioSource>();
        }
        ApplyVolumes();
    }

    public void ApplyVolumes()
    {
        musicSource.volume = muteMusic ? 0f : musicVolume;
        sfxSource.volume = muteSfx ? 0f : sfxVolume;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySfx(AudioClip clip, float pitch = 1f)
    {
        if (clip == null || muteSfx) return;
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // Convenience helpers
    public void Click() => PlaySfx(click);
    public void Match() => PlaySfx(match);
    public void NoMove() => PlaySfx(noMove);
    public void Win() => PlaySfx(win);
    public void Lose() => PlaySfx(lose);
    public void ToggleMusicMute() { muteMusic = !muteMusic; ApplyVolumes(); }
    public void ToggleSfxMute() { muteSfx = !muteSfx; ApplyVolumes(); }
}
