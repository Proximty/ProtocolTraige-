using UnityEngine;

public enum NtsUrgentie
{
    U0_Reanimatie = 0,       // Uitval ABCDE (Onmiddellijk)
    U1_Levensbedreigend = 1, // Instabiele ABCDE (< 15 min)
    U2_Spoed = 2,            // Bedreiging ABCDE (< 1 uur)
    U3_Dringend = 3,         // Kans op schade (< 3 uur)
    U4_NietDringend = 4,     // Verwaarloosbare kans op schade (< 24 uur)
    U5_Advies = 5            // Geen kans op schade
}

public class GameManager : MonoBehaviour
{
    [Header("Patient & Behavior References")]
    public Patient huidigePatient;
    public PatientBehavior patientBehavior;
    public PatientMovement patientMovement;

    [Header("Bed Instellingen")]
    public Transform bedTransform;
    public BoxCollider bedCollider;
    [Header("UI & Timer Reference")]
    public TriageTimerUI timerUI;

    public void ControleerInvoer(int airway, int breathing, int circulation, int disability, int exposure, int pijn, NtsUrgentie gekozenUrgentie)
    {
        if (huidigePatient == null) return;

        int fouten = 0;

        if (airway != huidigePatient.airway) fouten++;
        if (breathing != huidigePatient.breathing) fouten++;
        if (circulation != huidigePatient.circulation) fouten++;
        if (disability != huidigePatient.disabillity) fouten++;
        if (exposure != huidigePatient.exposure) fouten++;

        int totaleScore = huidigePatient.airway + huidigePatient.breathing + huidigePatient.circulation + huidigePatient.disabillity + huidigePatient.exposure;
        NtsUrgentie correcteUrgentie = BepaalNtsKlasse(totaleScore);

        if (gekozenUrgentie != correcteUrgentie) fouten++;

        bool allesGoed = (fouten == 0);
        string titel = allesGoed ? "GOED BEOORDEELD" : "FOUTEN GEMAAKT";
        string bericht = allesGoed
            ? "Alle ABCDE-scores en NTS-urgentie zijn correct!"
            : $"Je hebt {fouten} fout(en) gemaakt in de beoordeling.";

        // Geef door aan de Timer dat de patiënt is afgehandeld
        if (timerUI != null)
        {
            timerUI.PatientAfgehandeld(allesGoed, titel, bericht);
        }

        if (patientBehavior != null) patientBehavior.StelGedragIn(correcteUrgentie);
        if (patientMovement != null) patientMovement.WijsUrgentieEnBedToe(correcteUrgentie, bedTransform, bedCollider);
    }

    private NtsUrgentie BepaalNtsKlasse(int score)
    {
        if (score >= 42) return NtsUrgentie.U0_Reanimatie;
        if (score >= 34) return NtsUrgentie.U1_Levensbedreigend;
        if (score >= 26) return NtsUrgentie.U2_Spoed;
        if (score >= 18) return NtsUrgentie.U3_Dringend;
        if (score >= 10) return NtsUrgentie.U4_NietDringend;

        return NtsUrgentie.U5_Advies;
    }
    public void RondPatientAf()
    {
        if (patientMovement != null)
        {
            patientMovement.StartLopenNaarExit();
        }
    }

}