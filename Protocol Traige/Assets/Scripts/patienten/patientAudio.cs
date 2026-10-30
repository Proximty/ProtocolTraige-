using UnityEngine;

public class PatientAudio : MonoBehaviour
{
    [Header("Audio Sources & Clips")]
    public AudioSource ademhalingAudioSource;
    public AudioClip airwayClip;
    public AudioClip breathingClip;
    public AudioClip heartSoundClip;

    private static AudioClip proceduralHeartbeatClip;
    private static AudioClip proceduralBreathingClip;

    private void Awake()
    {
        if (ademhalingAudioSource == null)
        {
            ademhalingAudioSource = GetComponent<AudioSource>();
            if (ademhalingAudioSource == null)
            {
                ademhalingAudioSource = gameObject.AddComponent<AudioSource>();
                ademhalingAudioSource.spatialBlend = 1.0f; // 3D geluid
                ademhalingAudioSource.minDistance = 0.5f;
                ademhalingAudioSource.maxDistance = 5.0f;
                ademhalingAudioSource.rolloffMode = AudioRolloffMode.Linear;
            }
        }

        // Genereer synthetische audio als fallback als er in de inspector geen bestanden zijn gekoppeld
        if (heartSoundClip == null)
            heartSoundClip = KrijgProceduralHeartbeat();

        if (breathingClip == null)
            breathingClip = KrijgProceduralBreathing();
    }

    public void ExamineerAirway() => SpeelAudio(airwayClip != null ? airwayClip : breathingClip);
    public void ExamineerBreathing() => SpeelAudio(breathingClip);

    public void SpeelStethoscopeGeluidAf()
    {
        if (ademhalingAudioSource == null)
            ademhalingAudioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        AudioClip clip = heartSoundClip != null ? heartSoundClip : (breathingClip != null ? breathingClip : airwayClip);
        if (clip != null)
        {
            ademhalingAudioSource.PlayOneShot(clip, 1.0f);
            Debug.Log("[PatientAudio] Stethoscoop auscultatie geluid afgespeeld!");
        }
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

    // ─── Procedural Audio Generatie (Garandeert dat de stethoscoop altijd hoorbaar is) ───

    private static AudioClip KrijgProceduralHeartbeat()
    {
        if (proceduralHeartbeatClip != null) return proceduralHeartbeatClip;

        int sampleRate = 44100;
        float duration = 1.0f; // 1 seconde per hartslagcyclus (~60-70 BPM)
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        // Genereer realistische "lub-dub" hartslag (2 lage doffe tonen)
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float val = 0f;

            // Lub (S1 puls rond t = 0.05s)
            if (t >= 0.05f && t <= 0.18f)
            {
                float p = (t - 0.05f) / 0.13f;
                float env = Mathf.Sin(p * Mathf.PI);
                val += Mathf.Sin(2f * Mathf.PI * 65f * t) * env * 0.9f;
            }

            // Dub (S2 puls rond t = 0.35s)
            if (t >= 0.35f && t <= 0.46f)
            {
                float p = (t - 0.35f) / 0.11f;
                float env = Mathf.Sin(p * Mathf.PI);
                val += Mathf.Sin(2f * Mathf.PI * 85f * t) * env * 0.7f;
            }

            data[i] = val;
        }

        proceduralHeartbeatClip = AudioClip.Create("ProceduralHeartbeat", samples, 1, sampleRate, false);
        proceduralHeartbeatClip.SetData(data, 0);
        return proceduralHeartbeatClip;
    }

    private static AudioClip KrijgProceduralBreathing()
    {
        if (proceduralBreathingClip != null) return proceduralBreathingClip;

        int sampleRate = 44100;
        float duration = 2.0f; // 2 seconden inademing/uitademing
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        // Zachte gefilterde ruis voor ademhaling
        float lastVal = 0f;
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Sin((t / duration) * Mathf.PI); // Zacht aanzwellen en afnemen
            float whiteNoise = (Random.value * 2f - 1f);
            lastVal = Mathf.Lerp(lastVal, whiteNoise, 0.05f); // Low-pass filter voor zacht ademgeruis
            data[i] = lastVal * envelope * 0.5f;
        }

        proceduralBreathingClip = AudioClip.Create("ProceduralBreathing", samples, 1, sampleRate, false);
        proceduralBreathingClip.SetData(data, 0);
        return proceduralBreathingClip;
    }
}
