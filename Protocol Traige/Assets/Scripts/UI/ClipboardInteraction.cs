using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class ClipboardInteraction : MonoBehaviour
{
    [Header("Canvas op het klembord")]
    public Canvas clipboardCanvas;

    [Header("ABCDE Sliders (1 - 10)")]
    public Slider airwaySlider;
    public Slider breathingSlider;
    public Slider circulationSlider;
    public Slider disabilitySlider;
    public Slider exposureSlider;

    [Header("Pijn & Urgentie")]
    public Slider pijnSlider;
    public TMP_Dropdown urgentieDropdown;

    [Header("Waarde-labels")]
    public TextMeshProUGUI airwayLabel;
    public TextMeshProUGUI breathingLabel;
    public TextMeshProUGUI circulationLabel;
    public TextMeshProUGUI disabilityLabel;
    public TextMeshProUGUI exposureLabel;
    public TextMeshProUGUI pijnLabel;

    [Header("Knoppen")]
    public Button verstuurKnop;
    public Button resetKnop;

    [Header("Feedback")]
    public TextMeshProUGUI feedbackTekst;
    public float feedbackDuur = 3f;

    [Header("Manager")]
    public GameManager gameManager;

    private XRGrabInteractable grabInteractable;
    private float feedbackTimer = 0f;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (clipboardCanvas == null)
            clipboardCanvas = GetComponentInChildren<Canvas>();
    }

    private void Start()
    {
        if (clipboardCanvas != null)
            clipboardCanvas.enabled = true;

        InstellenSlider(airwaySlider, 1, 10);
        InstellenSlider(breathingSlider, 1, 10);
        InstellenSlider(circulationSlider, 1, 10);
        InstellenSlider(disabilitySlider, 1, 10);
        InstellenSlider(exposureSlider, 1, 10);
        InstellenSlider(pijnSlider, 0, 10);

        KoppelLabelAanSlider(airwaySlider, airwayLabel);
        KoppelLabelAanSlider(breathingSlider, breathingLabel);
        KoppelLabelAanSlider(circulationSlider, circulationLabel);
        KoppelLabelAanSlider(disabilitySlider, disabilityLabel);
        KoppelLabelAanSlider(exposureSlider, exposureLabel);
        KoppelLabelAanSlider(pijnSlider, pijnLabel);

        if (verstuurKnop != null)
            verstuurKnop.onClick.AddListener(VerstuurGegevens);

        if (resetKnop != null)
            resetKnop.onClick.AddListener(ResetSliders);

        if (feedbackTekst != null)
            feedbackTekst.text = "";

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnOppakken);
            grabInteractable.selectExited.AddListener(OnNeerzetten);
        }

        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnOppakken);
            grabInteractable.selectExited.RemoveListener(OnNeerzetten);
        }
    }

    private void Update()
    {
        if (feedbackTimer > 0f)
        {
            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer <= 0f && feedbackTekst != null)
                feedbackTekst.text = "";
        }
    }

    private void OnOppakken(SelectEnterEventArgs args)
    {
        if (clipboardCanvas != null)
            clipboardCanvas.enabled = true;
    }

    private void OnNeerzetten(SelectExitEventArgs args)
    {
    }

    public void VerstuurGegevens()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager == null)
        {
            if (feedbackTekst != null)
            {
                feedbackTekst.text = "Geen GameManager gevonden!";
                feedbackTekst.color = Color.red;
                feedbackTimer = feedbackDuur;
            }
            Debug.LogError("[Clipboard] GameManager is niet ingesteld!");
            return;
        }

        int airway = airwaySlider != null ? (int)airwaySlider.value : 1;
        int breathing = breathingSlider != null ? (int)breathingSlider.value : 1;
        int circulation = circulationSlider != null ? (int)circulationSlider.value : 1;
        int disability = disabilitySlider != null ? (int)disabilitySlider.value : 1;
        int exposure = exposureSlider != null ? (int)exposureSlider.value : 1;
        int pijn = pijnSlider != null ? (int)pijnSlider.value : 0;
        NtsUrgentie urgentie = urgentieDropdown != null ? (NtsUrgentie)urgentieDropdown.value : NtsUrgentie.U5_Advies;

        gameManager.ControleerInvoer(airway, breathing, circulation, disability, exposure, pijn, urgentie);

        if (feedbackTekst != null)
        {
            feedbackTekst.text = "Gegevens verstuurd!";
            feedbackTekst.color = Color.green;
            feedbackTimer = feedbackDuur;
        }
    }

    public void ResetSliders()
    {
        if (airwaySlider != null) airwaySlider.value = airwaySlider.minValue;
        if (breathingSlider != null) breathingSlider.value = breathingSlider.minValue;
        if (circulationSlider != null) circulationSlider.value = circulationSlider.minValue;
        if (disabilitySlider != null) disabilitySlider.value = disabilitySlider.minValue;
        if (exposureSlider != null) exposureSlider.value = exposureSlider.minValue;
        if (pijnSlider != null) pijnSlider.value = 0;
        if (urgentieDropdown != null) urgentieDropdown.value = 0;

        if (feedbackTekst != null)
        {
            feedbackTekst.text = "Sliders gereset.";
            feedbackTekst.color = Color.white;
            feedbackTimer = feedbackDuur;
        }
    }

    private void InstellenSlider(Slider slider, int min, int max)
    {
        if (slider == null) return;
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = true;
        slider.value = min;
    }

    private void KoppelLabelAanSlider(Slider slider, TextMeshProUGUI label)
    {
        if (slider == null || label == null) return;
        label.text = ((int)slider.value).ToString();
        slider.onValueChanged.AddListener(waarde =>
        {
            label.text = ((int)waarde).ToString();
        });
    }
}
