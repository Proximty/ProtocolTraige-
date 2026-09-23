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

    /// <summary>
    /// Pas het gedrag en de animatie aan op basis van de berekende NTS-urgentie.
    /// </summary>
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

    /// <summary>
    /// Geeft de gesproken reactie van de patiënt voor Disability (1e persoon)
    /// </summary>
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

    /// <summary>
    /// Geeft de gesproken reactie van de patiënt voor Exposure (1e persoon)
    /// </summary>
    public string GetExposureReactie()
    {
        if (patientData == null) return "...";

        int score = patientData.exposure;

        if (score >= 8)
            return "\"Mijn lichaamsgevoel is gewoon normaal, niet te koud en niet te warm.\"";
        if (score >= 5)
            return "\"Brr... ik heb het ontzettend koud! Mijn benen en armen doen ook pijn van de schaafwonden.\"";

        return "\"Ik lig helemaal te rillen van de kou... alles doet pijn als ik beweeg!\"";
    }
}