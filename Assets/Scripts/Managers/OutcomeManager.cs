using System.Collections;
using TMPro;
using UnityEngine;


public class OutcomeManager : MonoBehaviour
{
    public static OutcomeManager instance { get; private set; }

    [Header("Values")]
    [SerializeField] private float enjinValue;
    [SerializeField] private float workerMoraleValue;
    [SerializeField] private float ethicValue;
    [SerializeField] private float profitValue;

    [Header("Endings")]
    public Ending enjinEnding;
    public Ending neutralEnding;
    public Ending conservationEnding;

    [Header("Text Refs")]
    public TextMeshProUGUI mainText;
    public GameObject button;

    private int currentLine = 0;
    private Ending selectedEnding;

    [Header("Settings")]
    [SerializeField] private float typingSpeed;
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
        button.SetActive(true);
        SetValues(OutcomeValueContainer.instance.enjinValue,
                  OutcomeValueContainer.instance.workerMoraleValue,
                  OutcomeValueContainer.instance.ethicValue,
                  OutcomeValueContainer.instance.profitValue
                );
        StartEnding(DetermineOutcome());
    }

    public void SetValues(float enj, float morale, float ethic, float profit)
    {
        enjinValue = enj;
        workerMoraleValue = morale;
        ethicValue = ethic;
        profitValue = profit;
    }
    public void StartEnding(Ending end)
    {
        currentLine = 0;
        selectedEnding = end;
        Debug.Log("fired");
        StartCoroutine(TypeText(selectedEnding.endingText[currentLine], mainText));
    }
    
    public Ending DetermineOutcome()
    {
        if ((workerMoraleValue + ethicValue) / 2 < enjinValue) { return enjinEnding; }
        else if ((workerMoraleValue + ethicValue) / 2 > enjinValue) {return conservationEnding;}
        else {return neutralEnding;}
    }



    #region Text logic
    public void NextLine()
    {
        StopAllCoroutines();
        mainText.text = "";
        currentLine++;
        if (currentLine >= selectedEnding.endingText.Count) { mainText.text = "Thank you for playing our game!"; button.SetActive(false);}
        else { StartCoroutine(TypeText(selectedEnding.endingText[currentLine], mainText)); }
    }

    private IEnumerator TypeText(string text, TextMeshProUGUI targetText)
    {
        targetText.text = "";
        foreach (char c in text)
        {
            targetText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
    #endregion
}
