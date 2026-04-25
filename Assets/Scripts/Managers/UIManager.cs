using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }

    [Header("Waiting Screen")]
    [SerializeField] private GameObject emptyPlayerCardPrefab;
    [SerializeField] private GameObject playerCardTransform;
    [SerializeField] private TextMeshProUGUI displayedPlayerCount;
    [SerializeField] private TextMeshProUGUI roomCode;

    [Header("Player Colors")]
    [SerializeField] private Color player1OutlineColor = new Color(0.811f, 0.066f, 0.066f, 1.000f);
    [SerializeField] private Color player2OutlineColor = new Color(0.067f, 0.812f, 0.771f, 1.000f);
    [SerializeField] private Color player3OutlineColor = new Color(0.102f, 1.000f, 0.036f, 1.000f);
    [SerializeField] private Color player4OutlineColor = new Color(0.872f, 0.612f, 0.064f, 1.000f);
    [SerializeField] private Color player5OutlineColor = new Color(0.507f, 0.226f, 0.736f, 1.000f);
    [SerializeField] private Color player6OutlineColor = new Color(0.995f, 1.000f, 0.036f, 1.000f);

    [Space]
    public GameObject fraud;
    [SerializeField] private GameObject v1;
    [SerializeField] KnockOverEffect knockOverScript;
    [SerializeField] private GameObject everything;
    [SerializeField] private VideoPlayer player;
    [SerializeField] private GameObject stuff;
    private void Awake()
    {
        displayedPlayerCount.text = "0";
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
    private void Start()
    {
        StartCoroutine(fraudDelay());
    }
    public void IncreaseDisplayedPlayerCount()
    {
        float currentIndex = NetworkManager.instance.GetPlayerList().Count;
        float indexToDisplay = currentIndex++;
        displayedPlayerCount.text = indexToDisplay.ToString();
    }
    public void UpdatePlayerCard(int index, Player player)
    {
        GameObject playerCard = playerCardTransform.transform.GetChild(index).gameObject;

        Outline cardOutline = playerCard.GetComponent<Outline>();
        cardOutline.enabled = true;
        cardOutline.effectColor = DetermineOutlineColor(index);

        Image playerImage = playerCard.transform.Find("Player Portrait").GetComponent<Image>();
        playerImage.sprite = player.selectedCharacter.characterImage;

        TextMeshProUGUI playerName = playerCard.GetComponentInChildren<TextMeshProUGUI>();
        playerName.text = player.GetPlayerName();
    }
    private Color DetermineOutlineColor(int index)
    {
        switch (index)
        {
            case 0:
                return player1OutlineColor;
            case 1:
                return player2OutlineColor;
            case 2:
                return player3OutlineColor;
            case 3:
                return player4OutlineColor;
            case 4:
                return player5OutlineColor;
            case 5:
                return player6OutlineColor;
        }

        Debug.Log("Error: No color found. Returning red.");
        return Color.red;
    }
    public void SetRoomCode(string code)
    {
        roomCode.text = code;
    }
    private IEnumerator fraudDelay()
    {
        yield return new WaitForSeconds(3f);
        knockOverScript.Fall();
        everything.SetActive(false);
        yield return new WaitForSeconds(1.5f);

        v1.SetActive(true);
        StartCoroutine(fraudCoroutine());
    }
    private IEnumerator fraudCoroutine()
    {
        fraud.SetActive(true);
        //AudioManager.instance.PlayBGM();
        AudioManager.instance.StopBGM();
        stuff.SetActive(true);
        player.Play();
        player.SetDirectAudioVolume(0, 2f);
        yield return new WaitForSeconds(4f);
        fraud.SetActive(false);
    }
}
