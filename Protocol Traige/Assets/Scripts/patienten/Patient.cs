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
    }
}
