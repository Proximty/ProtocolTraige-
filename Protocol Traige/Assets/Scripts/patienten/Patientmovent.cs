using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class PatientMovement : MonoBehaviour
{
    [Header("Patient Status")]
    public NtsUrgentie huidigeUrgentie;

    [Header("Doel Locaties")]
    public Transform bedTransform;
    public BoxCollider bedCollider;
    public Transform exitPunt;
    public float stopAfstand = 0.8f;

    [Header("Hoogte & Rotatie Offset")]
    public float verticaleOffset = -0.15f;
    public Vector3 ligRotatieOffset = Vector3.zero;

    [Header("Unity Events")]
    public UnityEvent OnAangekomenBijBed;
    public UnityEvent OnAangekomenBijExit;

    private NavMeshAgent navAgent;
    private PatientAnimation animationController;
    private enum BewegingStatus { Idle, LopenNaarBed, LopenNaarExit }
    private BewegingStatus huidigeStatus = BewegingStatus.Idle;

    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animationController = GetComponent<PatientAnimation>();

        // Automatische fallback: Zoek de doelen in de scene als ze niet via de Spawner/Inspector zijn ingevuld!
        ZoekAutomatischLocaties();
    }

    /// <summary>
    /// Zoekt automatisch naar GameObjects als ze 'None' zijn
    /// </summary>
    public void ZoekAutomatischLocaties()
    {
        // Zoek het bed als bedTransform leeg is
        if (bedTransform == null)
        {
            GameObject bedObj = GameObject.FindWithTag("Bed");
            if (bedObj != null)
            {
                bedTransform = bedObj.transform;
                if (bedCollider == null)
                    bedCollider = bedObj.GetComponent<BoxCollider>();
            }
        }

        // Zoek de exit als exitPunt leeg is
        if (exitPunt == null)
        {
            GameObject exitObj = GameObject.FindWithTag("Exit");
            if (exitObj != null)
                exitPunt = exitObj.transform;
        }
    }

    public void InitialiseerPatiënt(Transform bed, BoxCollider collider, Transform exit)
    {
        bedTransform = bed;
        bedCollider = collider;
        exitPunt = exit;
    }

    public void WijsUrgentieEnBedToe(NtsUrgentie urgentie, Transform doelBed, BoxCollider doelCollider)
    {
        huidigeUrgentie = urgentie;
        if (doelBed != null) bedTransform = doelBed;
        if (doelCollider != null) bedCollider = doelCollider;

        StartLopenNaarBed();
    }

    public void StartLopenNaarBed()
    {
        // Extra check voor als ze nog steeds null zijn
        ZoekAutomatischLocaties();

        if (bedTransform == null)
        {
            Debug.LogError($"[PatientMovement] KAN NIET LOPEN: Bed Transform is 'None' op {gameObject.name}!");
            return;
        }

        if (navAgent != null)
        {
            navAgent.enabled = true;
            navAgent.isStopped = false;
            navAgent.SetDestination(bedTransform.position);
            huidigeStatus = BewegingStatus.LopenNaarBed;

            if (animationController != null)
                animationController.SpeelLopen();
        }
    }

    public void StartLopenNaarExit()
    {
        ZoekAutomatischLocaties();

        if (exitPunt == null)
        {
            Debug.LogError($"[PatientMovement] KAN NIET WEGLOPEN: Exit Punt is 'None' op {gameObject.name}!");
            return;
        }

        if (navAgent != null)
        {
            navAgent.enabled = true;
            navAgent.isStopped = false;
            navAgent.SetDestination(exitPunt.position);
            huidigeStatus = BewegingStatus.LopenNaarExit;

            if (animationController != null)
                animationController.SpeelLopen();
        }
    }

    void Update()
    {
        if (navAgent == null || !navAgent.enabled || navAgent.pathPending) return;

        if (navAgent.remainingDistance <= stopAfstand)
        {
            if (huidigeStatus == BewegingStatus.LopenNaarBed)
            {
                StopEnGaLiggen();
            }
            else if (huidigeStatus == BewegingStatus.LopenNaarExit)
            {
                BereikExit();
            }
        }
    }

    public void StopEnGaLiggen()
    {
        huidigeStatus = BewegingStatus.Idle;

        if (navAgent != null)
        {
            navAgent.isStopped = true;
            navAgent.enabled = false;
        }

        if (bedTransform != null)
        {
            Vector3 doelPositie = bedTransform.position;

            if (bedCollider != null)
            {
                float matrasBovenkantY = bedCollider.bounds.max.y + verticaleOffset;
                doelPositie = new Vector3(bedTransform.position.x, matrasBovenkantY, bedTransform.position.z);
            }
            else
            {
                doelPositie.y += verticaleOffset;
            }

            transform.position = doelPositie;
            transform.rotation = bedTransform.rotation * Quaternion.Euler(ligRotatieOffset);
        }

        if (animationController != null)
            animationController.SpeelLiggen();

        OnAangekomenBijBed?.Invoke();
    }

    private void BereikExit()
    {
        huidigeStatus = BewegingStatus.Idle;

        if (navAgent != null)
            navAgent.isStopped = true;

        OnAangekomenBijExit?.Invoke();
        Destroy(gameObject, 0.5f);
    }
}