using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Data")]
    [SerializeField] private string playerName;
    [SerializeField] private int firstOpinion; //made serializable for visualization purposes
    [SerializeField] private int secondOpinion;

    public string GetPlayerName()
    {
        return playerName;
    }
    public (int, int) GetPlayerOpinions()
    {
        return (firstOpinion, secondOpinion);
    }
}
