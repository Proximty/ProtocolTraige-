using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// ─── Data Transfer Objecten ──────────────────────────────────────────────────

[System.Serializable]
public class LoginRequestDto
{
    public string Gebruikersnaam;
    public string Wachtwoord;
}

[System.Serializable]
public class RegisterRequestDto
{
    public string Gebruikersnaam;
    public string Wachtwoord;
    public string VolledigeNaam;
}

[System.Serializable]
public class StartSessieRequestDto
{
    public int? GebruikerId;
}

[System.Serializable]
public class SluitSessieRequestDto
{
    public int EindLevel;
}

[System.Serializable]
public class PatientCaseRequestDto
{
    public int SessieId;
    public int Level;
    public int CorrecteAirway;
    public int CorrecteBreathing;
    public int CorrecteCirculation;
    public int CorrecteDisability;
    public int CorrecteExposure;
    public int CorrecteUrgentie;
    public int InvoerAirway;
    public int InvoerBreathing;
    public int InvoerCirculation;
    public int InvoerDisability;
    public int InvoerExposure;
    public int InvoerUrgentie;
    public int AantalFouten;
    public bool IsCorrect;
    public float VerstrekenTijdSeconden;
}

[System.Serializable]
public class GebruikerData
{
    public int id;
    public string gebruikersnaam;
    public string volledigeNaam;
    public string rol;
}

[System.Serializable]
public class AuthResponse
{
    public bool succes;
    public string melding;
    public GebruikerData gebruiker;
    public int id;
}

[System.Serializable]
public class SessieResponse
{
    public int id;
    public int Id;
    public int sessieId;

    public int KrijgId()
    {
        if (id > 0) return id;
        if (Id > 0) return Id;
        if (sessieId > 0) return sessieId;
        return -1;
    }
}

[System.Serializable]
public class CategoriePercentagesDto
{
    public float airway;
    public float breathing;
    public float circulation;
    public float disability;
    public float exposure;
    public float urgentie;
}

[System.Serializable]
public class GebruikerStatsDto
{
    public bool heeftData;
    public int totaalSessies;
    public int totaalCases;
    public int totaalCorrect;
    public float overallAccuratessePct;
    public float gemiddeldeTijdSeconden;
    public int hoogsteLevel;
    public CategoriePercentagesDto categoriePercentages;
}

[System.Serializable]
public class ScenarioItemDto
{
    public int id;
    public int level;
    public string titel;
    public string klacht;
    public int airway;
    public int breathing;
    public int circulation;
    public int disability;
    public int exposure;
    public int pijn;
    public int urgentie;
    public string disabilityReactie;
    public string exposureReactie;
}

public static class JsonArrayHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{\"items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper != null ? wrapper.items : new T[0];
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}

[System.Serializable]
public class LokaalAccount
{
    public string gebruikersnaam;
    public string wachtwoord;
    public string volledigeNaam;
    public string rol;
}

[System.Serializable]
public class LokaleAccountLijst
{
    public List<LokaalAccount> accounts = new List<LokaalAccount>();
}

/// <summary>
/// Beheert de communicatie met de .NET REST-API en MySQL Database.
/// Garandeert bovendien offline werking via lokale opslag indien de server tijdelijk niet bereikbaar is.
/// </summary>
public class DatabaseManager : MonoBehaviour
{
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

    [Header("Server Instellingen")]
    [Tooltip("Primaire URL (.NET Backend op localhost:5000/api)")]
    public string serverUrl = "http://localhost:5000/api";

    [Tooltip("Secundaire URL voor standalone Meta Quest (lokaal netwerk IP-adres van je PC)")]
    public string questFallbackUrl = "http://10.16.31.55:5000/api";

    [Header("Status")]
    public bool isVerbondenMetServer = false;
    public bool isOfflineModus = false;

    private int huidigeSessieId = -1;
    private string actieveServerUrl = "http://localhost:5000/api";
    private const string LOKALE_ACCOUNTS_KEY = "LOKALE_DATABASE_ACCOUNTS";

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Bepaal de meest geschikte start-URL (op Meta Quest is localhost de headset zelf)
#if UNITY_ANDROID && !UNITY_EDITOR
            actieveServerUrl = !string.IsNullOrEmpty(questFallbackUrl) ? questFallbackUrl : serverUrl;
#else
            actieveServerUrl = !string.IsNullOrEmpty(serverUrl) ? serverUrl : "http://localhost:5000/api";
#endif
            InitialiseerStandaardAccounts();
            StartCoroutine(InitialiseerEnBlijfVerbinden());
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private string CleanUrl(string baseUrl)
    {
        if (string.IsNullOrEmpty(baseUrl)) return "";
        baseUrl = baseUrl.Trim();
        if (!baseUrl.EndsWith("/")) baseUrl += "/";
        return baseUrl;
    }

