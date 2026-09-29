using JetBrains.Annotations;
using UnityEngine;

public class PatientBehavior : MonoBehaviour
{
    [Header("Patiënt Data")]
    public Patient patientData;
    public NtsUrgentie toegewezenUrgentie;

    [Header("Animation Settings")]
    public Animator patientAnimator;
    public string animatieParameterLiggen = "IsLiggen"; // Bool parameter in Animator
    public string animatieParameterLopen = "IsLopen";   // Bool parameter in Animator

    [Header("Audio (Airway & Breathing)")]
    public AudioSource ademhalingAudioSource;
    public AudioClip airwayClip;
    public AudioClip breathingClip;

    [Header("Stethoscope Audio Settings")]
    public AudioClip heartSoundClip; // Eventueel extra audio clip voor hartslag

    [Header("Reanimatie (CPR) Settings")]
    public int benodigdeCompressies = 30;
    public int huidigeCompressies = 0;
    public bool isGereanimeerd = false;

    /// <summary>
    /// Wordt aangeroepen bij elke geldige borstcompressie (VR Controller).
    /// Enkel mogelijk bij U0_Reanimatie.
    /// </summary>
    public void RegistreerCompressie()
    {
        // 1. Controleer of de patiënt daadwerkelijk urgentie U0 (Reanimatie) heeft
        if (toegewezenUrgentie != NtsUrgentie.U0_Reanimatie)
        {
            Debug.LogWarning("Compressie genegeerd: Patiënt heeft geen U0_Reanimatie urgentie!");
            return;
        }

        // 2. Voorkom extra compressies als de patiënt al geslaagd is
        if (isGereanimeerd) return;

        huidigeCompressies++;
        Debug.Log($"Compressie uitgevoerd! Totaal: {huidigeCompressies}/{benodigdeCompressies}");

        // Speel bij elke drukbeweging een kort ademhalings-/feedbackgeluid af
        if (ademhalingAudioSource != null && breathingClip != null)
        {
            ademhalingAudioSource.PlayOneShot(breathingClip, 0.4f);
        }

        // 3. Controleer of de reanimatie voltooid is
        if (huidigeCompressies >= benodigdeCompressies)
        {
            ReanimatieSucces();
        }
    }

    private void ReanimatieSucces()
    {
        isGereanimeerd = true;
        Debug.Log("Patiënt is succesvol gereanimeerd!");

        // Pas eventueel animatie / status van de patiënt aan
        if (patientAnimator != null)
        {
            patientAnimator.SetBool("IsGereanimeerd", true);
        }
    }

    /// <summary>
    /// Speelt de ademhaling/hartslag af wanneer de stethoscoop-knop wordt ingedrukt.
    /// </summary>
    public void SpeelStethoscopeGeluidAf()
    {
        if (ademhalingAudioSource == null)
        {
            Debug.LogWarning("Geen ademhalingAudioSource toegewezen op PatientBehavior!");
            return;
        }

        // Kies de clip die je wilt afspelen (bijv. breathingClip of heartSoundClip)
        AudioClip clipOmTeSpelen = heartSoundClip != null ? heartSoundClip : (breathingClip != null ? breathingClip : airwayClip);

        if (clipOmTeSpelen != null)
        {
            ademhalingAudioSource.PlayOneShot(clipOmTeSpelen);
            Debug.Log($"Stethoscope geluid afgespeeld: {clipOmTeSpelen.name}");
        }
    }

    public void StelGedragIn(NtsUrgentie urgentie)
    {
        toegewezenUrgentie = urgentie;

        if (patientAnimator == null)
        {
            Debug.LogError("Geen Animator gekoppeld aan PatientBehavior!");
            return;
        }

        Debug.Log($"Gedrag instellen gebaseerd op urgentie: {urgentie}");

        switch (urgentie)
        {
            // --- Rustig blijven liggen voor onderzoek ---
            case NtsUrgentie.U0_Reanimatie:   // Bewusteloos/Stil
            case NtsUrgentie.U4_NietDringend:
            case NtsUrgentie.U5_Advies:
                ZetAnimatieStaat(true, false); // Liggen = Aan, Lopen = Uit
                Debug.Log("Patiënt ligt rustig.");
                break;

            // --- Onrustig gedrag, proberen te lopen/bewegen ---
            case NtsUrgentie.U1_Levensbedreigend: // Benauwd/Paniek
            case NtsUrgentie.U2_Spoed:           // Veel Pijn
            case NtsUrgentie.U3_Dringend:        // Onrustig
                ZetAnimatieStaat(false, true); // Liggen = Uit, Lopen = Aan
                Debug.Log("Patiënt is onrustig en probeert te bewegen/lopen.");
                break;
        }
    }

    /// <summary>
    /// Hulpfunctie om de juiste bools in de Animator Controller te zetten.
    /// </summary>
    private void ZetAnimatieStaat(bool isLiggen, bool isLopen)
    {
        if (patientAnimator != null)
        {
            patientAnimator.SetBool(animatieParameterLiggen, isLiggen);
            patientAnimator.SetBool(animatieParameterLopen, isLopen);
        }
    }

    // =========================================================================
    // EXAMINERING METHODEN (Worden aangeroepen door audio-triggers / UI)
    // =========================================================================

    // --- AIRWAY & BREATHING AUDIO ---
    public void ExamineerAirway()
    {
        SpeelAudio(airwayClip);
    }

    public void ExamineerBreathing()
    {
        SpeelAudio(breathingClip);
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

    // --- DISABILITY & EXPOSURE DIALOOG (Aangeroepen door OnderzoekMenuUI) ---

    public string GetDisabilityReactie()
    {
        if (patientData == null) return "...";

        int score = patientData.disabillity;

        if (score >= 8)
            return "\"Ik voel me prima hoor! Ik weet wie ik ben en waar ik ben.\"";
        if (score >= 5)
            return "\"Uhm... mijn hoofd doet zo'n pijn... Alles voelt een beetje wazig en ik ben erg duizelig.\"";

        return "\"*Mompelt heel zachtjes*... Ik... ik krijg mijn ogen amper open...\"";
    }

    public string GetExposureReactie()
    {
        if (patientData == null) return "...";

        int score = patientData.exposure;

        if (score >= 8)
            return "\"Mijn lichaamsgevoel is gewoon normaal, niet te koud en niet te warm.\"";
        if (score >= 5)
            return "\"Brr... ik heb het ontzettend koud! Mijn benen en armen doen ook pijn van de schaafwonden.\"";

        return "\"Ik lig helemaal te rillingen van de kou... alles doet pijn als ik beweeg!\"";
    }
}