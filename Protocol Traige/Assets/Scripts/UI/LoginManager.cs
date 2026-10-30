using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    // ─── Login paneel ────────────────────────────────────────────────────────

    [Header("Login Paneel")]
    public GameObject loginPaneel;
    public TMP_InputField loginGebruikersnaamVeld;
    public TMP_InputField loginWachtwoordVeld;
    public Button inlogKnop;
    public Button naarRegistratieKnop;
    public TextMeshProUGUI loginFoutTekst;

    // ─── Registratie paneel ──────────────────────────────────────────────────

    [Header("Registratie Paneel")]
    public GameObject registratiePaneel;
    public TMP_InputField regGebruikersnaamVeld;
    public TMP_InputField regWachtwoordVeld;
    public TMP_InputField regWachtwoordBevestigingVeld;
    public Button registreerKnop;
    public Button naarLoginKnop;
    public TextMeshProUGUI registratieFoutTekst;

    // ─── Laad paneel ─────────────────────────────────────────────────────────

    [Header("Laad Paneel")]
    public GameObject laadPaneel;

    // ─── Scene ───────────────────────────────────────────────────────────────

    [Header("Scene Instellingen")]
    public string hoofdMenuScene = "MainMenu";

    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        if (laadPaneel != null) laadPaneel.SetActive(false);
        ToonLoginPaneel();

        if (inlogKnop != null) inlogKnop.onClick.AddListener(ProbeerInloggen);
        if (registreerKnop != null) registreerKnop.onClick.AddListener(ProbeerRegistreren);
        if (naarRegistratieKnop != null) naarRegistratieKnop.onClick.AddListener(ToonRegistratiePaneel);
        if (naarLoginKnop != null) naarLoginKnop.onClick.AddListener(ToonLoginPaneel);
    }

    // ─── Paneel wisselen ─────────────────────────────────────────────────────

    private void ToonLoginPaneel()
    {
        if (loginPaneel != null) loginPaneel.SetActive(true);
        if (registratiePaneel != null) registratiePaneel.SetActive(false);
        if (loginFoutTekst != null) loginFoutTekst.text = "";
        VRVirtualKeyboard.Instance?.KoppelAlleInputFieldsInScene();
    }

    private void ToonRegistratiePaneel()
    {
        if (registratiePaneel != null) registratiePaneel.SetActive(true);
        if (loginPaneel != null) loginPaneel.SetActive(false);
        if (registratieFoutTekst != null) registratieFoutTekst.text = "";
        VRVirtualKeyboard.Instance?.KoppelAlleInputFieldsInScene();
    }

    // ─── Inloggen ────────────────────────────────────────────────────────────

    public void ProbeerInloggen()
    {
        if (loginFoutTekst != null) loginFoutTekst.text = "";

        string gebruiker = loginGebruikersnaamVeld != null ? loginGebruikersnaamVeld.text.Trim() : "";
        string wachtwoord = loginWachtwoordVeld != null ? loginWachtwoordVeld.text : "";

        if (string.IsNullOrEmpty(gebruiker) || string.IsNullOrEmpty(wachtwoord))
        {
            if (loginFoutTekst != null) loginFoutTekst.text = "Vul je gebruikersnaam en wachtwoord in.";
            return;
        }

        if (inlogKnop != null) inlogKnop.interactable = false;
        if (loginFoutTekst != null) loginFoutTekst.text = "Inloggen...";

        if (DatabaseManager.Instance != null)
        {
            DatabaseManager.Instance.Login(gebruiker, wachtwoord, (succes, fout) =>
            {
                if (inlogKnop != null) inlogKnop.interactable = true;

                if (succes)
                    LaadHoofdmenu();
                else if (loginFoutTekst != null)
                    loginFoutTekst.text = fout;
            });
        }
        else
        {
            if (inlogKnop != null) inlogKnop.interactable = true;
            if (loginFoutTekst != null) loginFoutTekst.text = "Geen verbinding met de server.";
            Debug.LogError("[LoginManager] DatabaseManager niet gevonden!");
        }
    }

    // ─── Registreren ─────────────────────────────────────────────────────────

    public void ProbeerRegistreren()
    {
        if (registratieFoutTekst != null) registratieFoutTekst.text = "";

        string gebruiker = regGebruikersnaamVeld != null ? regGebruikersnaamVeld.text.Trim() : "";
        string wachtwoord = regWachtwoordVeld != null ? regWachtwoordVeld.text : "";
        string bevestiging = regWachtwoordBevestigingVeld != null ? regWachtwoordBevestigingVeld.text : "";

        if (string.IsNullOrEmpty(gebruiker) || string.IsNullOrEmpty(wachtwoord) || string.IsNullOrEmpty(bevestiging))
        {
            if (registratieFoutTekst != null) registratieFoutTekst.text = "Vul alle velden in.";
            return;
        }

        if (gebruiker.Length < 3)
        {
            if (registratieFoutTekst != null) registratieFoutTekst.text = "Gebruikersnaam moet minimaal 3 tekens bevatten.";
            return;
        }

        if (wachtwoord.Length < 6)
        {
            if (registratieFoutTekst != null) registratieFoutTekst.text = "Wachtwoord moet minimaal 6 tekens bevatten.";
            return;
        }

        if (wachtwoord != bevestiging)
        {
            if (registratieFoutTekst != null) registratieFoutTekst.text = "Wachtwoorden komen niet overeen.";
            return;
        }

        if (registreerKnop != null) registreerKnop.interactable = false;
        if (registratieFoutTekst != null) registratieFoutTekst.text = "Account aanmaken...";

        if (DatabaseManager.Instance != null)
        {
            DatabaseManager.Instance.Registreer(gebruiker, wachtwoord, (succes, fout) =>
            {
                if (registreerKnop != null) registreerKnop.interactable = true;

                if (succes)
                {
                    if (registratieFoutTekst != null) registratieFoutTekst.text = "Account aangemaakt! Je wordt ingelogd...";
                    DatabaseManager.Instance.Login(gebruiker, wachtwoord, (loginSucces, loginFout) =>
                    {
                        if (loginSucces)
                            LaadHoofdmenu();
                        else if (registratieFoutTekst != null)
                            registratieFoutTekst.text = loginFout;
                    });
                }
                else if (registratieFoutTekst != null)
                {
                    registratieFoutTekst.text = fout;
                }
            });
        }
        else
        {
            if (registreerKnop != null) registreerKnop.interactable = true;
            if (registratieFoutTekst != null) registratieFoutTekst.text = "Geen verbinding met de server.";
            Debug.LogError("[LoginManager] DatabaseManager niet gevonden!");
        }
    }

    // ─── Navigatie ───────────────────────────────────────────────────────────

    private void LaadHoofdmenu()
    {
        if (laadPaneel != null) laadPaneel.SetActive(true);
        if (loginPaneel != null) loginPaneel.SetActive(false);
        if (registratiePaneel != null) registratiePaneel.SetActive(false);
        SceneManager.LoadScene(hoofdMenuScene);
    }

    public void OnEnterInLoginVeld(string _) => ProbeerInloggen();
    public void OnEnterInRegistratieVeld(string _) => ProbeerRegistreren();
}
