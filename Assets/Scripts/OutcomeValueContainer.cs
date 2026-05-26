using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class OutcomeValueContainer : MonoBehaviour
{
    public static OutcomeValueContainer instance;
    [Header("Values")]
    [SerializeField] public float enjinValue;
    [SerializeField] public float workerMoraleValue;
    [SerializeField] public float ethicValue;
    [SerializeField] public float profitValue;

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

    public void SetValues(float enj, float morale, float ethic, float profit)
    {
        enjinValue = enj;
        workerMoraleValue = morale;
        ethicValue = ethic;
        profitValue = profit;
    }

}