using UnityEngine;

[CreateAssetMenu(fileName = "NieuwePatientData", menuName = "Medical/Patient Data")]
public class Patient : ScriptableObject
{
    [Header("ABCDE Waarden")]
    [SerializeField] public int Airway;
    [SerializeField] public int Breathing;
    [SerializeField] public int Cirulation;
    [SerializeField] public int Disabillity;
    [SerializeField] public int Exposure;

    [Header("Scenario Info")]
    public string titel;
    public string klacht;
    public int pijn;
    public int gewensteUrgentie = -1;
    public string disabilityReactie;
    public string exposureReactie;

    // Public getters
    public int airway => Airway;
    public int breathing => Breathing;
    public int circulation => Cirulation;
    public int disabillity => Disabillity;
    public int exposure => Exposure;

    public void Initialize()
    {
        Airway = Random.Range(1, 11);
        Breathing = Random.Range(1, 11);
        Cirulation = Random.Range(1, 11);
        Disabillity = Random.Range(1, 11);
        Exposure = Random.Range(1, 11);
        pijn = Random.Range(0, 11);
        titel = "Standaard Patiënt";
        klacht = "Algemene controle";
    }

    public void VulMetScenario(ScenarioItemDto scenario)
    {
        if (scenario == null) return;

        Airway = scenario.airway;
        Breathing = scenario.breathing;
        Cirulation = scenario.circulation;
        Disabillity = scenario.disability;
        Exposure = scenario.exposure;
        pijn = scenario.pijn;
        titel = scenario.titel;
        klacht = scenario.klacht;
        gewensteUrgentie = scenario.urgentie;
        disabilityReactie = scenario.disabilityReactie;
        exposureReactie = scenario.exposureReactie;
    }
}
