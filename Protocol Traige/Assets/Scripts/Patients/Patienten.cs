using NUnit.Framework;
using UnityEngine;

public class Patienten : ScriptableObject
{
    protected int Airway = Random.Range(1, 10);
    protected int Breathing = Random.Range(1, 10);
    protected int Circulation = Random.Range(1, 10);
    protected int Disabillity = Random.Range(1, 10);
    protected int Exposure = Random.Range(1, 10);

}
