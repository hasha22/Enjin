using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class OutcomeManager : MonoBehaviour
{
    public static OutcomeManager instance { get; private set; }

    [Header("Values")]
    [SerializeField] public float enjinValue;
    [SerializeField] public float workerMoraleValue;
    [SerializeField] public float ethicValue;
    [SerializeField] public float profitValue;

    #region some bullshit
    // [Header("Enjin ending limits")]
    // [SerializeField] public float enjEndingEN;
    // [SerializeField] public float enjEndingWM;
    // [SerializeField] public float enjEndingET;
    // [SerializeField] public float enjEndingPR;

    // [Header("Neutral ending limits")]
    // [SerializeField] public float neutralEndingEN;
    // [SerializeField] public float neutralEndingWM;
    // [SerializeField] public float neutralEndingET;
    // [SerializeField] public float neutralEndingPR;

    // [Header("Independent ending limits")]
    // [SerializeField] public float indepeEndingEN;
    // [SerializeField] public float indepEndingWM;
    // [SerializeField] public float indepEndingET;
    // [SerializeField] public float indepEndingPR;
    #endregion

    [Header("Endings")]
    public Ending enjinEnding;
    public Ending neutralEnding;
    public Ending antiEnjinEnding;

    [Header("Text Refs")]
    public TextMeshProUGUI title;
    public TextMeshProUGUI mainText;

    private int currentLine = 0;
    private Ending selectedEnding;
    
    

    [Header("Settings")]
    public float typingSpeed;

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
        title.text = selectedEnding.endingName;
        StartCoroutine(TypeText(selectedEnding.endingText[currentLine], mainText));
    }

    
    public Ending DetermineOutcome()
    {
        if (true){return enjinEnding;}
        else{return enjinEnding;}
    }


    #region Text things
    public void NextLine()
    {
        StopAllCoroutines();
        mainText.text = "";
        currentLine++;
        if (currentLine >= selectedEnding.endingText.Count){mainText.text = "we done";}
        else {StartCoroutine(TypeText(selectedEnding.endingText[currentLine], mainText));}
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
