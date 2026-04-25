using System.Collections;
using TMPro;
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
        StartCoroutine(TimerCoroutine(seconds, false));
    }

    IEnumerator TimerCoroutine(int seconds, bool isDiscussionRound)
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
        if (!isDiscussionRound){GameManager.instance.TimesUp();}
    }
}
