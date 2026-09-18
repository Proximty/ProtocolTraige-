using UnityEngine;

public class Gamemanger : MonoBehaviour
{
    Patient data = Resources.Load<Patient>("Data");
    PatientBehavior patient; 

    private void Urgentiebereken()
    {
        
        if (data != null)
        {
            patient.Ugerent = data.airway +
                      data.breathing +
                      data.exposure +
                      data.disabillity +
                      data.circulation;

        }
    }
}
