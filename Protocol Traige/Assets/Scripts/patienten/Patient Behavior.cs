using UnityEngine;

public class PatientBehavior : MonoBehaviour
{
    [Header("Patiënt Data")]
    public Patient patientData;
    public NtsUrgentie toegewezenUrgentie;

    [Header("Reanimatie (CPR) Settings")]
    public int benodigdeCompressies = 30;
    public int huidigeCompressies = 0;
    public bool isGereanimeerd = false;

    // Sub-componenten
    private PatientMovement movement;
    private PatientAnimation animationController;
    private PatientAudio audioController;

    void Awake()
    {
        movement = GetComponent<PatientMovement>();
        animationController = GetComponent<PatientAnimation>();
        audioController = GetComponent<PatientAudio>();

        // AUTOMATISCHE DATA GENERATIE:
        // Als er in de Inspector geen Patient asset is toegewezen, genereer er dan een runtime!
        if (patientData == null)
        {
            patientData = PatientDataGenerator.GenereerWillekeurigePatient();
            toegewezenUrgentie = PatientDataGenerator.GenereerWillekeurigeUrgentie();
        }
    }

    void Start()
    {
        if (patientData != null)
        {
            Debug.Log($"<color=green>[PATIËNT DATA GELADEN]</color> " +
                      $"Airway: {patientData.airway} | " +
                      $"Breathing: {patientData.breathing} | " +
                      $"Circulation: {patientData.circulation} | " +
                      $"Disability: {patientData.disabillity} | " +
                      $"Exposure: {patientData.exposure}");
        }

        // Laat de patiënt ALTIJD eerst lopen naar het bed bij de start
        if (movement != null && movement.bedTransform != null)
        {
            movement.StartLopenNaarBed();
        }
    }

    public void StelGedragIn(NtsUrgentie urgentie)
    {
        toegewezenUrgentie = urgentie;

        switch (urgentie)
        {
            case NtsUrgentie.U0_Reanimatie:
            case NtsUrgentie.U4_NietDringend:
            case NtsUrgentie.U5_Advies:
                if (movement != null) movement.StopEnGaLiggen();
                break;

            case NtsUrgentie.U1_Levensbedreigend:
            case NtsUrgentie.U2_Spoed:
            case NtsUrgentie.U3_Dringend:
                if (movement != null) movement.StartLopenNaarBed();
                break;
        }
    }

    public void RegistreerCompressie()
    {
        if (toegewezenUrgentie != NtsUrgentie.U0_Reanimatie || isGereanimeerd) return;

        huidigeCompressies++;

        if (audioController != null) audioController.SpeelCompressieGeluid();

        if (huidigeCompressies >= benodigdeCompressies)
        {
            ReanimatieSucces();
        }
    }

    private void ReanimatieSucces()
    {
        isGereanimeerd = true;
       
        Debug.Log("Patiënt is succesvol gereanimeerd!");
    }

    // --- Audio Doorverwijzingen (voor overige scripts in de scene) ---
    public void SpeelStethoscopeGeluidAf()
    {
        if (audioController != null) audioController.SpeelStethoscopeGeluidAf();
    }

    public void ExamineerAirway()
    {
        if (audioController != null) audioController.ExamineerAirway();
    }

    public void ExamineerBreathing()
    {
        if (audioController != null) audioController.ExamineerBreathing();
    }

    // --- Dialoog Opties ---
    public string GetDisabilityReactie() => patientData != null && patientData.disabillity >= 8 ? "\"Ik voel me prima hoor!\"" : "\"Ik ben erg duizelig...\"";
    public string GetExposureReactie() => patientData != null && patientData.exposure >= 8 ? "\"Mijn lichaamsgevoel is normaal.\"" : "\"Ik heb het koud...\"";
}
