using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Detecteert CPR-compressies via de VR-controller.
/// PERFORMANCE: OnTriggerStay vervangen door OnTriggerEnter + diepte-tracking
/// zodat de dure GetComponentInParent-aanroep niet elke frame plaatsvindt.
/// </summary>
public class CPRBorstCollider : MonoBehaviour
{
    private PatientBehavior _patient;

    [Header("Instellingen")]
    public float minimaleDiepte = 0.05f;
    public float hapticDuration  = 0.15f;
    public float hapticIntensity = 0.7f;

    private float _startY;
    private bool  _isIngedrukt      = false;
    private bool  _controllerAanwezig = false;

    // Gecachte controller reference – haal 1x op bij Enter
    private ActionBasedController _actieveController;
    private Transform              _controllerTransform;

    private void Start()
    {
        _patient = GetComponentInParent<PatientBehavior>();
        _startY  = transform.position.y;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_actieveController != null) return; // al bezet

        var ctrl = other.GetComponentInParent<ActionBasedController>();
        if (ctrl == null) return;

        _actieveController   = ctrl;
        _controllerTransform = ctrl.transform;
        _controllerAanwezig  = true;
    }

    /// <summary>
    /// Update() ipv OnTriggerStay – draait alleen als controller aanwezig is,
    /// vermijdt dure physics-query elke stay-frame.
    /// </summary>
    private void Update()
    {
        if (!_controllerAanwezig || _actieveController == null) return;
        if (_patient == null) return;
        if (_patient.toegewezenUrgentie != NtsUrgentie.U0_Reanimatie || _patient.isGereanimeerd) return;

        float diepte = _startY - _controllerTransform.position.y;

        if (!_isIngedrukt && diepte >= minimaleDiepte)
        {
            _isIngedrukt = true;
            _patient.RegistreerCompressie();
            _actieveController.SendHapticImpulse(hapticIntensity, hapticDuration);
        }
        else if (_isIngedrukt && diepte < minimaleDiepte * 0.5f)
        {
            // Controller omhoog: reset voor volgende compressie
            _isIngedrukt = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var ctrl = other.GetComponentInParent<ActionBasedController>();
        if (ctrl != null && ctrl == _actieveController)
        {
            _actieveController   = null;
            _controllerTransform = null;
            _controllerAanwezig  = false;
            _isIngedrukt         = false;
        }
    }
}