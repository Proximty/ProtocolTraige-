using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class StethoscopeInteraction : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private PatientBehavior huidigePatient;

    [Header("Instellingen")]
    private Vector3 origineleSchaal;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        origineleSchaal = transform.localScale;
    }

    void LateUpdate()
    {
        // Voorkomt dat het model vervormt als de parent controller een afwijkende schaal heeft
        if (transform.localScale != origineleSchaal)
        {
            transform.localScale = origineleSchaal;
        }
    }

    void OnEnable()
    {
        // 'activated' luistert automatisch naar de actieknop (B-knop / Trigger) als je het object vasthebt
        grabInteractable.activated.AddListener(OnButtonPressed);
    }

    void OnDisable()
    {
        grabInteractable.activated.RemoveListener(OnButtonPressed);
    }

    private void OnButtonPressed(ActivateEventArgs args)
    {
        // Speel de ademhalingsaudio op de patiënt alleen af als het borststuk bij de patiënt gehouden wordt
        if (huidigePatient != null)
        {
            // Pas deze functienaam eventueel aan naar de exacte functienaam in jouw patient audio script
            huidigePatient.SpeelStethoscopeGeluidAf();
            Debug.Log("[Stethoscope] B-knop ingedrukt: Ademhalingsgeluid afgespeeld!");
        }
    }

    // Detecteer of de kop van de stethoscoop tegen de patiënt aan gehouden wordt
    private void OnTriggerEnter(Collider other)
    {
        PatientBehavior patient = other.GetComponentInParent<PatientBehavior>();
        if (patient != null)
        {
            huidigePatient = patient;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PatientBehavior patient = other.GetComponentInParent<PatientBehavior>();
        if (patient != null && patient == huidigePatient)
        {
            huidigePatient = null;
        }
    }
}