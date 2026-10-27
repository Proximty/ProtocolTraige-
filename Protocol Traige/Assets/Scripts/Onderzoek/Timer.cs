using UnityEngine;
using TMPro;

public class TriageTimerUI : MonoBehaviour
{
    [Header("Level en Progressie")]
    public int   huidigLevel             = 1;
    public int   basisAantalPatienten    = 3;
    private int  _benodigdAantalPatienten;
    private int  _afgehandeldePatientenInLevel = 0;

    [Header("Timer Instellingen")]
    public float maxTijdInSeconden = 90f;
    private float _resterendeTijd;
    private bool  _isTimerActief   = false;

    [Header("Reanimatie (CPR) Settings")]
    public float benodigdeCprTijd    = 60f;
    private float _cprVoortgang      = 0f;
    private bool  _isReanimatieActief = false;
    public float  cprSnelheidPerDruk = 1.0f;

    [Header("UI Elementen - Tekst")]
    public TextMeshProUGUI timerTekst;
    public TextMeshProUGUI levelTekst;

    [Header("UI Elementen - Feedback Panel")]
    public GameObject      feedbackPaneel;
    public TextMeshProUGUI statusTitelTekst;
    public TextMeshProUGUI feedbackBerichtTekst;

    [Header("Referenties")]
    public GameManager gameManager;

    // Gecachte string-format resultaten om GC-allocaties te verminderen
    private int   _voigeMinuten = -1;
    private int   _vorigSeconden = -1;

    // =====================================================

    private void Start()
    {
        if (feedbackPaneel != null) feedbackPaneel.SetActive(false);
        ResetLevelProgressie();
        HerstartTimer();
    }

    private void Update()
    {
        // Guard: alleen uitvoeren als er iets actief is
        if (!_isTimerActief && !_isReanimatieActief) return;

        if (_isTimerActief)
        {
            _resterendeTijd -= Time.deltaTime;

            if (_resterendeTijd <= 0f)
            {
                _resterendeTijd = 0f;
                _isTimerActief  = false;
                UpdateTimerUI();
                TijdVerstreken();
            }
            else
            {
                UpdateTimerUI();
            }
        }

        if (_isReanimatieActief)
        {
            // Space = fallback voor desktop testen
            if (Input.GetKeyDown(KeyCode.Space))
                VoerCompressieUit();

            UpdateCprUI();

            if (_cprVoortgang >= benodigdeCprTijd)
            {
                _isReanimatieActief = false;
                AfrondenReanimatie();
            }
        }
    }

    // =====================================================
    //  LEVEL PROGRESSIE
    // =====================================================

    public void ResetLevelProgressie()
    {
        huidigLevel                    = 1;
        _afgehandeldePatientenInLevel  = 0;
        UpdateBenodigdAantal();
    }

    private void UpdateBenodigdAantal()
    {
        _benodigdAantalPatienten = basisAantalPatienten + (huidigLevel - 1);
        UpdateLevelUI();
    }

    private void UpdateLevelUI()
    {
        if (levelTekst != null)
            levelTekst.text = $"Level {huidigLevel} - Patient: {_afgehandeldePatientenInLevel}/{_benodigdAantalPatienten}";
    }

    // =====================================================
    //  TIMER
    // =====================================================

    public void HerstartTimer()
    {
        _isReanimatieActief = false;
        _cprVoortgang       = 0f;
        _resterendeTijd     = maxTijdInSeconden;
        _isTimerActief      = true;
        _voigeMinuten       = -1; // forceer UI-update
        _vorigSeconden      = -1;

        if (feedbackPaneel != null) feedbackPaneel.SetActive(false);
        UpdateLevelUI();
    }

    public void StopTimer()
    {
        _isTimerActief      = false;
        _isReanimatieActief = false;
    }

