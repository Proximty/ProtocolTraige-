using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotebookUI : MonoBehaviour
{
    [Header("ABCDE Sliders (1 - 10)")]
    public Slider airwaySlider;
    public Slider breathingSlider;
    public Slider circulationSlider;
    public Slider disabilitySlider;
    public Slider exposureSlider;

    [Header("Pijn & Urgentie")]
    public Slider pijnSlider;                  // Score 0 - 10
    public TMP_Dropdown urgentieDropdown;      // Keuze U0 t/m U5

    [Header("Manager")]
    public GameManager gameManager;

    private void Start()
    {
        // Stel automatisch alle sliders in op hele getallen (integers)
        InstellenSlider(airwaySlider, 1, 10);
        InstellenSlider(breathingSlider, 1, 10);
        InstellenSlider(circulationSlider, 1, 10);
        InstellenSlider(disabilitySlider, 1, 10);
        InstellenSlider(exposureSlider, 1, 10);
        InstellenSlider(pijnSlider, 0, 10);
    }

    private void InstellenSlider(Slider slider, int min, int max)
    {
        if (slider == null) return;
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = true; // Zorgt voor hele getallen (1, 2, 3...)
    }

    // Koppel deze functie aan de OnClick() van de "Verstuur / Controleer" knop op je VR-notitieblok
    public void OnVerstuurKnop()
    {
        if (gameManager != null)
        {
            gameManager.ControleerInvoer(
                (int)airwaySlider.value,
                (int)breathingSlider.value,
                (int)circulationSlider.value,
                (int)disabilitySlider.value,
                (int)exposureSlider.value,
                (int)pijnSlider.value,
                (NtsUrgentie)urgentieDropdown.value
            );
        }
    }
}