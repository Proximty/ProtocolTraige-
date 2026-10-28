using UnityEngine;

public class PatientSpawner : MonoBehaviour
{
    [Header("Prefabs & Locaties")]
    public GameObject patientPrefab;
    public Transform spawnPunt;
    public Transform bedTransform;
    public BoxCollider bedCollider;
    public Transform exitPunt;

    [Header("GameManager Reference")]
    public GameManager gameManager;

    private GameObject huidigePatientInstance;
    public TriageTimerUI timerUI;
    void Start()
    {
        SpawnNieuwePatient();
    }
    public void SpawnNieuwePatient()
    {
        if (timerUI != null)
        {
            timerUI.HerstartTimer();
        }
        if (patientPrefab == null || spawnPunt == null) return;

        if (huidigePatientInstance != null)
            Destroy(huidigePatientInstance);

        // 1. Instantieer de patient
        huidigePatientInstance = Instantiate(patientPrefab, spawnPunt.position, spawnPunt.rotation);

        // 2. Haal de component op
        PatientMovement movement = huidigePatientInstance.GetComponent<PatientMovement>();
        Patient patientData = huidigePatientInstance.GetComponent<Patient>();

        // 3. VUL DE VELDEN DIRECT IN
        if (movement != null)
        {
            movement.InitialiseerPatiënt(bedTransform, bedCollider, exitPunt);
        }

        // 4. Koppel aan GameManager
        if (gameManager != null)
        {
            gameManager.huidigePatient = patientData;
            gameManager.patientMovement = movement;
        }
    }
}