    private void UpdateTimerUI()
    {
        int minuten  = Mathf.FloorToInt(_resterendeTijd / 60f);
        int seconden = Mathf.FloorToInt(_resterendeTijd % 60f);

        // Alleen UI aanpassen als de waarde veranderd is (vermijdt string-GC elke frame)
        if (minuten == _voigeMinuten && seconden == _vorigSeconden) return;

        _voigeMinuten  = minuten;
        _vorigSeconden = seconden;

        if (timerTekst != null)
            timerTekst.text = $"Tijd: {minuten:00}:{seconden:00}";
    }

    public float GetVerstrekenTijd()
    {
        return Mathf.Max(0f, maxTijdInSeconden - _resterendeTijd);
    }

    // =====================================================
    //  CPR
    // =====================================================

    public void VoerCompressieUit()
    {
        if (!_isReanimatieActief) return;
        _cprVoortgang = Mathf.Clamp(_cprVoortgang + cprSnelheidPerDruk, 0f, benodigdeCprTijd);
    }

    private void UpdateCprUI()
    {
        int pct = Mathf.FloorToInt((_cprVoortgang / benodigdeCprTijd) * 100f);
        if (timerTekst != null)
            timerTekst.text = $"REANIMATIE: {pct}%";
    }

    private void TijdVerstreken()
    {
        NtsUrgentie urgentie = NtsUrgentie.U5_Advies;

        if (gameManager?.huidigePatient != null)
        {
            int score = gameManager.huidigePatient.airway
                      + gameManager.huidigePatient.breathing
                      + gameManager.huidigePatient.circulation
                      + gameManager.huidigePatient.disabillity
                      + gameManager.huidigePatient.exposure;
            urgentie = score >= 26 ? NtsUrgentie.U0_Reanimatie : NtsUrgentie.U5_Advies;
        }

        if (urgentie == NtsUrgentie.U0_Reanimatie ||
            urgentie == NtsUrgentie.U1_Levensbedreigend ||
            urgentie == NtsUrgentie.U2_Spoed)
        {
            StartReanimatie();
        }
        else
        {
            PatientAfgehandeld(false, "PATIENT WEGGEGAAN", "Je hebt er te lang over gedaan. De patient is boos vertrokken.");
            gameManager?.patientMovement?.StartLopenNaarExit();
        }
    }

    private void StartReanimatie()
    {
        _isReanimatieActief = true;
        _cprVoortgang       = 0f;
        gameManager?.patientMovement?.StopEnGaLiggen();
    }

    private void AfrondenReanimatie()
    {
        PatientAfgehandeld(false, "REANIMATIE AFGEROND", "Patient is gereanimeerd, maar de triage is mislukt door tijdgebrek.");
        gameManager?.patientMovement?.StartLopenNaarExit();
    }

    // =====================================================
    //  PATIENT AFGEHANDELD
    // =====================================================

    public void PatientAfgehandeld(bool isCorrect, string titel, string bericht)
    {
        StopTimer();
        _afgehandeldePatientenInLevel++;

        if (_afgehandeldePatientenInLevel >= _benodigdAantalPatienten)
        {
            huidigLevel++;
            _afgehandeldePatientenInLevel = 0;

            if (DatabaseManager.Instance != null)
                DatabaseManager.Instance.SluitSessie(huidigLevel);

            UpdateBenodigdAantal();

            string lvlBericht = $"{bericht}\n\nGEFELICITEERD! Level {huidigLevel - 1} voltooid! Level {huidigLevel} heeft nu {_benodigdAantalPatienten} patienten.";
            ToonFeedback(titel, lvlBericht);
        }
        else
        {
            UpdateLevelUI();
            ToonFeedback(titel, bericht);
        }
    }

    private void ToonFeedback(string titel, string bericht)
    {
        if (feedbackPaneel == null) return;
        feedbackPaneel.SetActive(true);
        if (statusTitelTekst    != null) statusTitelTekst.text    = titel;
        if (feedbackBerichtTekst != null) feedbackBerichtTekst.text = bericht;
    }
}