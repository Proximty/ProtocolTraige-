using UnityEngine;

[System.Serializable]
public class NotebookInput
{
    // De waarden/scores die de speler heeft ingevuld
    public int ingevuldeAirway;
    public int ingevuldeBreathing;
    public int ingevuldeCirculation;
    public int ingevuldeDisability;
    public int ingevuldeExposure;

    // De gekozen NTS-urgentieklasse
    public NtsUrgentie gekozenUrgentie;
}