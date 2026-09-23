using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OnderzoekMenuUI : MonoBehaviour
{
    [Header("Referentie naar de Patiënt")]
    public PatientBehavior huidigePatient;

    [Header("UI Elementen")]
    public GameObject menuCanvas;
    public TextMeshProUGUI spraakText;
    public Button disabilityKnop;
    public Button exposureKnop;

    private void Start()
    {
        // Koppel de knoppen direct aan de UI functies
        if (disabilityKnop != null)
            disabilityKnop.onClick.AddListener(OnDisabilityGekozen);

        if (exposureKnop != null)
            exposureKnop.onClick.AddListener(OnExposureGekozen);
    }

    /// <summary>
    /// Roep deze functie aan wanneer de speler in VR op de patiënt tikt.
    /// </summary>
    public void OpenMenu()
    {
        if (menuCanvas != null)
        {
            menuCanvas.SetActive(true);
            spraakText.text = "Waar wil je informatie over hebben?";
        }
    }

    private void OnDisabilityGekozen()
    {
        if (huidigePatient != null)
        {
            // Haal de reactie op uit de patiënt en toon deze in het scherm
            spraakText.text = huidigePatient.GetDisabilityReactie();
        }
    }

    private void OnExposureGekozen()
    {
        if (huidigePatient != null)
        {
            // Haal de reactie op uit de patiënt en toon deze in het scherm
            spraakText.text = huidigePatient.GetExposureReactie();
        }
    }

    public void SluitMenu()
    {
        if (menuCanvas != null)
            menuCanvas.SetActive(false);
    }
}