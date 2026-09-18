using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Patient : ScriptableObject
{
    protected int Airway = Random.Range(1, 10);
    protected int Breathing = Random.Range(1, 10);
    protected int Cirulation = Random.Range(1, 10);
    protected int Disabillity = Random.Range(1, 10);
    protected int Exposure = Random.Range(1, 10);

    public int airway => Airway;
    public int breathing => Breathing;
    public int circulation => Cirulation;
    public int exposure => Exposure;
    public int disabillity => Disabillity;




}
