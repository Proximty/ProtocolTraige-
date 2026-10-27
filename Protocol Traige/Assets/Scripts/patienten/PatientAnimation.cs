using UnityEngine;

public class PatientAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public Animator patientAnimator;

    [Header("Rotatie Correctie (Mixamo Fix)")]
    [Tooltip("Hoek om het model recht te zetten, bijv. -90 of 90")]
    public Vector3 visualRotationOffset = Vector3.zero;
    [Tooltip("Vink aan als het model kantelt tijdens animaties")]
    public bool dwingRechtop = true;

    private const string BOOL_LOPEN      = "IsLopen";
    private const string BOOL_LIGGEN     = "IsLiggen";
    private const string BOOL_REANIMATIE = "IsGereanimeerd";

    private Transform _visualModel;

    // Gecachte parameter IDs (sneller dan string-lookup elke frame)
    private int _idLopen;
    private int _idLiggen;
    private int _idReanimatie;

    // Gecachte euler Y zodat we niet elke frame localEulerAngles lezen
    private float _vorigeEulerY;

    private void Awake()
    {
        if (patientAnimator == null)
            patientAnimator = GetComponentInChildren<Animator>();

        if (patientAnimator != null)
        {
            patientAnimator.applyRootMotion = false;
            _visualModel = patientAnimator.transform;
        }

        // Hash IDs eenmalig cachen (sneller dan string lookup)
        _idLopen      = Animator.StringToHash(BOOL_LOPEN);
        _idLiggen     = Animator.StringToHash(BOOL_LIGGEN);
        _idReanimatie = Animator.StringToHash(BOOL_REANIMATIE);
    }

    private void LateUpdate()
    {
        // Alleen uitvoeren als rotatie-correctie nodig is
        if (!dwingRechtop || _visualModel == null) return;

        float nieuweY = _visualModel.localEulerAngles.y;

        // Alleen aanpassen als Y veranderd is (vermijdt onnodige rotatie-sets)
        if (Mathf.Abs(nieuweY - _vorigeEulerY) > 0.01f)
        {
            _vorigeEulerY = nieuweY;
            _visualModel.localRotation = Quaternion.Euler(
                visualRotationOffset.x,
                nieuweY + visualRotationOffset.y,
                visualRotationOffset.z);
        }
    }

    public void SpeelLopen()
    {
        if (patientAnimator == null) return;
        patientAnimator.SetBool(_idLopen,  true);
        patientAnimator.SetBool(_idLiggen, false);
    }

    public void SpeelLiggen()
    {
        if (patientAnimator == null) return;
        patientAnimator.SetBool(_idLiggen, true);
        patientAnimator.SetBool(_idLopen,  false);
    }

    public void TriggerReanimatieSucces()
    {
        if (patientAnimator != null)
            patientAnimator.SetBool(_idReanimatie, true);
    }
}