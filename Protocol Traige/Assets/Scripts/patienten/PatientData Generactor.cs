using UnityEngine;

public static class PatientDataGenerator
{
    public static Patient GenereerWillekeurigePatient()
    {
        // Maak ScriptableObject aan en initialiseer de ABCDE-waarden
        Patient nieuwePatient = ScriptableObject.CreateInstance<Patient>();
        nieuwePatient.Initialize();

        return nieuwePatient;
    }

    public static NtsUrgentie GenereerWillekeurigeUrgentie()
    {
        // Kiest alleen een van de wandelende urgenties (U1, U2, U3) bij de start
        NtsUrgentie[] wandelUrgenties = new NtsUrgentie[] {
        NtsUrgentie.U1_Levensbedreigend,
        NtsUrgentie.U2_Spoed,
        NtsUrgentie.U3_Dringend
    };

        return wandelUrgenties[Random.Range(0, wandelUrgenties.Length)];
    }
}