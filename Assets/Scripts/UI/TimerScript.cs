using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TimerScript : MonoBehaviour
{
    public static TimerScript instance;
    public TextMeshProUGUI timerText;

    void Awake()
    {
        if (instance == null){instance = this;}
    }

    public void StartTimer(int seconds)
    {
        StopAllCoroutines();
        StartCoroutine(TimerCoroutine(seconds));
    }

    IEnumerator TimerCoroutine(int seconds)
    {
        int timeLeft = seconds;
        while (timeLeft > 0)
        {
            int m = timeLeft / 60;
            int s = timeLeft % 60;
            timerText.text = m.ToString("00") + ":" + s.ToString("00");
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }
        timerText.text = "00:00";
        if (GameManager.instance.currentScreen == GameScreens.FirstPolicyVotingScreen || GameManager.instance.currentScreen == GameScreens.SecondPolicyVotingScreen)
        {
            GameUIManager.instance.TimerDone();
        }
        else if (GameManager.instance.currentScreen == GameScreens.DiscussionScreen)
        {
            
        }
    }
}