    // ─── Automatische Verbindingslus ──────────────────────────────────────────

    private IEnumerator InitialiseerEnBlijfVerbinden()
    {
        while (true)
        {
            yield return StartCoroutine(TestServerVerbinding(actieveServerUrl, (succes) =>
            {
                if (succes)
                {
                    isVerbondenMetServer = true;
                    isOfflineModus = false;
                }
                else
                {
                    // Probeer alternatieve URL indien geconfigureerd
                    string fallback = (actieveServerUrl == serverUrl) ? questFallbackUrl : serverUrl;
                    if (!string.IsNullOrEmpty(fallback) && fallback != actieveServerUrl)
                    {
                        StartCoroutine(TestServerVerbinding(fallback, (fallbackSucces) =>
                        {
                            if (fallbackSucces)
                            {
                                actieveServerUrl = fallback;
                                isVerbondenMetServer = true;
                                isOfflineModus = false;
                            }
                            else
                            {
                                isVerbondenMetServer = false;
                                isOfflineModus = true;
                            }
                        }));
                    }
                    else
                    {
                        isVerbondenMetServer = false;
                        isOfflineModus = true;
                    }
                }
            }));

            // Controleer elke 15 seconden op de achtergrond
            yield return new WaitForSeconds(15f);
        }
    }

    private IEnumerator TestServerVerbinding(string url, Action<bool> callback)
    {
        string healthUrl = CleanUrl(url) + "health";
        using (UnityWebRequest www = UnityWebRequest.Get(healthUrl))
        {
            www.timeout = 3;
            yield return www.SendWebRequest();

            bool gelukt = (www.result == UnityWebRequest.Result.Success);
            callback?.Invoke(gelukt);
        }
    }

    // ─── Authenticatie (Login & Registratie) ───────────────────────────────────

    public void Login(string gebruikersnaam, string wachtwoord, Action<bool, string> callback)
    {
        StartCoroutine(LoginRoutine(gebruikersnaam, wachtwoord, callback));
    }

