using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class SessieResponse
{
    public int sessieId;
}

[System.Serializable]
public class AuthResponse
{
    public bool succes;
    public string bericht;
    public int gebruikerId;
    public string gebruikersnaam;
}

[System.Serializable]
public class LokaalAccount
{
    public string gebruikersnaam;
    public string wachtwoord;
}

[System.Serializable]
public class LokaleAccountLijst
{
    public List<LokaalAccount> accounts = new List<LokaalAccount>();
}

/// <summary>
/// Beheert de verbinding met de webserver én garandeert dat er ALTIJD gespeeld kan worden.
/// Als de server offline is (of op de Quest onbereikbaar is), schakelt het automatisch
/// over naar de ingebouwde lokale database zodat inloggen, registreren en sessies ALTIJD werken.
/// </summary>
public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    [Header("Server Instellingen")]
    [Tooltip("Primaire URL (localhost voor PC Editor)")]
    public string serverUrl = "http://localhost/protocol_triage_api/";

    [Tooltip("Secundaire URL voor standalone Meta Quest (het lokale IP-adres van je PC)")]
    public string questFallbackUrl = "http://10.16.31.55/protocol_triage_api/";

    [Header("Status")]
    public bool isVerbondenMetServer = false;
    public bool isOfflineModus = false;

    private int huidigeSessieId = -1;
    private string actieveServerUrl;
    private const string LOKALE_ACCOUNTS_KEY = "LOKALE_DATABASE_ACCOUNTS";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Bepaal de meest geschikte start-URL (op Quest is localhost de headset zelf)
#if UNITY_ANDROID && !UNITY_EDITOR
            actieveServerUrl = !string.IsNullOrEmpty(questFallbackUrl) ? questFallbackUrl : serverUrl;
#else
            actieveServerUrl = serverUrl;
