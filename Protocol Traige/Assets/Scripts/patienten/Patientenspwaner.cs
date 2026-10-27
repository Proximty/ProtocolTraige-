using UnityEngine;

/// <summary>
/// Spawnt patienten en koppelt ze aan de GameManager.
/// PERFORMANCE: StartNieuweSessie() verwijderd - dit doet MainMenuManager al.
/// Dubbele sessie-start veroorzaakte onnodige API-calls en een verkeerde sessie-ID.
/// </summary>
public class PatientSpawner : MonoBehaviour
{
    [Header("Prefabs en Locaties")]
    public GameObject  patientPrefab;
    public Transform   spawnPunt;
    public Transform   bedTransform;
    public BoxCollider bedCollider;
    public Transform   exitPunt;

    [Header("Referenties")]
    public GameManager     gameManager;
    public TriageTimerUI   timerUI;

    private GameObject _huidigePatientInstance;

    private void Start()
    {
        // VERWIJDERD: DatabaseManager.Instance.StartNieuweSessie()
        // De sessie wordt al gestart door MainMenuManager VOOR het laden van deze scene.
        // Een dubbele aanroep hier maakte een nieuwe sessie aan met ID X+1
        // terwijl de echte sessie ID X al liep.

        SpawnNieuwePatient();
    }

    public void SpawnNieuwePatient()
    {
        if (timerUI != null)
            timerUI.HerstartTimer();

        if (patientPrefab == null || spawnPunt == null) return;

        // Verwijder vorige patient
        if (_huidigePatientInstance != null)
            Destroy(_huidigePatientInstance);

        // Maak nieuwe patient aan
        _huidigePatientInstance = Instantiate(patientPrefab, spawnPunt.position, spawnPunt.rotation);

        PatientMovement movement    = _huidigePatientInstance.GetComponent<PatientMovement>();
        PatientBehavior behavior    = _huidigePatientInstance.GetComponent<PatientBehavior>();
        Patient         patientData = behavior != null ? behavior.patientData : _huidigePatientInstance.GetComponent<Patient>();

        // Initialiseer beweging
        if (movement != null)
            movement.InitialiseerPatient(bedTransform, bedCollider, exitPunt);

        // Koppel aan GameManager
        if (gameManager != null)
        {
            gameManager.huidigePatient   = patientData;
            gameManager.patientMovement  = movement;
            gameManager.patientBehavior  = behavior;
        }
    }
}