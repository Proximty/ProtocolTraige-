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

        // 2. Haal de componenten op
        PatientMovement movement = huidigePatientInstance.GetComponent<PatientMovement>();
        PatientBehavior behavior = huidigePatientInstance.GetComponent<PatientBehavior>();

        // 3. Probeer medisch scenario op te halen uit de database voor dit level (Database -> Game)
        int level = (timerUI != null) ? timerUI.huidigLevel : 1;
        if (DatabaseManager.Instance != null && DatabaseManager.Instance.isVerbondenMetServer)
        {
            DatabaseManager.Instance.HaalScenarioOp(level, (scenario) =>
            {
                if (scenario != null && behavior != null)
                {
                    behavior.PasScenarioToe(scenario);
                    if (gameManager != null)
                        gameManager.huidigePatient = behavior.patientData;
                }
            });
        }

        // 4. Initialiseer beweging en koppel exit event zodat de volgende patient arriveert
        if (movement != null)
        {
            movement.InitialiseerPatiënt(bedTransform, bedCollider, exitPunt);
            movement.OnAangekomenBijExit.AddListener(() =>
            {
                Invoke(nameof(SpawnNieuwePatient), 2.0f);
            });
        }

        // 5. Koppel direct aan GameManager
        if (gameManager != null)
        {
            gameManager.huidigePatient = (behavior != null) ? behavior.patientData : null;
            gameManager.patientBehavior = behavior;
            gameManager.patientMovement = movement;
            gameManager.timerUI = timerUI;
        }
    }
}
