using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class StethoscopeInteraction : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private PatientBehavior huidigePatient;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        // Luister naar de Trigger-knop op de controller
        grabInteractable.activated.AddListener(OnTriggerPressed);
    }

    void OnDisable()
    {
        grabInteractable.activated.RemoveListener(OnTriggerPressed);
    }

    private void OnTriggerPressed(ActivateEventArgs args)
    {
        // Als we bij een patiënt staan, speel het geluid van die patiënt af
        if (huidigePatient != null)
        {
            huidigePatient.SpeelStethoscopeGeluidAf();
        }
    }

    // Detecteer of het borststuk de patiënt raakt via Triggers/Colliders
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