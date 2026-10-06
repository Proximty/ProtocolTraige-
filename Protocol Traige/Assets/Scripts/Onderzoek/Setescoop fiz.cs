using UnityEngine;

public class StethoscopeFix : MonoBehaviour
{
    private Vector3 origineleSchaal;

    void Awake()
    {
        origineleSchaal = transform.localScale;
    }

    void LateUpdate()
    {
        // Voorkomt dat het object vervormt als de parent-controller een afwijkende schaal heeft
        transform.localScale = origineleSchaal;
    }
}
