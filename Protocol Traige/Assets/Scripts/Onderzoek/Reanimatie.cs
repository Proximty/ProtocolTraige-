using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CPRBorstCollider : MonoBehaviour
{
    private PatientBehavior patient;

    [Header("Instellingen")]
    public float minimaleDiepte = 0.05f; // 5 cm naar beneden duwen
    public float hapticDuration = 0.15f;
    public float hapticIntensity = 0.7f;

    private float startY;
    private bool isIngedrukt = false;
    private ActionBasedController actieveController;

    void Start()
    {
        patient = GetComponentInParent<PatientBehavior>();
        startY = transform.position.y;
    }

    void OnTriggerEnter(Collider other)
    {
        ActionBasedController controller = other.GetComponentInParent<ActionBasedController>();
        if (controller != null)
        {
            actieveController = controller;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (actieveController == null || patient == null) return;

        // Mag alleen wanneer de patiënt U0_Reanimatie heeft en nog niet gereanimeerd is
        if (patient.toegewezenUrgentie != NtsUrgentie.U0_Reanimatie || patient.isGereanimeerd) return;

        float huidigeDiepte = startY - actieveController.transform.position.y;

        if (!isIngedrukt && huidigeDiepte >= minimaleDiepte)
        {
            isIngedrukt = true;

            // Registreer de compressie
            patient.RegistreerCompressie();

            // Trilling op de VR Controller
            actieveController.SendHapticImpulse(hapticIntensity, hapticDuration);
        }
    }

    void OnTriggerExit(Collider other)
    {
        ActionBasedController controller = other.GetComponentInParent<ActionBasedController>();
        if (controller != null && controller == actieveController)
        {
            actieveController = null;
            isIngedrukt = false;
        }
    }
}