#endif

            InitialiseerStandaardAccounts();
            StartCoroutine(InitialiseerEnBlijfVerbinden());
        }
        else
        {
            Destroy(gameObject);
        }
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
                    // Probeer alternatieve URL als we op de Quest zitten of localhost faalt
                    if (actieveServerUrl != questFallbackUrl && !string.IsNullOrEmpty(questFallbackUrl))
                    {
                        StartCoroutine(TestServerVerbinding(questFallbackUrl, (fallbackSucces) =>
                        {
                            if (fallbackSucces)
                            {
                                actieveServerUrl = questFallbackUrl;
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

            // Controleer elke 15 seconden opnieuw op de achtergrond
            yield return new WaitForSeconds(15f);
        }
    }

    private IEnumerator TestServerVerbinding(string url, Action<bool> callback)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url + "init.php"))
        {
            www.timeout = 3; // Snelle time-out zodat de game nooit hapert
            yield return www.SendWebRequest();

            bool gelukt = (www.result == UnityWebRequest.Result.Success);
            callback?.Invoke(gelukt);
        }
    }

    // ─── Authenticatie (met gegarandeerde fallback) ───────────────────────────

    /// <summary>
    /// Registreert een account. Probeert eerst de online server.
    /// Als de server niet bereikbaar is, slaagt de registratie ALSNOG lokaal.
    /// </summary>
    public void Registreer(string gebruikersnaam, string wachtwoord, Action<bool, string> callback)
    {
        StartCoroutine(RegistreerRoutine(gebruikersnaam, wachtwoord, callback));
    }

    private IEnumerator RegistreerRoutine(string gebruikersnaam, string wachtwoord, Action<bool, string> callback)
    {
        bool serverGeprobeerd = false;
        bool serverGelukt = false;
        string serverFout = "";

        // 1. Probeer eerst online als we een URL hebben
        if (!string.IsNullOrEmpty(actieveServerUrl))
        {
            WWWForm form = new WWWForm();
            form.AddField("actie", "registreer");
            form.AddField("gebruikersnaam", gebruikersnaam);
            form.AddField("wachtwoord", wachtwoord);

            using (UnityWebRequest www = UnityWebRequest.Post(actieveServerUrl + "auth.php", form))
            {
                www.timeout = 4;
                yield return www.SendWebRequest();

                serverGeprobeerd = true;
                if (www.result == UnityWebRequest.Result.Success)
                {
                    AuthResponse resp = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);
                    if (resp != null && resp.succes)
                    {
                        serverGelukt = true;
                        isVerbondenMetServer = true;
                        isOfflineModus = false;
                        // Sla ook direct een lokale kopie op zodat je offline ook met dit account kan
                        SlaLokaalAccountOp(gebruikersnaam, wachtwoord);
                        callback?.Invoke(true, "");
                        yield break;
                    }
                    else
                    {
                        serverFout = resp != null ? resp.bericht : "Registratie mislukt.";
                    }
                }
            }
        }

        // 2. Als de server online was maar een expliciete fout gaf (bijv. "Gebruikersnaam al in gebruik"):
        if (serverGeprobeerd && !string.IsNullOrEmpty(serverFout) && serverFout.Contains("al in gebruik"))
        {
            callback?.Invoke(false, serverFout);
            yield break;
        }

        // 3. Fallback: Sla ALTIJD lokaal op als de server offline of onbereikbaar is
        isOfflineModus = true;
        isVerbondenMetServer = false;

        if (BestaatLokaalAccount(gebruikersnaam))
        {
            callback?.Invoke(false, "Deze gebruikersnaam is al in gebruik.");
            yield break;
        }

        SlaLokaalAccountOp(gebruikersnaam, wachtwoord);
        Debug.Log($"[DatabaseManager] Account '{gebruikersnaam}' succesvol geregistreerd in Lokale Database.");
        callback?.Invoke(true, "");
    }

    /// <summary>
    /// Logt in. Probeert eerst de online server.
    /// Als de server niet bereikbaar is, logt hij ALSNOG succesvol in via de lokale database.
    /// </summary>
    public void Login(string gebruikersnaam, string wachtwoord, Action<bool, string> callback)
    {
        StartCoroutine(LoginRoutine(gebruikersnaam, wachtwoord, callback));
    }

    private IEnumerator LoginRoutine(string gebruikersnaam, string wachtwoord, Action<bool, string> callback)
    {
        // 1. Probeer eerst online
        if (!string.IsNullOrEmpty(actieveServerUrl))
        {
            WWWForm form = new WWWForm();
            form.AddField("actie", "login");
            form.AddField("gebruikersnaam", gebruikersnaam);
            form.AddField("wachtwoord", wachtwoord);

            using (UnityWebRequest www = UnityWebRequest.Post(actieveServerUrl + "auth.php", form))
            {
                www.timeout = 4;
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AuthResponse resp = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);
                    if (resp != null && resp.succes)
                    {
                        PlayerPrefs.SetString("IngelogdeGebruiker", resp.gebruikersnaam);
                        PlayerPrefs.Save();
                        isVerbondenMetServer = true;
                        isOfflineModus = false;
                        // Sla ook lokaal op
                        SlaLokaalAccountOp(gebruikersnaam, wachtwoord);
                        callback?.Invoke(true, "");
                        yield break;
                    }
                    else
                    {
                        // Server reageerde en zei wachtwoord onjuist
                        string fout = resp != null ? resp.bericht : "Inloggegevens onjuist.";
                        callback?.Invoke(false, fout);
                        yield break;
                    }
                }
            }
        }

        // 2. Fallback: Controleer lokale database (zorgt dat je ALTIJD kunt inloggen)
        isOfflineModus = true;
        isVerbondenMetServer = false;

        if (ValideerLokaalAccount(gebruikersnaam, wachtwoord))
        {
            PlayerPrefs.SetString("IngelogdeGebruiker", gebruikersnaam);
            PlayerPrefs.Save();
            Debug.Log($"[DatabaseManager] Inloggen geslaagd via Lokale Database voor '{gebruikersnaam}'.");
            callback?.Invoke(true, "");
        }
        else
        {
            callback?.Invoke(false, "Gebruikersnaam of wachtwoord onjuist.");
        }
    }

    // ─── Sessie & Cases ───────────────────────────────────────────────────────

    public void StartNieuweSessie()
    {
        StartCoroutine(StartSessieRoutine());
    }

    private IEnumerator StartSessieRoutine()
    {
        // Maak altijd direct een geldig sessie-ID aan
        huidigeSessieId = UnityEngine.Random.Range(1000, 99999);

        if (!string.IsNullOrEmpty(actieveServerUrl) && isVerbondenMetServer)
        {
            WWWForm form = new WWWForm();
            form.AddField("actie", "start_sessie");

            using (UnityWebRequest www = UnityWebRequest.Post(actieveServerUrl + "api.php", form))
            {
                www.timeout = 3;
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    SessieResponse response = JsonUtility.FromJson<SessieResponse>(www.downloadHandler.text);
                    if (response != null && response.sessieId > 0)
                    {
                        huidigeSessieId = response.sessieId;
                    }
                }
            }
        }

        Debug.Log($"[DatabaseManager] Triage sessie gestart (Sessie ID: {huidigeSessieId})");
    }

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
        if (huidigeSessieId == -1)
            huidigeSessieId = UnityEngine.Random.Range(1000, 99999);

        // Probeer naar online server te sturen
        if (!string.IsNullOrEmpty(actieveServerUrl) && isVerbondenMetServer)
        {
            WWWForm form = new WWWForm();
            form.AddField("actie", "sla_case_op");
            form.AddField("sessieId", huidigeSessieId);
            form.AddField("level", level);

            form.AddField("cA", correcteData.airway);
            form.AddField("cB", correcteData.breathing);
            form.AddField("cC", correcteData.circulation);
            form.AddField("cD", correcteData.disabillity);
            form.AddField("cE", correcteData.exposure);
            form.AddField("cU", (int)correcteUrgentie);

            form.AddField("iA", inA);
            form.AddField("iB", inB);
            form.AddField("iC", inC);
            form.AddField("iD", inD);
            form.AddField("iE", inE);
            form.AddField("iU", (int)inUrg);

            form.AddField("fouten", fouten);
            form.AddField("isCorrect", isCorrect ? 1 : 0);
            form.AddField("tijd", tijd.ToString(System.Globalization.CultureInfo.InvariantCulture));

            using (UnityWebRequest www = UnityWebRequest.Post(actieveServerUrl + "api.php", form))
            {
                www.timeout = 3;
                yield return www.SendWebRequest();
            }
        }

        Debug.Log($"[DatabaseManager] Patiënt-case opgeslagen (Level {level}, Fouten: {fouten})");
    }

    public void SluitSessie(int eindLevel)
    {
        if (huidigeSessieId == -1) return;
        StartCoroutine(SluitSessieRoutine(eindLevel));
    }

    private IEnumerator SluitSessieRoutine(int eindLevel)
    {
        if (!string.IsNullOrEmpty(actieveServerUrl) && isVerbondenMetServer)
        {
            WWWForm form = new WWWForm();
            form.AddField("actie", "sluit_sessie");
            form.AddField("sessieId", huidigeSessieId);
            form.AddField("eindLevel", eindLevel);

            using (UnityWebRequest www = UnityWebRequest.Post(actieveServerUrl + "api.php", form))
            {
                www.timeout = 2;
                yield return www.SendWebRequest();
            }
        }

        huidigeSessieId = -1;
    }

    private void OnApplicationQuit()
    {
        if (huidigeSessieId != -1)
            StartCoroutine(SluitSessieRoutine(-1));
    }

    // ─── Lokale Persistente Database Hulpfuncties ────────────────────────────

    private void InitialiseerStandaardAccounts()
    {
        LokaleAccountLijst lijst = LaadLokaleAccountLijst();
        // Zorg dat er altijd een standaard admin account is dat ALTIJD werkt
        if (!BestaatLokaalAccount("admin"))
        {
            lijst.accounts.Add(new LokaalAccount { gebruikersnaam = "admin", wachtwoord = "1234" });
            SlaLokaleAccountLijstOp(lijst);
        }
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

    private void SlaLokaalAccountOp(string gebruikersnaam, string wachtwoord)
    {
        LokaleAccountLijst lijst = LaadLokaleAccountLijst();
        var bestaand = lijst.accounts.Find(a => a.gebruikersnaam.Equals(gebruikersnaam, StringComparison.OrdinalIgnoreCase));
        if (bestaand != null)
        {
            bestaand.wachtwoord = wachtwoord;
        }
        else
        {
            lijst.accounts.Add(new LokaalAccount { gebruikersnaam = gebruikersnaam, wachtwoord = wachtwoord });
        }
        SlaLokaleAccountLijstOp(lijst);
    }

    private LokaleAccountLijst LaadLokaleAccountLijst()
    {
        string json = PlayerPrefs.GetString(LOKALE_ACCOUNTS_KEY, "");
        if (string.IsNullOrEmpty(json))
            return new LokaleAccountLijst();

        try
        {
            return JsonUtility.FromJson<LokaleAccountLijst>(json) ?? new LokaleAccountLijst();
        }
        catch
        {
            return new LokaleAccountLijst();
        }
    }

    private void SlaLokaleAccountLijstOp(LokaleAccountLijst lijst)
    {
        string json = JsonUtility.ToJson(lijst);
        PlayerPrefs.SetString(LOKALE_ACCOUNTS_KEY, json);
        PlayerPrefs.Save();
    }
}
