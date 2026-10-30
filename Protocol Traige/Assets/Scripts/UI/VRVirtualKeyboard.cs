using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Compact en ergonomisch VR Virtueel Toetsenbord voor Meta Quest (en andere VR headsets).
/// </summary>
public class VRVirtualKeyboard : MonoBehaviour
{
    public static VRVirtualKeyboard Instance { get; private set; }

    [Header("Positie & Schaal")]
    [Tooltip("Vaste positie (X, Y) onderaan het canvas")]
    public Vector2 vasteCanvasPositie = new Vector2(0f, -115f);

    [Tooltip("Schaal van het toetsenbord")]
    public float toetsenbordSchaal = 0.85f;

    private GameObject keyboardRoot;
    private TextMeshProUGUI veldIndicatorLabel;
    private TMP_InputField actieveInput;
    private bool isShiftActief = false;
    private List<Button> letterKnoppen = new List<Button>();

    private readonly string[] rij1 = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "0" };
    private readonly string[] rij2 = { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" };
    private readonly string[] rij3 = { "A", "S", "D", "F", "G", "H", "J", "K", "L" };
    private readonly string[] rij4 = { "Z", "X", "C", "V", "B", "N", "M", "@", ".", "_" };

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        KoppelAlleInputFieldsInScene();
    }

    /// <summary>
    /// Koppel elk veld direct met zijn eigen referentie, zodat nooit het verkeerde veld wordt gekozen.
    /// </summary>
    public void KoppelAlleInputFieldsInScene()
    {
        TMP_InputField[] alleVelden = FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None);
        foreach (var veld in alleVelden)
        {
            TMP_InputField targetVeld = veld;

            // 1. Luister naar OnSelect
            targetVeld.onSelect.RemoveAllListeners();
            targetVeld.onSelect.AddListener((val) => OpenToetsenbordVoor(targetVeld));

            // 2. Voeg EventTrigger toe voor PointerClick zodat een VR Raycast-klik altijd direct registreert
            EventTrigger trigger = targetVeld.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = targetVeld.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            clickEntry.callback.AddListener((data) => OpenToetsenbordVoor(targetVeld));
            trigger.triggers.Clear();
            trigger.triggers.Add(clickEntry);
        }
    }

    public void OpenToetsenbordVoor(TMP_InputField veld)
    {
        if (veld == null) return;

        actieveInput = veld;
        actieveInput.ActivateInputField();

        Canvas parentCanvas = veld.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            if (keyboardRoot == null)
            {
                BouwToetsenbordUI(parentCanvas);
            }
            else
            {
                keyboardRoot.transform.SetParent(parentCanvas.transform, false);
                keyboardRoot.SetActive(true);
            }

            RectTransform rt = keyboardRoot.GetComponent<RectTransform>();
            rt.anchoredPosition = vasteCanvasPositie;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one * toetsenbordSchaal;

            // Toon de naam van het actieve veld op het toetsenbord
            if (veldIndicatorLabel != null)
            {
                string veldNaam = veld.gameObject.name.Replace("(Clone)", "").Trim();
                veldIndicatorLabel.text = "Invoer: " + veldNaam;
            }
        }
    }

    public void SluitToetsenbord()
    {
        if (keyboardRoot != null)
            keyboardRoot.SetActive(false);

        actieveInput = null;
    }

    // ─── Toets Invoer Handlers ───────────────────────────────────────────────

    public void TypTeken(string teken)
    {
        if (actieveInput == null) return;

        string invoer = isShiftActief ? teken.ToUpper() : teken.ToLower();
        actieveInput.text += invoer;
        actieveInput.caretPosition = actieveInput.text.Length;
        actieveInput.ForceLabelUpdate();
        actieveInput.onValueChanged?.Invoke(actieveInput.text);
    }

    public void Backspace()
    {
        if (actieveInput == null) return;

        string huidigeTekst = actieveInput.text;
        if (!string.IsNullOrEmpty(huidigeTekst))
        {
            actieveInput.text = huidigeTekst.Substring(0, huidigeTekst.Length - 1);
            actieveInput.caretPosition = actieveInput.text.Length;
            actieveInput.ForceLabelUpdate();
            actieveInput.onValueChanged?.Invoke(actieveInput.text);
        }
    }

    public void WisAlles()
    {
        if (actieveInput == null) return;

        actieveInput.text = "";
        actieveInput.caretPosition = 0;
        actieveInput.ForceLabelUpdate();
        actieveInput.onValueChanged?.Invoke(actieveInput.text);
    }

    public void Spatie()
    {
        if (actieveInput == null) return;

        actieveInput.text += " ";
        actieveInput.caretPosition = actieveInput.text.Length;
        actieveInput.ForceLabelUpdate();
        actieveInput.onValueChanged?.Invoke(actieveInput.text);
    }

    public void SchakelShift()
    {
        isShiftActief = !isShiftActief;

        foreach (var knop in letterKnoppen)
        {
            if (knop == null) continue;
            TextMeshProUGUI label = knop.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null && label.text.Length == 1 && char.IsLetter(label.text[0]))
            {
                label.text = isShiftActief ? label.text.ToUpper() : label.text.ToLower();
            }
        }
    }

    public void EnterGereed()
    {
        if (actieveInput != null)
        {
            actieveInput.onEndEdit?.Invoke(actieveInput.text);
            actieveInput.onSubmit?.Invoke(actieveInput.text);
        }
        SluitToetsenbord();
    }

    // ─── Programmatische UI ──────────────────────────────────────────────────

    private void BouwToetsenbordUI(Canvas canvas)
    {
        letterKnoppen.Clear();

        keyboardRoot = new GameObject("VR_VirtualKeyboard");
        keyboardRoot.transform.SetParent(canvas.transform, false);

        RectTransform rootRect = keyboardRoot.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(460f, 165f);
        rootRect.anchoredPosition = vasteCanvasPositie;

        Image bg = keyboardRoot.AddComponent<Image>();
        bg.color = new Color(0.10f, 0.12f, 0.16f, 0.95f);
        bg.raycastTarget = true;

        VerticalLayoutGroup vlg = keyboardRoot.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(6, 6, 6, 6);
        vlg.spacing = 3;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = true;

        // Bovenbalk: Veld indicator
        GameObject indicatorObj = new GameObject("VeldIndicator");
        indicatorObj.transform.SetParent(keyboardRoot.transform, false);
        veldIndicatorLabel = indicatorObj.AddComponent<TextMeshProUGUI>();
        veldIndicatorLabel.text = "Invoer";
        veldIndicatorLabel.fontSize = 11;
        veldIndicatorLabel.alignment = TextAlignmentOptions.Center;
        veldIndicatorLabel.color = new Color(0.7f, 0.85f, 1f);
        veldIndicatorLabel.raycastTarget = false;

        MaakRij(keyboardRoot.transform, rij1);
        MaakRij(keyboardRoot.transform, rij2);
        MaakRij(keyboardRoot.transform, rij3);
        MaakRij(keyboardRoot.transform, rij4);
        MaakFunctieRij(keyboardRoot.transform);
    }

    private void MaakRij(Transform parent, string[] tekens)
    {
        GameObject rijObj = new GameObject("Rij");
        rijObj.transform.SetParent(parent, false);

        HorizontalLayoutGroup hlg = rijObj.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 3;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        foreach (string t in tekens)
        {
            Button btn = MaakToetsKnop(rijObj.transform, t, new Color(0.20f, 0.23f, 0.29f));
            string gevangen = t;
            btn.onClick.AddListener(() => TypTeken(gevangen));

            if (t.Length == 1 && char.IsLetter(t[0]))
                letterKnoppen.Add(btn);
        }
    }

    private void MaakFunctieRij(Transform parent)
    {
        GameObject rijObj = new GameObject("FunctieRij");
        rijObj.transform.SetParent(parent, false);

        HorizontalLayoutGroup hlg = rijObj.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 4;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        Button shiftBtn = MaakToetsKnop(rijObj.transform, "Shift", new Color(0.28f, 0.35f, 0.45f));
        shiftBtn.onClick.AddListener(SchakelShift);

        Button spatieBtn = MaakToetsKnop(rijObj.transform, "Spatie", new Color(0.20f, 0.23f, 0.29f));
        spatieBtn.onClick.AddListener(Spatie);

        Button bsBtn = MaakToetsKnop(rijObj.transform, "⌫", new Color(0.55f, 0.25f, 0.25f));
        bsBtn.onClick.AddListener(Backspace);

        Button wisAllesBtn = MaakToetsKnop(rijObj.transform, "Wissen", new Color(0.65f, 0.18f, 0.18f));
        wisAllesBtn.onClick.AddListener(WisAlles);

        Button enterBtn = MaakToetsKnop(rijObj.transform, "✔ Klaar", new Color(0.18f, 0.48f, 0.26f));
        enterBtn.onClick.AddListener(EnterGereed);
    }

    private Button MaakToetsKnop(Transform parent, string labelTekst, Color kleur)
    {
        GameObject btnObj = new GameObject("Toets_" + labelTekst);
        btnObj.transform.SetParent(parent, false);

        Image img = btnObj.AddComponent<Image>();
        img.color = kleur;
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = kleur;
        colors.highlightedColor = kleur * 1.35f;
        colors.pressedColor = kleur * 0.65f;
        colors.selectedColor = kleur * 1.2f;
        btn.colors = colors;

        GameObject txtObj = new GameObject("Label");
        txtObj.transform.SetParent(btnObj.transform, false);

        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = labelTekst.ToLower();
        tmp.fontSize = 12;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero;

        return btn;
    }
}
