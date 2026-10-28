using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class SessieResponse
{
    public int sessieId;
}

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    [Header("Web Server Instellingen")]
    // Vervang dit door de echte URL naar je webserver of map (bijv. op je lokale XAMPP/WAMP server)
    public string serverUrl = "http://localhost/protocol_triage_api/";

    private int huidigeSessieId = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(InitialiseerDatabaseServer());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator InitialiseerDatabaseServer()
    {
        // We sturen een verzoek om de database/tabellen klaar te zetten op de server
        using (UnityWebRequest www = UnityWebRequest.Get(serverUrl + "init.php"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[DatabaseManager] Kon geen verbinding maken met de server: " + www.error);
            }
            else
            {
                Debug.Log("[DatabaseManager] Server connectie succesvol: " + www.downloadHandler.text);
            }
        }
    }

    public void StartNieuweSessie()
    {
        StartCoroutine(StartSessieRoutine());
    }

    private IEnumerator StartSessieRoutine()
    {
        WWWForm form = new WWWForm();
        form.AddField("actie", "start_sessie");

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "api.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Fout bij StartNieuweSessie: " + www.error);
            }
            else
            {
                string jsonResult = www.downloadHandler.text;
                Debug.Log("Ontvangen JSON van server: " + jsonResult);

                // Gebruik Unity's JsonUtility om het nieuwe sessie-ID uit te lezen
                SessieResponse response = JsonUtility.FromJson<SessieResponse>(jsonResult);
                if (response != null)
                {
                    huidigeSessieId = response.sessieId;
                    Debug.Log($"[DatabaseManager] Nieuwe triage sessie gestart (ID: {huidigeSessieId})");
                }
            }
        }
    }

    public void SlaPatientCaseOp(int level, Patient correcteData, NtsUrgentie correcteUrgentie,
        int inA, int inB, int inC, int inD, int inE, NtsUrgentie inUrg,
        int fouten, bool isCorrect, float tijd)
    {
        if (huidigeSessieId == -1) return;

        StartCoroutine(SlaCaseRoutine(level, correcteData, correcteUrgentie, inA, inB, inC, inD, inE, inUrg, fouten, isCorrect, tijd));
    }

    private IEnumerator SlaCaseRoutine(int level, Patient correcteData, NtsUrgentie correcteUrgentie,
        int inA, int inB, int inC, int inD, int inE, NtsUrgentie inUrg,
        int fouten, bool isCorrect, float tijd)
    {
        WWWForm form = new WWWForm();
        form.AddField("actie", "sla_case_op");
        form.AddField("sessieId", huidigeSessieId);
        form.AddField("level", level);

        // Correcte data
        form.AddField("cA", correcteData.airway);
        form.AddField("cB", correcteData.breathing);
        form.AddField("cC", correcteData.circulation);
        form.AddField("cD", correcteData.disabillity);
        form.AddField("cE", correcteData.exposure);
        form.AddField("cU", (int)correcteUrgentie);

        // Invoer data
        form.AddField("iA", inA);
        form.AddField("iB", inB);
        form.AddField("iC", inC);
        form.AddField("iD", inD);
        form.AddField("iE", inE);
        form.AddField("iU", (int)inUrg);

        form.AddField("fouten", fouten);
        form.AddField("isCorrect", isCorrect ? 1 : 0);
        form.AddField("tijd", tijd.ToString(System.Globalization.CultureInfo.InvariantCulture));

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "api.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Fout bij SlaPatientCaseOp: " + www.error);
            }
            else
            {
                Debug.Log("[DatabaseManager] Patiënt-resultaat succesvol opgeslagen via JSON/Server!");
            }
        }
    }

    public void SluitSessie(int eindLevel)
    {
        if (huidigeSessieId == -1) return;
        StartCoroutine(SluitSessieRoutine(eindLevel));
    }

    private IEnumerator SluitSessieRoutine(int eindLevel)
    {
        WWWForm form = new WWWForm();
        form.AddField("actie", "sluit_sessie");
        form.AddField("sessieId", huidigeSessieId);
        form.AddField("eindLevel", eindLevel);

        using (UnityWebRequest www = UnityWebRequest.Post(serverUrl + "api.php", form))
        {
            yield return www.SendWebRequest();
        }
    }

    private void OnApplicationQuit()
    {
        if (huidigeSessieId != -1)
        {
            // Omdat de game afsluit kunnen coroutines soms niet afmaken, 
            // maar dit stuurt in ieder geval de laatste sluitingspoging.
            StartCoroutine(SluitSessieRoutine(-1));
        }
    }
}