using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ValueManager : MonoBehaviour
{
    [Header("Slider References")]
    public GameObject sliderContainer;
    public Slider enjinSlider;
    public Slider workerMoraleSlider;
    public Slider ethicSlider;
    public Slider profitSlider;

    [Header("mark,")]
    public GameObject markContainer;
    public RectTransform enijnMark;
    public RectTransform workMark;
    public RectTransform ethMark;
    public RectTransform profMark;

    [Header("Values")]
    [SerializeField] public float enjinValue;
    [SerializeField] public float workerMoraleValue;
    [SerializeField] public float ethicValue;
    [SerializeField] public float profitValue;

    [Header("PrevValues")]
    public float prevEnjinValue;
    public float prevWorkerMoraleValue;
    public float prevEthicValue;
    public float prevProfitValue;

    [Header("Settings")]
    public float barDelay;
    public float lerpDuration;
  
    

    public static ValueManager instance { get; private set; }

    public void Awake()
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

    public void SetValues(float enj, float morale, float ethic, float profit)
    {
        AssignPrevs();
        enjinValue = enj;
        workerMoraleValue = morale;
        ethicValue = ethic;
        profitValue = profit;
        StartCoroutine(UpdateSliderCoroutine());
    }

    public void ChangeValue(float enj, float morale, float ethic, float profit)
    {
        InstaSetSliders();
        AssignPrevs();
        enjinValue += enj;
        workerMoraleValue += morale;
        ethicValue += ethic;
        profitValue += profit;
        StartCoroutine(UpdateSliderCoroutine());
    }

    public void MakeBig()
    {
        RectTransform rect = sliderContainer.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, -26);
        rect.localScale = new Vector2(3, 3);
    }

    public void MakeSmall()
    {
        markContainer.SetActive(false);
        RectTransform rect = sliderContainer.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-781, -445);
        rect.localScale = new Vector2(1, 1);
    }

    public void InstaSetSliders()
    {
        StopAllCoroutines();
        enjinSlider.value = enjinValue;
        workerMoraleSlider.value = workerMoraleValue;
        ethicSlider.value = ethicValue;
        profitSlider.value = profitValue;
    }

    public void AssignPrevs()
    {
        prevEnjinValue = enjinSlider.value;
        prevWorkerMoraleValue = workerMoraleSlider.value;
        prevEthicValue = ethicSlider.value;
        prevProfitValue = profitSlider.value;
    }

    private IEnumerator UpdateSliderCoroutine()
    {
        yield return StartCoroutine(ShowMarks());
        yield return new WaitForSeconds(2);
        yield return StartCoroutine(SmoothSliderCoroutine(enjinSlider, enjinValue));
        yield return new WaitForSeconds(barDelay);
        yield return StartCoroutine(SmoothSliderCoroutine(workerMoraleSlider, workerMoraleValue));
        yield return new WaitForSeconds(barDelay);
        yield return StartCoroutine(SmoothSliderCoroutine(ethicSlider, ethicValue));
        yield return new WaitForSeconds(barDelay);
        yield return StartCoroutine(SmoothSliderCoroutine(profitSlider, profitValue));
    }

    private IEnumerator SmoothSliderCoroutine(Slider slider, float targetValue)
    {
        GameAudioManager.instance.PlaySFX(GameAudioManager.instance.barSfx, 0.3f);
        float startValue = slider.value;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / lerpDuration;
            float easedT = Mathf.SmoothStep(0f, 1f, t);
            slider.value = Mathf.Lerp(startValue, targetValue, easedT);
            yield return null;
        }

        slider.value = targetValue;
    }

    private IEnumerator ShowMarks()
    {
        markContainer.SetActive(true);
        float xPos = prevEnjinValue / 10 * 290;
        enijnMark.anchoredPosition = new Vector2(xPos, enijnMark.anchoredPosition.y);
        xPos = prevWorkerMoraleValue / 10 * 290;
        workMark.anchoredPosition = new Vector2(xPos, workMark.anchoredPosition.y);
        xPos = prevEthicValue / 10 * 290;
        ethMark.anchoredPosition = new Vector2(xPos, ethMark.anchoredPosition.y);
        xPos = prevProfitValue / 10 * 290;
        profMark.anchoredPosition = new Vector2(xPos, profMark.anchoredPosition.y);
        yield return null;
    }

    public void testSliders()
    {
        SetValues(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));
    }
}
