using UnityEngine;

public class OutcomeManager : MonoBehaviour
{
    public static OutcomeManager instance { get; private set; }

    [Header("Outcome Variables")]
    public float productivity;
    public float sustainability;
    public float AIusage;
    public float privacy;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetValues(float prod, float sustain, float AI, float priv)
    {
        productivity = prod;
        sustainability = sustain;
        AIusage = AI;
        privacy = priv;
    }

    public void UpdateValues(float prod, float sustain, float AI, float priv)
    {
        productivity += prod;
        sustainability += sustain;
        AIusage += AI;
        privacy += priv;
    }

    public void ResetValues()
    {
        productivity += 0;
        sustainability += 0;
        AIusage += 0;
        privacy += 0;
    }

    public void DetermineOutcome()
    {

    }
}
