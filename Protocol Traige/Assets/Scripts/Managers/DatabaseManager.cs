using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Singleton die alle communicatie met de .NET REST-API afhandelt.
/// Overleeft scene-loads via DontDestroyOnLoad.
/// 
/// VEREISTE API-ENDPOINTS:
///   POST  /api/login          { gebruikersnaam, wachtwoord }
///   POST  /api/registreer     { gebruikersnaam, email, wachtwoord }
///   POST  /api/sessie         { gebruikerId }           → { id }
///   POST  /api/patientcase    { PatientCaseDto }
///   PUT   /api/sessie/{id}    { eindLevel }
/// </summary>
public class DatabaseManager : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════
    //  SINGLETON
    // ══════════════════════════════════════════════════════════════════

    private static DatabaseManager _instance;

    public static DatabaseManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DatabaseManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("[DatabaseManager]");
                    _instance = go.AddComponent<DatabaseManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    // ══════════════════════════════════════════════════════════════════
    //  INSPECTOR VELDEN
    // ══════════════════════════════════════════════════════════════════

    [Header("API Instellingen")]
    [Tooltip("Basis-URL van de .NET API, bijv. http://localhost:5000/api")]
    public string apiUrl = "http://localhost:5000/api";

    [Header("Account (fallback als SessionData leeg is)")]
    public int gekoppeldeGebruikerId = 1;

    // ══════════════════════════════════════════════════════════════════
    //  PRIVÉ STATUS
    // ══════════════════════════════════════════════════════════════════

    private int  _huidigeSessieId  = -1;
    private bool _sessieActief     = false;

    public int  HuidigeSessieId => _huidigeSessieId;
    public bool SessieActief    => _sessieActief;

    // ══════════════════════════════════════════════════════════════════
    //  SESSIE BEHEER
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Start een nieuwe sessie voor de ingelogde gebruiker.
    /// Wordt aangeroepen vanuit MainMenuManager.OnStartKnopGedrukt().
    /// </summary>
    public void StartNieuweSessie()
    {
        // Gebruik altijd de ingelogde gebruiker uit SessionData wanneer beschikbaar
        if (SessionData.IsIngelogd)
            gekoppeldeGebruikerId = SessionData.GebruikerId;

        if (gekoppeldeGebruikerId <= 0)
        {
            Debug.LogError("[DatabaseManager] Geen geldig gebruikerId – log eerst in!");
            return;
        }

        // Reset vorige sessie-status
        _huidigeSessieId = -1;
        _sessieActief    = false;

        StartCoroutine(StartSessieRoutine());
    }

    private IEnumerator StartSessieRoutine()
    {
        string jsonBody = $"{{\"gebruikerId\": {gekoppeldeGebruikerId}}}";
        byte[] bodyRaw  = Encoding.UTF8.GetBytes(jsonBody);

        using UnityWebRequest request = new UnityWebRequest($"{apiUrl}/sessie", "POST");
        request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            SessieResponse response = JsonUtility.FromJson<SessieResponse>(request.downloadHandler.text);
            if (response != null && response.id > 0)
            {
                _huidigeSessieId = response.id;
                _sessieActief    = true;
                Debug.Log($"[DatabaseManager] ✅ Sessie gestart (Sessie ID: {_huidigeSessieId}, Gebruiker: {gekoppeldeGebruikerId})");
            }
            else
            {
                Debug.LogError($"[DatabaseManager] ❌ Server stuurde een ongeldig sessie-antwoord: {request.downloadHandler.text}");
            }
        }
        else
        {
            Debug.LogError($"[DatabaseManager] ❌ Fout bij starten sessie: {request.error} | URL: {apiUrl}/sessie");
        }
    }

    // ══════════════════════════════════════════════════════════════════
    //  PATIENT CASE OPSLAAN
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Slaat de uitkomst van een individuele patiënt op via de API.
    /// Wordt aangeroepen vanuit GameManager.ControleerInvoer().
    /// </summary>
    public void SlaPatientCaseOp(
        int level,
        Patient correcteData,
        NtsUrgentie correcteUrgentie,
        int inA, int inB, int inC, int inD, int inE,
        NtsUrgentie inUrg,
        int fouten, bool isCorrect, float tijd)
    {
        if (!_sessieActief || _huidigeSessieId <= 0)
        {
            Debug.LogWarning("[DatabaseManager] ⚠️ Geen actieve sessie – patient-case wordt NIET opgeslagen. Controleer of StartNieuweSessie() is aangeroepen.");
            return;
        }

        if (correcteData == null)
        {
            Debug.LogError("[DatabaseManager] ❌ correcteData (Patient) is null – kan niet opslaan.");
            return;
        }

        StartCoroutine(SlaCaseRoutine(level, correcteData, correcteUrgentie,
            inA, inB, inC, inD, inE, inUrg, fouten, isCorrect, tijd));
    }

    private IEnumerator SlaCaseRoutine(
        int level,
        Patient correcteData,
        NtsUrgentie correcteUrgentie,
        int inA, int inB, int inC, int inD, int inE,
        NtsUrgentie inUrg,
        int fouten, bool isCorrect, float tijd)
    {
        PatientCaseDto dto = new PatientCaseDto
        {
            sessieId              = _huidigeSessieId,
            level                 = level,
            correcteAirway        = correcteData.airway,
            correcteBreathing     = correcteData.breathing,
            correcteCirculation   = correcteData.circulation,
            correcteDisability    = correcteData.disabillity,
            correcteExposure      = correcteData.exposure,
            correcteUrgentie      = (int)correcteUrgentie,
            invoerAirway          = inA,
            invoerBreathing       = inB,
            invoerCirculation     = inC,
            invoerDisability      = inD,
            invoerExposure        = inE,
            invoerUrgentie        = (int)inUrg,
            aantalFouten          = fouten,
            isCorrect             = isCorrect,
            verstrekenTijdSeconden = tijd
        };

        string jsonBody = JsonUtility.ToJson(dto);
        byte[] bodyRaw  = Encoding.UTF8.GetBytes(jsonBody);

        using UnityWebRequest request = new UnityWebRequest($"{apiUrl}/patientcase", "POST");
        request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[DatabaseManager] ✅ Patient-case opgeslagen (Level {level}, Correct: {isCorrect}, Fouten: {fouten})");
        }
        else
        {
            Debug.LogError($"[DatabaseManager] ❌ Fout bij opslaan patient-case: {request.error}");
            Debug.LogError($"[DatabaseManager]    Body verstuurd: {jsonBody}");
        }
    }

    // ══════════════════════════════════════════════════════════════════
    //  SESSIE AFSLUITEN
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Sluit de huidige sessie af met het behaalde eindlevel.
    /// Wordt aangeroepen vanuit TriageTimerUI.PatientAfgehandeld() bij level-up.
    /// </summary>
    public void SluitSessie(int eindLevel)
    {
        if (!_sessieActief || _huidigeSessieId <= 0)
        {
            Debug.LogWarning("[DatabaseManager] ⚠️ SluitSessie aangeroepen maar er is geen actieve sessie.");
            return;
        }

        StartCoroutine(SluitSessieRoutine(eindLevel));
    }

    private IEnumerator SluitSessieRoutine(int eindLevel)
    {
        string jsonBody = $"{{\"eindLevel\": {eindLevel}}}";
        byte[] bodyRaw  = Encoding.UTF8.GetBytes(jsonBody);

        using UnityWebRequest request = new UnityWebRequest($"{apiUrl}/sessie/{_huidigeSessieId}", "PUT");
        request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"[DatabaseManager] ✅ Sessie {_huidigeSessieId} afgesloten op level {eindLevel}");
            _sessieActief    = false;
            _huidigeSessieId = -1;
        }
        else
        {
            Debug.LogError($"[DatabaseManager] ❌ Fout bij afsluiten sessie: {request.error}");
        }
    }

    // ══════════════════════════════════════════════════════════════════
    //  APPLICATIE STOPPEN
    // ══════════════════════════════════════════════════════════════════

    private void OnApplicationQuit()
    {
        if (_sessieActief && _huidigeSessieId > 0)
        {
            // Synchrone best-effort afsluiting (geen yield mogelijk in OnApplicationQuit)
            Debug.Log("[DatabaseManager] Applicatie sluit – sessie wordt afgesloten.");
            StartCoroutine(SluitSessieRoutine(1));
        }
    }
}

// ══════════════════════════════════════════════════════════════════════
//  DATA TRANSFER OBJECTS
// ══════════════════════════════════════════════════════════════════════

[System.Serializable]
public class SessieResponse
{
    public int id;
}

[System.Serializable]
public class PatientCaseDto
{
    public int   sessieId;
    public int   level;
    public int   correcteAirway;
    public int   correcteBreathing;
    public int   correcteCirculation;
    public int   correcteDisability;
    public int   correcteExposure;
    public int   correcteUrgentie;
    public int   invoerAirway;
    public int   invoerBreathing;
    public int   invoerCirculation;
    public int   invoerDisability;
    public int   invoerExposure;
    public int   invoerUrgentie;
    public int   aantalFouten;
    public bool  isCorrect;
    public float verstrekenTijdSeconden;
}
