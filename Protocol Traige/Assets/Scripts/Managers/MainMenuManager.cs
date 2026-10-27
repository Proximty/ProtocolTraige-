using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Beheert het hoofdmenu na inloggen.
/// Start de database-sessie en wacht tot deze bevestigd is
/// voordat de GameScene geladen wordt.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    // ══════════════════════════════════════════════════════════════════
    //  INSPECTOR VELDEN
    // ══════════════════════════════════════════════════════════════════

    [Header("Scène Instellingen")]
    public string gameSceneNaam  = "GameScene";
    public string loginSceneNaam = "LoginScene";

    [Header("UI Elementen")]
    public TextMeshProUGUI welkomstTekst;
    public TextMeshProUGUI versieInfoTekst;
    public TextMeshProUGUI statusTekst;      // Toont "Sessie starten..." of fout

    public Button startKnop;
    public Button uitlogKnop;
    public Button afsluitenKnop;

    [Header("Menu Panelen (optioneel)")]
    public GameObject hoofdPaneel;
    public GameObject instellingenPaneel;
    public GameObject laadOverlay;           // Optioneel: verduistering tijdens laden

    // ══════════════════════════════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ══════════════════════════════════════════════════════════════════

    private void Start()
    {
        // Geen actieve login? → terug naar inlogscherm
        if (!SessionData.IsIngelogd)
        {
            Debug.LogWarning("[MainMenu] Geen actieve sessie – terug naar login.");
            SceneManager.LoadScene(loginSceneNaam);
            return;
        }

        // UI instellen
        if (welkomstTekst  != null) welkomstTekst.text  = $"Welkom, {SessionData.Gebruikersnaam}!";
        if (versieInfoTekst != null) versieInfoTekst.text = "Protocol Triage VR – v1.0";
        if (statusTekst    != null) statusTekst.text    = "";
        if (laadOverlay    != null) laadOverlay.SetActive(false);

        // Koppel knoppen
        if (startKnop      != null) startKnop.onClick.AddListener(OnStartKnopGedrukt);
        if (uitlogKnop     != null) uitlogKnop.onClick.AddListener(OnUitlogKnopGedrukt);
        if (afsluitenKnop  != null) afsluitenKnop.onClick.AddListener(OnAfsluitenKnopGedrukt);

        if (hoofdPaneel        != null) hoofdPaneel.SetActive(true);
        if (instellingenPaneel != null) instellingenPaneel.SetActive(false);
    }

    // ══════════════════════════════════════════════════════════════════
    //  KNOPPEN
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Start een database-sessie en wacht op bevestiging (max 10 sec),
    /// daarna wordt de GameScene geladen.
    /// </summary>
    public void OnStartKnopGedrukt()
    {
        StartCoroutine(StartSessieEnLaadSpel());
    }

    private IEnumerator StartSessieEnLaadSpel()
    {
        // UI blokkeren
        SetKnoppenActief(false);
        if (laadOverlay != null) laadOverlay.SetActive(true);
        if (statusTekst != null) statusTekst.text = "Sessie starten...";

        // Controleer DatabaseManager
        if (DatabaseManager.Instance == null)
        {
            Debug.LogError("[MainMenu] DatabaseManager.Instance is null!");
            ToonFout("Databasefout – probeer opnieuw.");
            yield break;
        }

        // Koppel gebruiker en start sessie
        DatabaseManager.Instance.gekoppeldeGebruikerId = SessionData.GebruikerId;
        DatabaseManager.Instance.StartNieuweSessie();

        // Wacht tot sessie actief is (max 10 seconden)
        float gewacht = 0f;
        float timeout = 10f;

        while (!DatabaseManager.Instance.SessieActief && gewacht < timeout)
        {
            gewacht += Time.deltaTime;
            yield return null;
        }

        if (!DatabaseManager.Instance.SessieActief)
        {
            Debug.LogWarning("[MainMenu] Sessie kon niet worden gestart binnen 10 seconden. Toch doorgaan...");
            if (statusTekst != null) statusTekst.text = "Geen verbinding met database.\nSpel wordt toch geladen...";
            yield return new WaitForSeconds(2f);
        }
        else
        {
            Debug.Log($"[MainMenu] Sessie actief (ID: {DatabaseManager.Instance.HuidigeSessieId}). GameScene laden...");
            if (statusTekst != null) statusTekst.text = "Verbonden! Spel laden...";
            yield return new WaitForSeconds(0.5f);
        }

        SceneManager.LoadScene(gameSceneNaam);
    }

    public void OnUitlogKnopGedrukt()
    {
        Debug.Log($"[MainMenu] '{SessionData.Gebruikersnaam}' uitgelogd.");
        SessionData.GebruikerId    = -1;
        SessionData.Gebruikersnaam = "Onbekend";
        SceneManager.LoadScene(loginSceneNaam);
    }

    public void OnAfsluitenKnopGedrukt()
    {
        Debug.Log("[MainMenu] Applicatie afgesloten.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void WisselInstellingenPaneel()
    {
        if (instellingenPaneel != null)
            instellingenPaneel.SetActive(!instellingenPaneel.activeSelf);
    }

    // ══════════════════════════════════════════════════════════════════
    //  HULP-METHODEN
    // ══════════════════════════════════════════════════════════════════

    private void ToonFout(string bericht)
    {
        if (statusTekst != null) statusTekst.text = bericht;
        if (laadOverlay != null) laadOverlay.SetActive(false);
        SetKnoppenActief(true);
    }

    private void SetKnoppenActief(bool actief)
    {
        if (startKnop  != null) startKnop.interactable  = actief;
        if (uitlogKnop != null) uitlogKnop.interactable = actief;
    }
}
