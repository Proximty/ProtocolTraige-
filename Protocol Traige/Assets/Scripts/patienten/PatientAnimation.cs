using UnityEngine;

public class PatientAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public Animator patientAnimator;

    [Header("Rotatie Correctie (Mixamo Fix)")]
    [Tooltip("Stel hier de hoek in om het gekantelde model recht te zetten (bijv. -90, 90, of 180)")]
    public Vector3 visualRotationOffset = new Vector3(0f, 0f, 0f);

    [Tooltip("Vink dit aan als het model gekanteld raakt op de X- of Z-as tijdens animaties")]
    public bool dwingRechtop = true;

    // Exacte namen van de booleans in je Animator Controller Parameters
    private const string BOOL_LOPEN = "IsLopen";
    private const string BOOL_LIGGEN = "IsLiggen";
    private const string BOOL_REANIMATIE = "IsGereanimeerd";

    private Transform visualModelTransform;

    void Awake()
    {
        if (patientAnimator == null)
            patientAnimator = GetComponentInChildren<Animator>();

        if (patientAnimator != null)
        {
            patientAnimator.applyRootMotion = false;

            // Sla het Transform-object op waar de Animator op staat (het visuele model)
            visualModelTransform = patientAnimator.transform;
        }
    }

    void LateUpdate()
    {
        // LateUpdate draait NÁ de animatieberekeningen van Unity.
        // Hier dwingen we het model om rechtop te blijven en corrigeren we de rotatie-offset.
        if (dwingRechtop && visualModelTransform != null)
        {
            // Corrigeer eventuele kanteling op de X- en Z-as en pas de offset toe
            Vector3 currentEuler = visualModelTransform.localEulerAngles;

            visualModelTransform.localRotation = Quaternion.Euler(
                visualRotationOffset.x,
                currentEuler.y + visualRotationOffset.y,
                visualRotationOffset.z
            );
        }
    }

    public void SpeelLopen()
    {
        if (patientAnimator == null) return;

        patientAnimator.SetBool(BOOL_LOPEN, true);
        patientAnimator.SetBool(BOOL_LIGGEN, false);
    }

    public void SpeelLiggen()
    {
        if (patientAnimator == null) return;

        patientAnimator.SetBool(BOOL_LIGGEN, true);
        patientAnimator.SetBool(BOOL_LOPEN, false);
    }

    public void TriggerReanimatieSucces()
    {
        if (patientAnimator == null) return;

        patientAnimator.SetBool(BOOL_REANIMATIE, true);
    }
}