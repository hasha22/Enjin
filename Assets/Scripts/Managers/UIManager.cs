using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }

    [Header("Waiting Screen")]
    [SerializeField] private GameObject emptyPlayerCardPrefab;
    [SerializeField] private GameObject playerCardTransform;
    [SerializeField] private TextMeshProUGUI roomCode;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void UpdatePlayerCard(int index, Player player)
    {
        GameObject playerCard = playerCardTransform.transform.GetChild(index).gameObject;

        Image playerImage = playerCard.transform.Find("Player Portrait").GetComponent<Image>();
        playerImage.sprite = player.selectedCharacter.characterImage;

        TextMeshProUGUI playerName = playerCard.GetComponentInChildren<TextMeshProUGUI>();
        playerName.text = player.GetPlayerName();
    }
    public void SetRoomCode(string code)
    {
        roomCode.text = $"Room code: {code}";
    }
}
