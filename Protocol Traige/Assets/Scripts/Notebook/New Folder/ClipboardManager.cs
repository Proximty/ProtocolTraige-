using UnityEngine;
using UnityEngine.UI;

using TMPro;

/// <summary>
/// VR Clipboard – een klembord dat de speler kan oppakken (XR Grab Interactable),
/// de ABCDE-sliders en urgentie-dropdown kan aanpassen, en daarna via de
/// "Verstuur"-knop de invoer doorstuurt naar de GameManager.
///
/// HOE TE KOPPELEN IN DE EDITOR:
/// 1. Voeg dit script toe aan het Clipboard-prefab (met een XRGrabInteractable).
/// 2. Sleep de UI-elementen (sliders, dropdown, knop) in de Inspector-velden.
/// 3. Sleep de GameManager in het 'gameManager'-veld.
/// 4. Zorg dat de Canvas-render-mode op "World Space" staat zodat hij in VR zichtbaar is.
/// </summary>
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class ClipboardManager : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────
    // Inspector velden
    // ──────────────────────────────────────────────────────────────────

    [Header("Clipboard Canvas")]
    [Tooltip("Het World Space Canvas dat op het klembord zit")]
    public Canvas clipboardCanvas;

    [Header("ABCDE Sliders (waarden 1 – 10)")]
    public Slider airwaySlider;
    public Slider breathingSlider;
    public Slider circulationSlider;
    public Slider disabilitySlider;
    public Slider exposureSlider;

    [Header("Pijn & Urgentie")]
    [Tooltip("Pijnslider (0 – 10)")]
    public Slider pijnSlider;
    [Tooltip("Dropdown met opties: U0 Reanimatie, U1 Levensbedreigend, U2 Spoed, U3 Dringend, U4 NietDringend, U5 Advies")]
    public TMP_Dropdown urgentieDropdown;

    [Header("Labels – live feedback (optioneel)")]
    public TextMeshProUGUI airwayLabel;
    public TextMeshProUGUI breathingLabel;
    public TextMeshProUGUI circulationLabel;
    public TextMeshProUGUI disabilityLabel;
    public TextMeshProUGUI exposureLabel;
    public TextMeshProUGUI pijnLabel;

    [Header("Knoppen")]
    public Button verstuurKnop;
    public Button resetKnop;

    [Header("Feedback tekst (op het klembord zelf)")]
    [Tooltip("Optioneel tekstveld op het klembord voor korte bevestiging/fout")]
    public TextMeshProUGUI feedbackTekst;

    [Header("Manager Referentie")]
    public GameManager gameManager;

    // ──────────────────────────────────────────────────────────────────
    // Privé velden
    // ──────────────────────────────────────────────────────────────────
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _grabInteractable;
    private bool _isVastgepakt = false;

    // ──────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────

    private void Awake()
    {
        _grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    private void Start()
    {
        // ── Sliders instellen ──────────────────────────────────────────
        InstellenSlider(airwaySlider,      1, 10);
        InstellenSlider(breathingSlider,   1, 10);
        InstellenSlider(circulationSlider, 1, 10);
        InstellenSlider(disabilitySlider,  1, 10);
        InstellenSlider(exposureSlider,    1, 10);
        InstellenSlider(pijnSlider,        0, 10);

        // ── Live label-updates koppelen ────────────────────────────────
        KoppelSliderLabel(airwaySlider,      airwayLabel,      "A");
        KoppelSliderLabel(breathingSlider,   breathingLabel,   "B");
        KoppelSliderLabel(circulationSlider, circulationLabel, "C");
        KoppelSliderLabel(disabilitySlider,  disabilityLabel,  "D");
        KoppelSliderLabel(exposureSlider,    exposureLabel,    "E");
        KoppelSliderLabel(pijnSlider,        pijnLabel,        "Pijn");

        // ── Knoppen koppelen ───────────────────────────────────────────
        if (verstuurKnop != null)
            verstuurKnop.onClick.AddListener(OnVerstuurGedrukt);

        if (resetKnop != null)
            resetKnop.onClick.AddListener(ResetAlleSliders);

        // ── Grab events ────────────────────────────────────────────────
        if (_grabInteractable != null)
        {
            _grabInteractable.selectEntered.AddListener(_ => OnOppakken());
            _grabInteractable.selectExited.AddListener(_ => OnNeerleggen());
        }

        // ── Urgentie dropdown vullen (als hij leeg is) ─────────────────
        VulUrgentieDropdown();

        // ── Canvas altijd inschakelen ──────────────────────────────────
        if (clipboardCanvas != null)
            clipboardCanvas.gameObject.SetActive(true);

        WisFeedback();
        UpdateAllLabels();
    }

    // ──────────────────────────────────────────────────────────────────
    // Grab callbacks
    // ──────────────────────────────────────────────────────────────────

    private void OnOppakken()
    {
        _isVastgepakt = true;
        Debug.Log("[Clipboard] Klembord opgepakt.");
    }

    private void OnNeerleggen()
    {
        _isVastgepakt = false;
        Debug.Log("[Clipboard] Klembord neergelegd.");
    }

    // ──────────────────────────────────────────────────────────────────
    // Knop-acties
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verstuurt de ingevulde waarden naar de GameManager.
    /// Koppelbaar via de OnClick()-event van de Verstuur-knop.
    /// </summary>
    public void OnVerstuurGedrukt()
    {
        if (gameManager == null)
        {
            ToonFeedback("Fout: Geen GameManager gevonden!", Color.red);
            Debug.LogError("[Clipboard] gameManager is null – sleep hem in de Inspector!");
            return;
        }

        // Lees alle waarden uit
        int airway      = SliderWaarde(airwaySlider);
        int breathing   = SliderWaarde(breathingSlider);
        int circulation = SliderWaarde(circulationSlider);
        int disability  = SliderWaarde(disabilitySlider);
        int exposure    = SliderWaarde(exposureSlider);
        int pijn        = SliderWaarde(pijnSlider);

        NtsUrgentie urgentie = urgentieDropdown != null
            ? (NtsUrgentie)urgentieDropdown.value
            : NtsUrgentie.U5_Advies;

        Debug.Log($"[Clipboard] Versturen → A:{airway} B:{breathing} C:{circulation} D:{disability} E:{exposure} Pijn:{pijn} Urgentie:{urgentie}");

        // Stuur naar de GameManager
        gameManager.ControleerInvoer(airway, breathing, circulation, disability, exposure, pijn, urgentie);

        ToonFeedback("Verstuurd!", Color.green);
    }

    /// <summary>
    /// Reset alle sliders terug naar de standaard startwaarden.
    /// Koppelbaar aan een "Reset"-knop op het klembord.
    /// </summary>
    public void ResetAlleSliders()
    {
        SetSliderWaarde(airwaySlider,      1);
        SetSliderWaarde(breathingSlider,   1);
        SetSliderWaarde(circulationSlider, 1);
        SetSliderWaarde(disabilitySlider,  1);
        SetSliderWaarde(exposureSlider,    1);
        SetSliderWaarde(pijnSlider,        0);

        if (urgentieDropdown != null)
            urgentieDropdown.value = 0;

        UpdateAllLabels();
        WisFeedback();
        Debug.Log("[Clipboard] Alle waarden gereset.");
    }

    // ──────────────────────────────────────────────────────────────────
    // Hulp-methoden
    // ──────────────────────────────────────────────────────────────────

    private void InstellenSlider(Slider slider, int min, int max)
    {
        if (slider == null) return;
        slider.minValue    = min;
        slider.maxValue    = max;
        slider.wholeNumbers = true;
        slider.value       = min; // Startwaarde
    }

    private void KoppelSliderLabel(Slider slider, TextMeshProUGUI label, string prefix)
    {
        if (slider == null || label == null) return;

        // Initiële weergave
        label.text = $"{prefix}: {(int)slider.value}";

        // Live update bij verandering
        slider.onValueChanged.AddListener(waarde =>
        {
            label.text = $"{prefix}: {(int)waarde}";
        });
    }

    private void UpdateAllLabels()
    {
        UpdateLabel(airwayLabel,      airwaySlider,      "A");
        UpdateLabel(breathingLabel,   breathingSlider,   "B");
        UpdateLabel(circulationLabel, circulationSlider, "C");
        UpdateLabel(disabilityLabel,  disabilitySlider,  "D");
        UpdateLabel(exposureLabel,    exposureSlider,    "E");
        UpdateLabel(pijnLabel,        pijnSlider,        "Pijn");
    }

    private void UpdateLabel(TextMeshProUGUI label, Slider slider, string prefix)
    {
        if (label == null || slider == null) return;
        label.text = $"{prefix}: {(int)slider.value}";
    }

    private int SliderWaarde(Slider slider)
    {
        return slider != null ? (int)slider.value : 0;
    }

    private void SetSliderWaarde(Slider slider, int waarde)
    {
        if (slider != null) slider.value = waarde;
    }

    private void VulUrgentieDropdown()
    {
        if (urgentieDropdown == null) return;

        // Alleen vullen als de dropdown leeg is
        if (urgentieDropdown.options.Count == 0)
        {
            urgentieDropdown.ClearOptions();
            urgentieDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "U0 – Reanimatie (Onmiddellijk)",
                "U1 – Levensbedreigend (< 15 min)",
                "U2 – Spoed (< 1 uur)",
                "U3 – Dringend (< 3 uur)",
                "U4 – Niet Dringend (< 24 uur)",
                "U5 – Advies (geen schade)"
            });
            urgentieDropdown.value = 0;
        }
    }

    private void ToonFeedback(string bericht, Color kleur)
    {
        if (feedbackTekst == null) return;
        feedbackTekst.text  = bericht;
        feedbackTekst.color = kleur;

        // Wis automatisch na 3 seconden
        CancelInvoke(nameof(WisFeedback));
        Invoke(nameof(WisFeedback), 3f);
    }

    private void WisFeedback()
    {
        if (feedbackTekst != null) feedbackTekst.text = "";
    }
}
