using UnityEngine;
using UnityEngine.AI;

public class PatientMovement : MonoBehaviour
{
    [Header("Bed & Positionering")]
    public Transform bedTransform;
    public float stopAfstand = 0.8f;

    [Header("Collider Alignment")]
    [Tooltip("Sleep hier de BoxCollider van het bed/matras naartoe")]
    public BoxCollider bedCollider;

    [Tooltip("Zet deze waarde op een negatief getal (bijv. -0.15 of -0.2) om haar wat meer naar beneden in het matras te zakken")]
    public float verticaleOffset = -0.1f;

    [Tooltip("Extra rotatie-offset als de animatie nog schuin ligt (bijv. Y = 90 of 180)")]
    public Vector3 ligRotatieOffset = Vector3.zero;

    private NavMeshAgent navAgent;
    private PatientAnimation animationController;
    private bool isOnderwegNaarBed = false;

    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        animationController = GetComponent<PatientAnimation>();
    }

    void Update()
    {
        if (isOnderwegNaarBed && navAgent != null && bedTransform != null)
        {
            if (!navAgent.pathPending && navAgent.remainingDistance <= stopAfstand)
            {
                StopEnGaLiggen();
            }
        }
    }

    public void StartLopenNaarBed()
    {
        if (bedTransform == null) return;

        if (navAgent != null)
        {
            navAgent.enabled = true;
            navAgent.isStopped = false;
            navAgent.SetDestination(bedTransform.position);
            isOnderwegNaarBed = true;

            if (animationController != null)
            {
                animationController.SpeelLopen();
            }
        }
    }

    public void StopEnGaLiggen()
    {
        isOnderwegNaarBed = false;

        // 1. Schakel NavMeshAgent uit
        if (navAgent != null)
        {
            navAgent.isStopped = true;
            navAgent.enabled = false;
        }

        // 2. Bepaal de exacte positie inclusief de gewenste Y-correctie
        if (bedTransform != null)
        {
            Vector3 doelPositie = bedTransform.position;

            if (bedCollider != null)
            {
                // Bovenkant BoxCollider + de verticale offset voor de gewenste inzinking
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

        // 3. Start de lig-animatie
        if (animationController != null)
        {
            animationController.SpeelLiggen();
        }
    }
}