    private IEnumerator LoginRoutine(string gebruikersnaam, string wachtwoord, Action<bool, string> callback)
    {
        if (!string.IsNullOrEmpty(actieveServerUrl))
        {
            var reqObj = new LoginRequestDto { Gebruikersnaam = gebruikersnaam, Wachtwoord = wachtwoord };
            string jsonBody = JsonUtility.ToJson(reqObj);
            string loginUrl = CleanUrl(actieveServerUrl) + "auth/login";

            using (UnityWebRequest www = new UnityWebRequest(loginUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.timeout = 4;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AuthResponse resp = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);
                    if (resp != null && resp.succes && resp.gebruiker != null)
                    {
                        PlayerPrefs.SetInt("IngelogdeGebruikerId", resp.gebruiker.id);
                        PlayerPrefs.SetString("IngelogdeGebruiker", resp.gebruiker.gebruikersnaam);
                        PlayerPrefs.SetString("IngelogdeVolledigeNaam", resp.gebruiker.volledigeNaam);
                        PlayerPrefs.SetString("IngelogdeRol", resp.gebruiker.rol);
                        PlayerPrefs.Save();

                        isVerbondenMetServer = true;
                        isOfflineModus = false;
                        SlaLokaalAccountOp(gebruikersnaam, wachtwoord, resp.gebruiker.volledigeNaam, resp.gebruiker.rol);
                        Debug.Log($"[DatabaseManager] Succesvol ingelogd via MySQL database als '{resp.gebruiker.gebruikersnaam}' (ID: {resp.gebruiker.id})");
                        callback?.Invoke(true, "");
                        yield break;
                    }
                }
                else if (www.result == UnityWebRequest.Result.ProtocolError && !string.IsNullOrEmpty(www.downloadHandler.text))
                {
                    // Server reageerde met expliciete fout (bijv. 400 Bad Request: onjuist wachtwoord)
                    try
                    {
                        AuthResponse resp = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);
                        if (resp != null && !string.IsNullOrEmpty(resp.melding))
                        {
                            isVerbondenMetServer = true;
                            callback?.Invoke(false, resp.melding);
                            yield break;
                        }
                    }
                    catch { }
                }
            }
        }

        // Fallback: controleer lokaal account
        if (ValideerLokaalAccount(gebruikersnaam, wachtwoord))
        {
            PlayerPrefs.SetInt("IngelogdeGebruikerId", 1);
            PlayerPrefs.SetString("IngelogdeGebruiker", gebruikersnaam);
            PlayerPrefs.SetString("IngelogdeVolledigeNaam", gebruikersnaam);
            PlayerPrefs.SetString("IngelogdeRol", "Trainee SEH");
            PlayerPrefs.Save();
            isOfflineModus = true;
            Debug.Log($"[DatabaseManager] Inloggen geslaagd via Lokale Modus voor '{gebruikersnaam}'.");
            callback?.Invoke(true, "");
        }
        else
        {
            callback?.Invoke(false, "Gebruikersnaam of wachtwoord onjuist.");
        }
    }

    public void Registreer(string gebruikersnaam, string wachtwoord, Action<bool, string> callback)
    {
        Registreer(gebruikersnaam, wachtwoord, gebruikersnaam, callback);
    }

    public void Registreer(string gebruikersnaam, string wachtwoord, string volledigeNaam, Action<bool, string> callback)
    {
        StartCoroutine(RegistreerRoutine(gebruikersnaam, wachtwoord, volledigeNaam, callback));
    }

    private IEnumerator RegistreerRoutine(string gebruikersnaam, string wachtwoord, string volledigeNaam, Action<bool, string> callback)
    {
        if (string.IsNullOrEmpty(volledigeNaam)) volledigeNaam = gebruikersnaam;

        if (!string.IsNullOrEmpty(actieveServerUrl))
        {
            var reqObj = new RegisterRequestDto { Gebruikersnaam = gebruikersnaam, Wachtwoord = wachtwoord, VolledigeNaam = volledigeNaam };
            string jsonBody = JsonUtility.ToJson(reqObj);
            string regUrl = CleanUrl(actieveServerUrl) + "auth/register";

            using (UnityWebRequest www = new UnityWebRequest(regUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.timeout = 4;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AuthResponse resp = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);
                    if (resp != null && resp.succes)
                    {
                        isVerbondenMetServer = true;
                        isOfflineModus = false;
                        SlaLokaalAccountOp(gebruikersnaam, wachtwoord, volledigeNaam, "Trainee SEH");
                        Debug.Log($"[DatabaseManager] Account '{gebruikersnaam}' geregistreerd in MySQL database.");
                        callback?.Invoke(true, "");
                        yield break;
                    }
                }
            }
        }

        // Lokale registratie fallback
        isOfflineModus = true;
        if (BestaatLokaalAccount(gebruikersnaam))
        {
            callback?.Invoke(false, "Deze gebruikersnaam is al in gebruik.");
            yield break;
        }

        SlaLokaalAccountOp(gebruikersnaam, wachtwoord, volledigeNaam, "Trainee SEH");
        callback?.Invoke(true, "");
    }

    // ─── Sessies ──────────────────────────────────────────────────────────────

    public void StartNieuweSessie(Action<int> callback = null)
    {
        StartCoroutine(StartSessieRoutine(callback));
    }

    private IEnumerator StartSessieRoutine(Action<int> callback)
    {
        huidigeSessieId = UnityEngine.Random.Range(1000, 99999);
        int gebruikerId = PlayerPrefs.GetInt("IngelogdeGebruikerId", 1);

        if (!string.IsNullOrEmpty(actieveServerUrl) && isVerbondenMetServer)
        {
            var reqObj = new StartSessieRequestDto { GebruikerId = gebruikerId };
            string jsonBody = JsonUtility.ToJson(reqObj);
            string url = CleanUrl(actieveServerUrl) + "sessie";

            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.timeout = 4;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    SessieResponse response = JsonUtility.FromJson<SessieResponse>(www.downloadHandler.text);
                    int dbId = response != null ? response.KrijgId() : -1;
                    if (dbId > 0)
                    {
                        huidigeSessieId = dbId;
                        Debug.Log($"[DatabaseManager] MySQL Sessie gestart met ID: {huidigeSessieId} (Gebruiker: {gebruikerId})");
                    }
                }
            }
        }

        callback?.Invoke(huidigeSessieId);
    }

    public void UpdateSessieLevel(int level)
    {
        if (huidigeSessieId <= 0) return;
        StartCoroutine(UpdateSessieLevelRoutine(huidigeSessieId, level));
    }

    private IEnumerator UpdateSessieLevelRoutine(int sessieId, int level)
    {
        if (!string.IsNullOrEmpty(actieveServerUrl) && isVerbondenMetServer)
        {
            var reqObj = new SluitSessieRequestDto { EindLevel = level };
            string json = JsonUtility.ToJson(reqObj);
            string url = CleanUrl(actieveServerUrl) + $"sessie/{sessieId}";

            using (UnityWebRequest www = new UnityWebRequest(url, "PUT"))
            {
                byte[] body = Encoding.UTF8.GetBytes(json);
                www.uploadHandler = new UploadHandlerRaw(body);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.timeout = 3;

                yield return www.SendWebRequest();
            }
        }
    }

    public void SluitSessie(int eindLevel)
    {
        if (huidigeSessieId <= 0) return;
        int sessieOmTeSluiten = huidigeSessieId;
        huidigeSessieId = -1;
        StartCoroutine(UpdateSessieLevelRoutine(sessieOmTeSluiten, eindLevel));
    }

    // ─── Opslaan van Patient Cases (Game -> Database) ─────────────────────────

    public void SlaPatientCaseOp(int level, Patient correcteData, NtsUrgentie correcteUrgentie,
        int inA, int inB, int inC, int inD, int inE, NtsUrgentie inUrg,
        int fouten, bool isCorrect, float tijd)
    {
        StartCoroutine(SlaCaseRoutine(level, correcteData, correcteUrgentie, inA, inB, inC, inD, inE, inUrg, fouten, isCorrect, tijd));
    }

    private IEnumerator SlaCaseRoutine(int level, Patient correcteData, NtsUrgentie correcteUrgentie,
        int inA, int inB, int inC, int inD, int inE, NtsUrgentie inUrg,
        int fouten, bool isCorrect, float tijd)
    {
        if (huidigeSessieId <= 0)
        {
            // Start automatisch een sessie indien deze nog niet gestart was
            yield return StartCoroutine(StartSessieRoutine(null));
        }

        var dto = new PatientCaseRequestDto
        {
            SessieId = huidigeSessieId,
            Level = level,
            CorrecteAirway = correcteData != null ? correcteData.airway : 5,
            CorrecteBreathing = correcteData != null ? correcteData.breathing : 5,
            CorrecteCirculation = correcteData != null ? correcteData.circulation : 5,
            CorrecteDisability = correcteData != null ? correcteData.disabillity : 5,
            CorrecteExposure = correcteData != null ? correcteData.exposure : 5,
            CorrecteUrgentie = (int)correcteUrgentie,
            InvoerAirway = inA,
            InvoerBreathing = inB,
            InvoerCirculation = inC,
            InvoerDisability = inD,
            InvoerExposure = inE,
            InvoerUrgentie = (int)inUrg,
            AantalFouten = fouten,
            IsCorrect = isCorrect,
            VerstrekenTijdSeconden = tijd
        };

        if (!string.IsNullOrEmpty(actieveServerUrl) && isVerbondenMetServer)
        {
            string jsonBody = JsonUtility.ToJson(dto);
            string url = CleanUrl(actieveServerUrl) + "patientcase";

            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.timeout = 4;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[DatabaseManager] Patiënt-case #{dto.Level} succesvol opgeslagen in MySQL database! (Correct: {isCorrect})");
                    yield break;
                }
            }
        }

        Debug.Log($"[DatabaseManager] Patiënt-case opgeslagen in lokale buffer (Level {level}, Fouten: {fouten})");
    }

    // ─── Data Ophalen (Database -> Game) ──────────────────────────────────────

    public void HaalStatsOp(Action<GebruikerStatsDto> callback)
    {
        StartCoroutine(HaalStatsRoutine(callback));
    }

    private IEnumerator HaalStatsRoutine(Action<GebruikerStatsDto> callback)
    {
        int gebruikerId = PlayerPrefs.GetInt("IngelogdeGebruikerId", 1);

        if (!string.IsNullOrEmpty(actieveServerUrl) && isVerbondenMetServer)
        {
            string url = CleanUrl(actieveServerUrl) + $"stats?gebruikerId={gebruikerId}";

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.timeout = 4;
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    GebruikerStatsDto stats = JsonUtility.FromJson<GebruikerStatsDto>(www.downloadHandler.text);
                    if (stats != null)
                    {
                        callback?.Invoke(stats);
                        yield break;
                    }
                }
            }
        }

        callback?.Invoke(null);
    }

    public void HaalScenarioOp(int level, Action<ScenarioItemDto> callback)
    {
        StartCoroutine(HaalScenarioRoutine(level, callback));
    }

    private IEnumerator HaalScenarioRoutine(int level, Action<ScenarioItemDto> callback)
    {
        if (!string.IsNullOrEmpty(actieveServerUrl) && isVerbondenMetServer)
        {
            string url = CleanUrl(actieveServerUrl) + $"scenarios?level={level}";

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.timeout = 4;
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    ScenarioItemDto[] items = JsonArrayHelper.FromJson<ScenarioItemDto>(www.downloadHandler.text);
                    if (items != null && items.Length > 0)
                    {
                        // Kies een willekeurig scenario uit de resultaten voor dit level
                        ScenarioItemDto gekozen = items[UnityEngine.Random.Range(0, items.Length)];
                        callback?.Invoke(gekozen);
                        yield break;
                    }
                }
            }
        }

        callback?.Invoke(null);
    }

    private void OnApplicationQuit()
    {
        if (huidigeSessieId > 0)
        {
            StartCoroutine(UpdateSessieLevelRoutine(huidigeSessieId, 1));
        }
    }

    // ─── Lokale Database Hulpfuncties ────────────────────────────────────────

    private void InitialiseerStandaardAccounts()
    {
        LokaleAccountLijst lijst = LaadLokaleAccountLijst();
        bool gewijzigd = false;

        if (!BestaatLokaalAccount("hanna"))
        {
            lijst.accounts.Add(new LokaalAccount { gebruikersnaam = "hanna", wachtwoord = "admin", volledigeNaam = "Hanna Versluizen", rol = "Trainee SEH" });
            gewijzigd = true;
        }

        if (!BestaatLokaalAccount("admin"))
        {
            lijst.accounts.Add(new LokaalAccount { gebruikersnaam = "admin", wachtwoord = "1234", volledigeNaam = "Systeem Beheerder", rol = "Docent / Beheerder" });
            gewijzigd = true;
        }

        if (gewijzigd) SlaLokaleAccountLijstOp(lijst);
    }

    private bool BestaatLokaalAccount(string gebruikersnaam)
    {
        LokaleAccountLijst lijst = LaadLokaleAccountLijst();
        return lijst.accounts.Exists(a => a.gebruikersnaam.Equals(gebruikersnaam, StringComparison.OrdinalIgnoreCase));
    }

    private bool ValideerLokaalAccount(string gebruikersnaam, string wachtwoord)
    {
        LokaleAccountLijst lijst = LaadLokaleAccountLijst();
        return lijst.accounts.Exists(a =>
            a.gebruikersnaam.Equals(gebruikersnaam, StringComparison.OrdinalIgnoreCase) &&
            a.wachtwoord == wachtwoord);
    }

    private void SlaLokaalAccountOp(string gebruikersnaam, string wachtwoord, string volledigeNaam, string rol)
    {
        LokaleAccountLijst lijst = LaadLokaleAccountLijst();
        var bestaand = lijst.accounts.Find(a => a.gebruikersnaam.Equals(gebruikersnaam, StringComparison.OrdinalIgnoreCase));
        if (bestaand != null)
        {
            bestaand.wachtwoord = wachtwoord;
            bestaand.volledigeNaam = volledigeNaam;
            bestaand.rol = rol;
        }
        else
        {
            lijst.accounts.Add(new LokaalAccount { gebruikersnaam = gebruikersnaam, wachtwoord = wachtwoord, volledigeNaam = volledigeNaam, rol = rol });
        }
        SlaLokaleAccountLijstOp(lijst);
    }

    private LokaleAccountLijst LaadLokaleAccountLijst()
    {
        string json = PlayerPrefs.GetString(LOKALE_ACCOUNTS_KEY, "");
        if (string.IsNullOrEmpty(json)) return new LokaleAccountLijst();
        try { return JsonUtility.FromJson<LokaleAccountLijst>(json) ?? new LokaleAccountLijst(); }
        catch { return new LokaleAccountLijst(); }
    }

    private void SlaLokaleAccountLijstOp(LokaleAccountLijst lijst)
    {
        string json = JsonUtility.ToJson(lijst);
        PlayerPrefs.SetString(LOKALE_ACCOUNTS_KEY, json);
        PlayerPrefs.Save();
    }
}
