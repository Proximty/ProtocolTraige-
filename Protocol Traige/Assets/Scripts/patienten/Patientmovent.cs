using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class PatientMovement : MonoBehaviour
{
    [Header("Patient Status")]
    public NtsUrgentie huidigeUrgentie;

    [Header("Doel Locaties")]
    public Transform   bedTransform;
    public BoxCollider bedCollider;
    public Transform   exitPunt;
    public float       stopAfstand = 0.8f;

    [Header("Hoogte en Rotatie Offset")]
    public float   verticaleOffset    = -0.15f;
    public Vector3 ligRotatieOffset   = Vector3.zero;

    [Header("Performance")]
    [Tooltip("Hoe vaak per seconde de afstand gecontroleerd wordt. Standaard 10 = elke 0.1s ipv elke frame.")]
    public float afstandCheckHz = 10f;

    [Header("Unity Events")]
    public UnityEvent OnAangekomenBijBed;
    public UnityEvent OnAangekomenBijExit;

    private NavMeshAgent     _navAgent;
    private PatientAnimation _animCtrl;

    private enum BewegingStatus { Idle, LopenNaarBed, LopenNaarExit }
    private BewegingStatus _status = BewegingStatus.Idle;

    // Statische cache zodat FindWithTag maar 1x per sessie wordt aangeroepen
    private static Transform   _cachedBed;
    private static BoxCollider _cachedBedCollider;
    private static Transform   _cachedExit;

    private void Awake()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _animCtrl = GetComponent<PatientAnimation>();
        ZoekAutomatischLocaties();
    }

    private void OnEnable()
    {
        // Vervangt Update() polling: check op vaste Hz i.p.v. elke frame
        InvokeRepeating(nameof(ControleerAankomst), 0.1f, 1f / afstandCheckHz);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(ControleerAankomst));
    }

    private void ControleerAankomst()
    {
        if (_navAgent == null || !_navAgent.enabled || _navAgent.pathPending) return;
        if (_status == BewegingStatus.Idle) return;
        if (_navAgent.remainingDistance > stopAfstand) return;

        if (_status == BewegingStatus.LopenNaarBed)
            StopEnGaLiggen();
        else if (_status == BewegingStatus.LopenNaarExit)
            BereikExit();
    }

    public void ZoekAutomatischLocaties()
    {
        if (bedTransform == null)
        {
            if (_cachedBed == null)
            {
                GameObject obj = GameObject.FindWithTag("Bed");
                if (obj != null)
                {
                    _cachedBed         = obj.transform;
                    _cachedBedCollider = obj.GetComponent<BoxCollider>();
                }
            }
            bedTransform = _cachedBed;
            if (bedCollider == null) bedCollider = _cachedBedCollider;
        }

        if (exitPunt == null)
        {
            if (_cachedExit == null)
            {
                GameObject obj = GameObject.FindWithTag("Exit");
                if (obj != null) _cachedExit = obj.transform;
            }
            exitPunt = _cachedExit;
        }
    }

    public void InitialiseerPatient(Transform bed, BoxCollider collider, Transform exit)
    {
        bedTransform       = bed;
        bedCollider        = collider;
        exitPunt           = exit;
        _cachedBed         = bed;
        _cachedBedCollider = collider;
        _cachedExit        = exit;
    }

    public void WijsUrgentieEnBedToe(NtsUrgentie urgentie, Transform doelBed, BoxCollider doelCollider)
    {
        huidigeUrgentie = urgentie;
        if (doelBed      != null) bedTransform = doelBed;
        if (doelCollider != null) bedCollider  = doelCollider;
        StartLopenNaarBed();
    }

    public void StartLopenNaarBed()
    {
        ZoekAutomatischLocaties();
        if (bedTransform == null) { Debug.LogError("[PatientMovement] Bed Transform is leeg!"); return; }
        _status = BewegingStatus.LopenNaarBed;
        SetNavDoel(bedTransform.position);
        _animCtrl?.SpeelLopen();
    }

    public void StartLopenNaarExit()
    {
        ZoekAutomatischLocaties();
        if (exitPunt == null) { Debug.LogError("[PatientMovement] Exit Punt is leeg!"); return; }
        _status = BewegingStatus.LopenNaarExit;
        SetNavDoel(exitPunt.position);
        _animCtrl?.SpeelLopen();
    }

    public void StopEnGaLiggen()
    {
        _status = BewegingStatus.Idle;
        StopNav();

        if (bedTransform != null)
        {
            Vector3 doel = bedTransform.position;
            if (bedCollider != null)
                doel = new Vector3(doel.x, bedCollider.bounds.max.y + verticaleOffset, doel.z);
            else
                doel.y += verticaleOffset;

            transform.SetPositionAndRotation(
                doel,
                bedTransform.rotation * Quaternion.Euler(ligRotatieOffset));
        }

        _animCtrl?.SpeelLiggen();
        OnAangekomenBijBed?.Invoke();
    }

    private void SetNavDoel(Vector3 doel)
    {
        if (_navAgent == null) return;
        _navAgent.enabled   = true;
        _navAgent.isStopped = false;
        _navAgent.SetDestination(doel);
    }

    private void StopNav()
    {
        if (_navAgent == null) return;
        _navAgent.isStopped = true;
        _navAgent.enabled   = false;
    }

    private void BereikExit()
    {
        _status = BewegingStatus.Idle;
        StopNav();
        OnAangekomenBijExit?.Invoke();
        Destroy(gameObject, 0.5f);
    }
}