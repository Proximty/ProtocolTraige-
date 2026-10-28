using UnityEngine;
using TMPro;

public class TriageTimerUI : MonoBehaviour
{
    [Header("Level & Progressie Settings")]
    public int huidigLevel = 1;
    public int basisAantalPatienten = 3; // Level 1 heeft 3 patienten
    private int benodigdAantalPatienten;
    private int afgehandeldePatientenInLevel = 0;

    [Header("Timer Instellingen")]
    public float maxTijdInSeconden = 90f; // 1.5 minuut per patient
    private float resterendeTijd;
    private bool isTimerActief = false;

    [Header("Reanimatie (CPR) Settings")]
    public float benodigdeCprTijd = 60f;
    private float cprVoortgang = 0f;
    private bool isReanimatieActief = false;
    public float cprSnelheidPerDruk = 1.0f;

    [Header("UI Elementen - Tekst")]
    public TextMeshProUGUI timerTekst;
    public TextMeshProUGUI levelTekst; // Bijv: "Level 1 (1/3)"

    [Header("UI Elementen - Feedback Panel")]
    public GameObject feedbackPaneel;
    public TextMeshProUGUI statusTitelTekst;
    public TextMeshProUGUI feedbackBerichtTekst;

    [Header("Referenties")]
    public GameManager gameManager;

    void Start()
    {
        if (feedbackPaneel != null) feedbackPaneel.SetActive(false);

        ResetLevelProgressie();
        HerstartTimer();
    }

    void Update()
    {
        // 1. Standaard Triage Timer
        if (isTimerActief)
        {
            if (resterendeTijd > 0)
            {
                resterendeTijd -= Time.deltaTime;
                UpdateTimerUI();
            }
            else
            {
                resterendeTijd = 0;
                isTimerActief = false;
                UpdateTimerUI();
                TijdVerstreken();
            }
        }

        // 2. Reanimatie (CPR) Input
        if (isReanimatieActief)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                VoerCompressieUit();
            }

            UpdateCprUI();

            if (cprVoortgang >= benodigdeCprTijd)
            {
                isReanimatieActief = false;
                AfrondenReanimatie();
            }
        }
    }

    public void ResetLevelProgressie()
    {
        huidigLevel = 1;
        afgehandeldePatientenInLevel = 0;
        UpdateBenodigdAantal();
    }

    private void UpdateBenodigdAantal()
    {
        // Level 1 = 3, Level 2 = 4, Level 3 = 5, enz.
        benodigdAantalPatienten = basisAantalPatienten + (huidigLevel - 1);
        UpdateLevelUI();
    }

    private void UpdateLevelUI()
    {
        if (levelTekst != null)
        {
            levelTekst.text = $"Level {huidigLevel} - Patiënt: {afgehandeldePatientenInLevel}/{benodigdAantalPatienten}";
        }
    }

    public void VoerCompressieUit()
    {
        if (!isReanimatieActief) return;

        cprVoortgang += cprSnelheidPerDruk;
        cprVoortgang = Mathf.Clamp(cprVoortgang, 0, benodigdeCprTijd);
    }

    public void HerstartTimer()
    {
        isReanimatieActief = false;
        cprVoortgang = 0f;
        resterendeTijd = maxTijdInSeconden;
        isTimerActief = true;

        if (feedbackPaneel != null) feedbackPaneel.SetActive(false);
        UpdateLevelUI();
    }

    public void StopTimer()
    {
        isTimerActief = false;
        isReanimatieActief = false;
    }

    private void UpdateTimerUI()
    {
        int minuten = Mathf.FloorToInt(resterendeTijd / 60);
        int seconden = Mathf.FloorToInt(resterendeTijd % 60);

        if (timerTekst != null)
            timerTekst.text = string.Format("Tijd: {0:00}:{1:00}", minuten, seconden);
    }

    private void UpdateCprUI()
    {
        int percentage = Mathf.FloorToInt((cprVoortgang / benodigdeCprTijd) * 100f);

        if (timerTekst != null)
            timerTekst.text = $"REANIMATIE: {percentage}%";
    }

    private void TijdVerstreken()
    {
        NtsUrgentie urgentie = NtsUrgentie.U5_Advies;
        if (gameManager != null && gameManager.huidigePatient != null)
        {
            int totaleScore = gameManager.huidigePatient.airway +
                              gameManager.huidigePatient.breathing +
                              gameManager.huidigePatient.circulation +
                              gameManager.huidigePatient.disabillity +
                              gameManager.huidigePatient.exposure;

            if (totaleScore >= 26) urgentie = NtsUrgentie.U0_Reanimatie;
            else urgentie = NtsUrgentie.U5_Advies;
        }

        if (urgentie == NtsUrgentie.U0_Reanimatie || urgentie == NtsUrgentie.U1_Levensbedreigend || urgentie == NtsUrgentie.U2_Spoed)
        {
            StartReanimatie();
        }
        else
        {
            PatientAfgehandeld(false, "PATIËNT WEGGEGAAN", "Je hebt er te lang over gedaan. De patiënt is boos vertrokken.");

            if (gameManager != null && gameManager.patientMovement != null)
            {
                gameManager.patientMovement.StartLopenNaarExit();
            }
        }
    }

    private void StartReanimatie()
    {
        isReanimatieActief = true;
        cprVoortgang = 0f;

        if (gameManager != null && gameManager.patientMovement != null)
        {
            gameManager.patientMovement.StopEnGaLiggen();
        }
    }

    private void AfrondenReanimatie()
    {
        PatientAfgehandeld(false, "REANIMATIE AFGEROND", "Patiënt is geheranimeerd, maar de triage is mislukt door tijdgebrek.");

        if (gameManager != null && gameManager.patientMovement != null)
        {
            gameManager.patientMovement.StartLopenNaarExit();
        }
    }

    /// <summary>
    /// Wordt aangeroepen wanneer een patiënt klaar is (goed, fout of reanimatie)
    /// </summary>
    public void PatientAfgehandeld(bool isCorrect, string titel, string bericht)
    {
        StopTimer();
        afgehandeldePatientenInLevel++;

        // Controleer of het level is voltooid
        if (afgehandeldePatientenInLevel >= benodigdAantalPatienten)
        {
            huidigLevel++;
            afgehandeldePatientenInLevel = 0;
            UpdateBenodigdAantal();

            string levelVoltooidBericht = $"{bericht}\n\nGEFELICITEERD! Level {huidigLevel - 1} voltooid! Level {huidigLevel} heeft nu {benodigdAantalPatienten} patiënten.";
            ToonFeedback(titel, levelVoltooidBericht);
        }
        else
        {
            UpdateLevelUI();
            ToonFeedback(titel, bericht);
        }
    }

    private void ToonFeedback(string titel, string bericht)
    {
        if (feedbackPaneel != null)
        {
            feedbackPaneel.SetActive(true);

            if (statusTitelTekst != null) statusTitelTekst.text = titel;
            if (feedbackBerichtTekst != null) feedbackBerichtTekst.text = bericht;
        }
    }
}