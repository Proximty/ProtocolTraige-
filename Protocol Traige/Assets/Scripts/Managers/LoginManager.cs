using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Beheert zowel de inlogpagina als de accountregistratie.
/// De UI bestaat uit twee panelen die gewisseld worden:
///   - LoginPaneel   → gebruikersnaam + wachtwoord + "Inloggen"-knop
///   - RegistratiePaneel → gebruikersnaam, e-mail, wachtwoord, herhaal + "Registreer"-knop
///
/// Koppel dit script aan een leeg GameObject in de LoginScene.
/// </summary>
public class LoginManager : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════
    //  INSPECTOR VELDEN
    // ══════════════════════════════════════════════════════════════════

    [Header("API Instellingen")]
    [Tooltip("Basis-URL van de .NET API, bijv. http://localhost:5000/api")]
    public string apiUrl = "http://localhost:5000/api";

    [Header("Scène Instellingen")]
    public string hoofdmenuSceneNaam = "MainMenu";

    // ── Panelen ───────────────────────────────────────────────────────
    [Header("Panelen")]
    [Tooltip("Het volledige login-paneel GameObject")]
    public GameObject loginPaneel;
    [Tooltip("Het volledige registratie-paneel GameObject")]
    public GameObject registratiePaneel;

    // ── Login UI ──────────────────────────────────────────────────────
    [Header("UI – Inloggen")]
    public TMP_InputField login_GebruikersnaamInput;
    public TMP_InputField login_WachtwoordInput;
    public Button         login_InlogKnop;
    public Button         login_ToonWachtwoordKnop;
    public Button         login_NaarRegistratieKnop;  // "Nog geen account? Registreer hier"
    public TextMeshProUGUI login_FoutmeldingTekst;

    // ── Registratie UI ────────────────────────────────────────────────
    [Header("UI – Registreren")]
    public TMP_InputField reg_GebruikersnaamInput;
    public TMP_InputField reg_EmailInput;
    public TMP_InputField reg_WachtwoordInput;
    public TMP_InputField reg_HerhaalWachtwoordInput;
    public Button         reg_RegistreerKnop;
    public Button         reg_ToonWachtwoordKnop;
    public Button         reg_TerugNaarLoginKnop;     // "Al een account? Log in"
    public TextMeshProUGUI reg_FoutmeldingTekst;
    public TextMeshProUGUI reg_SuccesTekst;

    // ── Gedeeld ───────────────────────────────────────────────────────
    [Header("Gedeeld")]
    [Tooltip("Optioneel: laad-spinner of 'Even geduld...' tekst")]
    public GameObject laadIndicator;

    // ══════════════════════════════════════════════════════════════════
    //  PRIVÉ VELDEN
    // ══════════════════════════════════════════════════════════════════

    private bool login_WachtwoordZichtbaar = false;
    private bool reg_WachtwoordZichtbaar   = false;

    // ══════════════════════════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════

    private void Start()
    {
        // Start altijd op het login-paneel
        ToonLoginPaneel();

        // ── Login-knoppen koppelen ────────────────────────────────────
        if (login_InlogKnop != null)
            login_InlogKnop.onClick.AddListener(OnInlogKnopGedrukt);

        if (login_ToonWachtwoordKnop != null)
            login_ToonWachtwoordKnop.onClick.AddListener(WisselLoginWachtwoord);

        if (login_NaarRegistratieKnop != null)
            login_NaarRegistratieKnop.onClick.AddListener(ToonRegistratiePaneel);

        // Wachtwoord-invoerveld: druk op Enter = inloggen (voor VR toetsenbord)
        if (login_WachtwoordInput != null)
        {
            login_WachtwoordInput.contentType = TMP_InputField.ContentType.Password;
            login_WachtwoordInput.onSubmit.AddListener(_ => OnInlogKnopGedrukt());
        }

        // ── Registratie-knoppen koppelen ──────────────────────────────
        if (reg_RegistreerKnop != null)
            reg_RegistreerKnop.onClick.AddListener(OnRegistreerKnopGedrukt);

        if (reg_ToonWachtwoordKnop != null)
            reg_ToonWachtwoordKnop.onClick.AddListener(WisselRegWachtwoord);

        if (reg_TerugNaarLoginKnop != null)
            reg_TerugNaarLoginKnop.onClick.AddListener(ToonLoginPaneel);

        if (reg_WachtwoordInput != null)
            reg_WachtwoordInput.contentType = TMP_InputField.ContentType.Password;

        if (reg_HerhaalWachtwoordInput != null)
            reg_HerhaalWachtwoordInput.contentType = TMP_InputField.ContentType.Password;

        // Laad-indicator verbergen
        if (laadIndicator != null) laadIndicator.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════════════
    //  PANEEL WISSELEN
    // ══════════════════════════════════════════════════════════════════

    /// <summary>Toon het inlog-paneel en verberg registratie.</summary>
    public void ToonLoginPaneel()
    {
        if (loginPaneel     != null) loginPaneel.SetActive(true);
        if (registratiePaneel != null) registratiePaneel.SetActive(false);

        // Wis feedback
        if (login_FoutmeldingTekst != null) login_FoutmeldingTekst.text = "";
    }

    /// <summary>Toon het registratie-paneel en verberg login.</summary>
    public void ToonRegistratiePaneel()
    {
        if (loginPaneel       != null) loginPaneel.SetActive(false);
        if (registratiePaneel != null) registratiePaneel.SetActive(true);

        // Wis velden en feedback
        WisRegistratieVelden();
    }

    // ══════════════════════════════════════════════════════════════════
    //  INLOGGEN
    // ══════════════════════════════════════════════════════════════════

    /// <summary>Koppel aan de "Inloggen"-knop in de Editor.</summary>
    public void OnInlogKnopGedrukt()
    {
        string gebruiker  = login_GebruikersnaamInput != null ? login_GebruikersnaamInput.text.Trim() : "";
        string wachtwoord = login_WachtwoordInput     != null ? login_WachtwoordInput.text            : "";

        if (string.IsNullOrEmpty(gebruiker) || string.IsNullOrEmpty(wachtwoord))
        {
            ToonLoginFout("Vul je gebruikersnaam en wachtwoord in.");
            return;
        }

        StartCoroutine(InlogRoutine(gebruiker, wachtwoord));
    }

    public void WisselLoginWachtwoord()
    {
        if (login_WachtwoordInput == null) return;
        login_WachtwoordZichtbaar = !login_WachtwoordZichtbaar;
        login_WachtwoordInput.contentType = login_WachtwoordZichtbaar
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;
        login_WachtwoordInput.ForceLabelUpdate();
    }

    private IEnumerator InlogRoutine(string gebruiker, string wachtwoord)
    {
        SetLaadStatus(true);

        string jsonBody = $"{{\"gebruikersnaam\":\"{EscapeJson(gebruiker)}\",\"wachtwoord\":\"{EscapeJson(wachtwoord)}\"}}";
        byte[] bodyRaw  = Encoding.UTF8.GetBytes(jsonBody);

        using UnityWebRequest request = new UnityWebRequest($"{apiUrl}/login", "POST");
        request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        SetLaadStatus(false);

        if (request.result == UnityWebRequest.Result.Success)
        {
            LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);

            if (response != null && response.success)
            {
                SessionData.GebruikerId    = response.gebruikerId;
                SessionData.Gebruikersnaam = response.gebruikersnaam;
                Debug.Log($"[LoginManager] Ingelogd als '{response.gebruikersnaam}' (ID: {response.gebruikerId})");
                SceneManager.LoadScene(hoofdmenuSceneNaam);
            }
            else
            {
                ToonLoginFout(response != null ? response.foutmelding : "Onbekende fout.");
            }
        }
        else
        {
            ToonLoginFout($"Kan de server niet bereiken.\n({request.error})");
        }
    }

    // ══════════════════════════════════════════════════════════════════
    //  REGISTREREN
    // ══════════════════════════════════════════════════════════════════

    /// <summary>Koppel aan de "Account aanmaken"-knop in de Editor.</summary>
    public void OnRegistreerKnopGedrukt()
    {
        string gebruiker        = reg_GebruikersnaamInput      != null ? reg_GebruikersnaamInput.text.Trim()      : "";
        string email            = reg_EmailInput               != null ? reg_EmailInput.text.Trim()               : "";
        string wachtwoord       = reg_WachtwoordInput          != null ? reg_WachtwoordInput.text                 : "";
        string herhaalWachtwoord= reg_HerhaalWachtwoordInput   != null ? reg_HerhaalWachtwoordInput.text          : "";

        // ── Validatie ─────────────────────────────────────────────────
        if (string.IsNullOrEmpty(gebruiker))
        {
            ToonRegFout("Vul een gebruikersnaam in.");
            return;
        }
        if (gebruiker.Length < 3)
        {
            ToonRegFout("Gebruikersnaam moet minimaal 3 tekens zijn.");
            return;
        }
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        {
            ToonRegFout("Vul een geldig e-mailadres in.");
            return;
        }
        if (string.IsNullOrEmpty(wachtwoord))
        {
            ToonRegFout("Vul een wachtwoord in.");
            return;
        }
        if (wachtwoord.Length < 6)
        {
            ToonRegFout("Wachtwoord moet minimaal 6 tekens zijn.");
            return;
        }
        if (wachtwoord != herhaalWachtwoord)
        {
            ToonRegFout("Wachtwoorden komen niet overeen.");
            return;
        }

        StartCoroutine(RegistreerRoutine(gebruiker, email, wachtwoord));
    }

    public void WisselRegWachtwoord()
    {
        if (reg_WachtwoordInput == null) return;
        reg_WachtwoordZichtbaar = !reg_WachtwoordZichtbaar;

        TMP_InputField.ContentType type = reg_WachtwoordZichtbaar
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;

        reg_WachtwoordInput.contentType        = type;
        reg_HerhaalWachtwoordInput.contentType = type;

        reg_WachtwoordInput.ForceLabelUpdate();
        if (reg_HerhaalWachtwoordInput != null) reg_HerhaalWachtwoordInput.ForceLabelUpdate();
    }

    private IEnumerator RegistreerRoutine(string gebruiker, string email, string wachtwoord)
    {
        SetLaadStatus(true);

        string jsonBody = $"{{\"gebruikersnaam\":\"{EscapeJson(gebruiker)}\",\"email\":\"{EscapeJson(email)}\",\"wachtwoord\":\"{EscapeJson(wachtwoord)}\"}}";
        byte[] bodyRaw  = Encoding.UTF8.GetBytes(jsonBody);

        using UnityWebRequest request = new UnityWebRequest($"{apiUrl}/registreer", "POST");
        request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        SetLaadStatus(false);

        if (request.result == UnityWebRequest.Result.Success)
        {
            RegistreerResponse response = JsonUtility.FromJson<RegistreerResponse>(request.downloadHandler.text);

            if (response != null && response.success)
            {
                // Toon succesboodschap en ga na 2 seconden automatisch terug naar login
                ToonRegSucces($"Account aangemaakt! Je kunt nu inloggen als '{gebruiker}'.");
                Debug.Log($"[LoginManager] Nieuw account aangemaakt: '{gebruiker}'");

                // Vul gebruikersnaam alvast in op het loginpaneel
                if (login_GebruikersnaamInput != null)
                    login_GebruikersnaamInput.text = gebruiker;

                yield return new WaitForSeconds(2f);
                ToonLoginPaneel();
            }
            else
            {
                string fout = response != null ? response.foutmelding : "Onbekende fout bij registratie.";
                ToonRegFout(fout);
            }
        }
        else
        {
            ToonRegFout($"Kan de server niet bereiken.\n({request.error})");
        }
    }

    // ══════════════════════════════════════════════════════════════════
    //  HULP-METHODEN
    // ══════════════════════════════════════════════════════════════════

    private void ToonLoginFout(string bericht)
    {
        if (login_FoutmeldingTekst != null) login_FoutmeldingTekst.text = bericht;
        Debug.LogWarning($"[Login] {bericht}");
    }

    private void ToonRegFout(string bericht)
    {
        if (reg_FoutmeldingTekst != null) reg_FoutmeldingTekst.text = bericht;
        if (reg_SuccesTekst      != null) reg_SuccesTekst.text      = "";
        Debug.LogWarning($"[Registratie] {bericht}");
    }

    private void ToonRegSucces(string bericht)
    {
        if (reg_SuccesTekst      != null) reg_SuccesTekst.text      = bericht;
        if (reg_FoutmeldingTekst != null) reg_FoutmeldingTekst.text = "";
    }

    private void SetLaadStatus(bool isLaden)
    {
        if (laadIndicator        != null) laadIndicator.SetActive(isLaden);
        if (login_InlogKnop      != null) login_InlogKnop.interactable      = !isLaden;
        if (reg_RegistreerKnop   != null) reg_RegistreerKnop.interactable   = !isLaden;
    }

    private void WisRegistratieVelden()
    {
        if (reg_GebruikersnaamInput    != null) reg_GebruikersnaamInput.text    = "";
        if (reg_EmailInput             != null) reg_EmailInput.text             = "";
        if (reg_WachtwoordInput        != null) reg_WachtwoordInput.text        = "";
        if (reg_HerhaalWachtwoordInput != null) reg_HerhaalWachtwoordInput.text = "";
        if (reg_FoutmeldingTekst       != null) reg_FoutmeldingTekst.text       = "";
        if (reg_SuccesTekst            != null) reg_SuccesTekst.text            = "";
    }

    /// <summary>Escapet aanhalingstekens en backslashes voor JSON-strings.</summary>
    private static string EscapeJson(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}

// ══════════════════════════════════════════════════════════════════════
//  DATA-KLASSEN
// ══════════════════════════════════════════════════════════════════════

/// <summary>
/// Houdt de ingelogde gebruiker bij gedurende de hele spelsessie.
/// Statisch, dus persists zonder DontDestroyOnLoad.
/// </summary>
public static class SessionData
{
    public static int    GebruikerId    = -1;
    public static string Gebruikersnaam = "Onbekend";

    public static bool IsIngelogd => GebruikerId > 0;
}

[System.Serializable]
public class LoginResponse
{
    public bool   success;
    public int    gebruikerId;
    public string gebruikersnaam;
    public string foutmelding;
}

[System.Serializable]
public class RegistreerResponse
{
    public bool   success;
    public int    gebruikerId;
    public string foutmelding;
}
