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
    public string loginScene = "LoginScene";

    private void Start()
    {
        string gebruiker = PlayerPrefs.GetString("IngelogdeGebruiker", "Onbekend");
        welkomTekst.text = $"Welkom, {gebruiker}!";
        statusTekst.text = "";

        startKnop.onClick.AddListener(StartGame);
        uitlogKnop.onClick.AddListener(Uitloggen);

        if (statusTekst != null)
        {
            if (DatabaseManager.Instance != null && DatabaseManager.Instance.isVerbondenMetServer)
                statusTekst.text = "Server: Online";
            else
                statusTekst.text = "Server: Lokale Modus Actief";
        }
    }

    public void StartGame()
    {
        statusTekst.text = "Sessie wordt gestart...";

        if (DatabaseManager.Instance != null)
            DatabaseManager.Instance.StartNieuweSessie();
        else
            Debug.LogWarning("[MainMenuManager] DatabaseManager niet gevonden!");

        SceneManager.LoadScene(gameScene);
    }

    private void Uitloggen()
    {
        PlayerPrefs.DeleteKey("IngelogdeGebruiker");
        PlayerPrefs.Save();
        SceneManager.LoadScene(loginScene);
    }
}
