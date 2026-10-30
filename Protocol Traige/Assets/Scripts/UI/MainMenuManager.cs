using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI - Tekst")]
    public TextMeshProUGUI welkomTekst;
    public TextMeshProUGUI statusTekst;

    [Header("UI - Knoppen")]
    public Button startKnop;
    public Button uitlogKnop;

    [Header("Scene Instellingen")]
    public string gameScene  = "GameScene";
    public string loginScene = "Login"; // De scene heet Login.unity

    private void Start()
    {
        string gebruiker = PlayerPrefs.GetString("IngelogdeGebruiker", "Hanna");
        string rol = PlayerPrefs.GetString("IngelogdeRol", "Trainee SEH");

        if (welkomTekst != null)
        {
            welkomTekst.text = $"Welkom, {gebruiker}!\n<size=70%>{rol}</size>";
        }

        if (startKnop != null) startKnop.onClick.AddListener(StartGame);
        if (uitlogKnop != null) uitlogKnop.onClick.AddListener(Uitloggen);

        UpdateServerEnStatsUI();
    }

    private void UpdateServerEnStatsUI()
    {
        if (statusTekst == null) return;

        bool isOnline = (DatabaseManager.Instance != null && DatabaseManager.Instance.isVerbondenMetServer);
        string serverLabel = isOnline ? "<color=green>Online (MySQL DB)</color>" : "<color=yellow>Lokale Modus</color>";
        statusTekst.text = $"Server: {serverLabel}";

        // Haal statistieken rechtstreeks uit de database op (Database -> Game)
        if (DatabaseManager.Instance != null)
        {
            DatabaseManager.Instance.HaalStatsOp((stats) =>
            {
                if (statusTekst == null) return;

                if (stats != null && stats.heeftData)
                {
                    statusTekst.text = $"Server: <color=green>Online (MySQL DB)</color>\n" +
                                       $"Sessies: {stats.totaalSessies} | Cases: {stats.totaalCases}\n" +
                                       $"Nauwkeurigheid: {stats.overallAccuratessePct:F1}%\n" +
                                       $"Hoogste Level: {stats.hoogsteLevel}";
                }
                else
                {
                    statusTekst.text = $"Server: {serverLabel}\nGeen eerdere speelstatistieken.";
                }
            });
        }
    }

    public void StartGame()
    {
        if (statusTekst != null)
            statusTekst.text = "Sessie wordt gestart in de database...";

        if (DatabaseManager.Instance != null)
        {
            DatabaseManager.Instance.StartNieuweSessie((sessieId) =>
            {
                SceneManager.LoadScene(gameScene);
            });
        }
        else
        {
            SceneManager.LoadScene(gameScene);
        }
    }

    private void Uitloggen()
    {
        PlayerPrefs.DeleteKey("IngelogdeGebruiker");
        PlayerPrefs.DeleteKey("IngelogdeGebruikerId");
        PlayerPrefs.DeleteKey("IngelogdeVolledigeNaam");
        PlayerPrefs.DeleteKey("IngelogdeRol");
        PlayerPrefs.Save();
        SceneManager.LoadScene(loginScene);
    }
}
