using UnityEngine;

public class PatientAudio : MonoBehaviour
{
    [Header("Audio Sources & Clips")]
    public AudioSource ademhalingAudioSource;
    public AudioClip airwayClip;
    public AudioClip breathingClip;
    public AudioClip heartSoundClip;

    public void ExamineerAirway() => SpeelAudio(airwayClip);
    public void ExamineerBreathing() => SpeelAudio(breathingClip);

    public void SpeelStethoscopeGeluidAf()
    {
        if (ademhalingAudioSource == null) return;
        AudioClip clip = heartSoundClip != null ? heartSoundClip : (breathingClip != null ? breathingClip : airwayClip);
        if (clip != null) ademhalingAudioSource.PlayOneShot(clip);
    }

    public void SpeelCompressieGeluid()
    {
        if (ademhalingAudioSource != null && breathingClip != null)
        {
            ademhalingAudioSource.PlayOneShot(breathingClip, 0.4f);
        }
    }

    private void SpeelAudio(AudioClip clip)
    {
        if (ademhalingAudioSource != null && clip != null)
        {
            ademhalingAudioSource.Stop();
            ademhalingAudioSource.clip = clip;
            ademhalingAudioSource.Play();
        }
    }
}