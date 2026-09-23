using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotebookUIGenerator : MonoBehaviour
{
    [Header("Referenties")]
    public GameManager gameManager;
    public TMP_Dropdown urgentieDropdown;
    public Button verstuurKnop;

    [Header("UI Layout Instellingen")]
    public int tekstGrootte = 24;
    public Color tekstKleur = Color.black;

    // Dictionary om gegenereerde sliders op te slaan
    private Dictionary<string, Slider> gegenereerdeSliders = new Dictionary<string, Slider>();

    private string[] parameterNamen = new string[]
    {
        "Airway",
        "Breathing",
        "Circulation",
        "Disability",
        "Exposure",
        "Pijnscore"
    };

    private void Start()
    {
        // 1. Bouw een layout container binnen het bestaande Canvas
        GameObject container = MaakLayoutContainer();

        // 2. Genereer alle 6 sliders en elementen volledig via code
        foreach (string paramNaam in parameterNamen)
        {
            MaakSliderRij(container.transform, paramNaam);
        }

        // 3. Koppel de verstuurknop
        if (verstuurKnop != null)
        {
            verstuurKnop.onClick.AddListener(OnVerstuurGeklikt);
        }
    }

    private GameObject MaakLayoutContainer()
    {
        GameObject container = new GameObject("SliderContainer", typeof(RectTransform));
        container.transform.SetParent(transform, false);

        RectTransform rt = container.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.2f);
        rt.anchorMax = new Vector2(0.95f, 0.95f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 15;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;

        return container;
    }

    private void MaakSliderRij(Transform parent, string paramNaam)
    {
        // --- Rij Container ---
        GameObject rijObj = new GameObject($"Rij_{paramNaam}", typeof(RectTransform));
        rijObj.transform.SetParent(parent, false);

        RectTransform rijRt = rijObj.GetComponent<RectTransform>();
        rijRt.sizeDelta = new Vector2(0, 45);

        HorizontalLayoutGroup hlg = rijObj.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.childControlWidth = false;
        hlg.childControlHeight = true;
        hlg.childForceExpandHeight = true;

        // --- 1. Label Tekst (bijv. "Airway") ---
        GameObject labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(rijObj.transform, false);
        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = paramNaam;
        labelText.fontSize = tekstGrootte;
        labelText.color = tekstKleur;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        labelObj.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 0);

        // --- 2. Slider Component ---
        GameObject sliderObj = new GameObject("Slider", typeof(RectTransform));
        sliderObj.transform.SetParent(rijObj.transform, false);
        sliderObj.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 0);

        // Achtergrond
        Image bgImage = sliderObj.AddComponent<Image>();
        bgImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);

        // Fill Area & Fill
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRt = fillArea.GetComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.sizeDelta = Vector2.zero;

        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.6f, 1f, 1f);
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.sizeDelta = Vector2.zero;

        // Handle
        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRt = handleArea.GetComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.sizeDelta = Vector2.zero;

        GameObject handle = new GameObject("Handle", typeof(RectTransform));
        handle.transform.SetParent(handleArea.transform, false);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;
        RectTransform handleRt = handle.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(30, 0);

        // Slider Functionaliteit
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = (paramNaam == "Pijnscore") ? 0 : 1;
        slider.maxValue = 10;
        slider.wholeNumbers = true;
        slider.value = slider.minValue;

        // --- 3. Waarde Tekst (bijv. "1") ---
        GameObject waardeObj = new GameObject("WaardeText", typeof(RectTransform));
        waardeObj.transform.SetParent(rijObj.transform, false);
        TextMeshProUGUI waardeText = waardeObj.AddComponent<TextMeshProUGUI>();
        waardeText.text = slider.value.ToString();
        waardeText.fontSize = tekstGrootte;
        waardeText.color = tekstKleur;
        waardeText.alignment = TextAlignmentOptions.MidlineCenter;
        waardeObj.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 0);

        // Update tekst direct als slider verschuift
        slider.onValueChanged.AddListener((val) =>
        {
            waardeText.text = val.ToString();
        });

        gegenereerdeSliders[paramNaam] = slider;
    }

    private void OnVerstuurGeklikt()
    {
        if (gameManager == null) return;

        int airway = GetSliderWaarde("Airway");
        int breathing = GetSliderWaarde("Breathing");
        int circulation = GetSliderWaarde("Circulation");
        int disability = GetSliderWaarde("Disability");
        int exposure = GetSliderWaarde("Exposure");
        int pijn = GetSliderWaarde("Pijnscore");
        NtsUrgentie urgentie = (NtsUrgentie)(urgentieDropdown != null ? urgentieDropdown.value : 0);

        gameManager.ControleerInvoer(airway, breathing, circulation, disability, exposure, pijn, urgentie);
    }

    private int GetSliderWaarde(string paramNaam)
    {
        if (gegenereerdeSliders.TryGetValue(paramNaam, out Slider slider))
        {
            return (int)slider.value;
        }
        return 1;
    }
}