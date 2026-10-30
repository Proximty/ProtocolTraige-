using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class StethoscopeInteraction : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private PatientBehavior huidigePatient;
    private Vector3 origineleSchaal;

    [Header("Auscultatie Bereik")]
    public float maximaleAfstandTotPatient = 2.5f;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        origineleSchaal = transform.localScale;

        // Zorg dat de stethoscoop netjes bereikbaar op de tafel ligt naast het klembord
        if (transform.position.y > 2.0f || transform.position.z > 0f)
        {
            transform.position = new Vector3(-1.65f, 0.95f, -4.05f);
            transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        }

        // Garandeer dat er een bruikbare collider op zit
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(0.25f, 0.25f, 0.25f);
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    void Start()
    {
        huidigePatient = VindPatientInBuurt();
    }

    void LateUpdate()
    {
        // Voorkomt dat het model vervormt als de parent controller een afwijkende schaal heeft
        if (transform.localScale != origineleSchaal && origineleSchaal != Vector3.zero)
        {
            transform.localScale = origineleSchaal;
        }
    }

    void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.activated.AddListener(OnButtonPressed);
        }
    }

    void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(OnButtonPressed);
        }
    }

    private void OnButtonPressed(ActivateEventArgs args)
    {
        VoerAuscultatieUit(args.interactorObject);
    }

    public void VoerAuscultatieUit(IXRInteractor interactor = null)
    {
        PatientBehavior patient = VindPatientInBuurt();
        if (patient == null)
        {
            Debug.Log("[Stethoscope] Geen patient dichtbij genoeg om te beluisteren.");
            var cb = FindFirstObjectByType<ClipboardInteraction>();
            if (cb != null) cb.ToonAuscultatieFeedback("Stethoscoop: Breng de stethoscoop naar de patient op het bed.");
            return;
        }

        // Speel stethoscoop auscultatie audio af (hartslag / ademhaling)
        patient.SpeelStethoscopeGeluidAf();

        // Haptische trilling op de VR controller
        GeefTrilling(interactor);

        // Feedback tonen op het klembord
        var clipboard = FindFirstObjectByType<ClipboardInteraction>();
        if (clipboard != null && patient.patientData != null)
        {
            string titel = !string.IsNullOrEmpty(patient.patientData.titel) ? patient.patientData.titel : "Patient";
            clipboard.ToonAuscultatieFeedback($"Auscultatie ({titel}): Ademhaling A:{patient.patientData.airway} B:{patient.patientData.breathing} | Hartslag C:{patient.patientData.circulation}");
        }

        Debug.Log($"<color=cyan>[Stethoscope]</color> Auscultatie uitgevoerd op {patient.name}!");
    }

    private void GeefTrilling(IXRInteractor interactor)
    {
        if (interactor is XRBaseInputInteractor inputInteractor)
        {
            inputInteractor.SendHapticImpulse(0.6f, 0.25f);
        }
    }

    private PatientBehavior VindPatientInBuurt()
    {
        // 1. Zoek via actieve GameManager
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null && gm.patientBehavior != null)
        {
            float d = Vector3.Distance(transform.position, gm.patientBehavior.transform.position);
            if (d <= maximaleAfstandTotPatient) return gm.patientBehavior;

            if (gm.bedTransform != null && Vector3.Distance(transform.position, gm.bedTransform.position) <= maximaleAfstandTotPatient + 0.5f)
                return gm.patientBehavior;
        }

        // 2. Zoek via alle PatientBehavior objecten in scene
        PatientBehavior[] allePatienten = FindObjectsByType<PatientBehavior>(FindObjectsSortMode.None);
        foreach (var p in allePatienten)
        {
            if (Vector3.Distance(transform.position, p.transform.position) <= maximaleAfstandTotPatient)
                return p;
        }

        return (gm != null) ? gm.patientBehavior : (allePatienten.Length > 0 ? allePatienten[0] : null);
    }

    private void OnTriggerEnter(Collider other)
    {
        PatientBehavior patient = other.GetComponentInParent<PatientBehavior>();
        if (patient != null) huidigePatient = patient;
    }

    private void OnTriggerExit(Collider other)
    {
        PatientBehavior patient = other.GetComponentInParent<PatientBehavior>();
        if (patient != null && patient == huidigePatient) huidigePatient = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        PatientBehavior patient = collision.collider.GetComponentInParent<PatientBehavior>();
        if (patient != null)
        {
            huidigePatient = patient;
            VoerAuscultatieUit(null);
        }
    }
}
