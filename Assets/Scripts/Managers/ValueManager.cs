using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ValueManager : MonoBehaviour
{
    [Header("Slider References")]
    [SerializeField] private GameObject sliderContainer;
    [SerializeField] private Slider enjinSlider;
    [SerializeField] private Slider workerMoraleSlider;
    [SerializeField] private Slider ethicSlider;
    [SerializeField] private Slider profitSlider;

    [Header("mark,")]
    [SerializeField] private GameObject markContainer;
    [SerializeField] private RectTransform enijnMark;
    [SerializeField] private RectTransform workMark;
    [SerializeField] private RectTransform ethMark;
    [SerializeField] private RectTransform profMark;

    [Header("Values")]
    [SerializeField] private float enjinValue;
    [SerializeField] private float workerMoraleValue;
    [SerializeField] private float ethicValue;
    [SerializeField] private float profitValue;

    [Header("Previous Values")]
    private float prevEnjinValue;
    private float prevWorkerMoraleValue;
    private float prevEthicValue;
    private float prevProfitValue;

    [Header("Settings")]
    [SerializeField] private float barDelay;
    [SerializeField] private float lerpDuration;

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
        InstantlySetSliders();
    }
    public void SetValues(float enj, float morale, float ethic, float profit)
    {
        AssignPreviousValues();
        enjinValue = enj;
        workerMoraleValue = morale;
        ethicValue = ethic;
        profitValue = profit;
        StartCoroutine(UpdateSliderCoroutine());
    }
    public void AssignOutcomeValues()
    {
        OutcomeValueContainer.instance.SetValues(enjinValue, workerMoraleValue, ethicValue, profitValue);
    }
    public void ChangeValue(float enj, float morale, float ethic, float profit)
    {
        InstantlySetSliders();
        AssignPreviousValues();
        enjinValue += enj;
        workerMoraleValue += morale;
        ethicValue += ethic;
        profitValue += profit;
        StartCoroutine(UpdateSliderCoroutine());
    }
    public void MakeBig()
    {
        RectTransform rect = sliderContainer.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-15, -40);
        rect.localScale = new Vector2(3, 3);
        rect.localRotation = Quaternion.Euler(0, 0, 0);
    }
    public void MakeSmall()
    {
        markContainer.SetActive(false);
        RectTransform rect = sliderContainer.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-762, -358);
        rect.localScale = new Vector2(0.9f, 0.9f);
        rect.localRotation = Quaternion.Euler(0, 0, 90);
    }
    public void InstantlySetSliders()
    {
        StopAllCoroutines();
        enjinSlider.value = enjinValue;
        workerMoraleSlider.value = workerMoraleValue;
        ethicSlider.value = ethicValue;
        profitSlider.value = profitValue;
    }

    public void AssignPreviousValues()
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
        // markContainer.SetActive(true);
        // float xPos = prevEnjinValue / 10 * 290;
        // enijnMark.anchoredPosition = new Vector2(xPos, enijnMark.anchoredPosition.y);
        // xPos = prevWorkerMoraleValue / 10 * 290;
        // workMark.anchoredPosition = new Vector2(xPos, workMark.anchoredPosition.y);
        // xPos = prevEthicValue / 10 * 290;
        // ethMark.anchoredPosition = new Vector2(xPos, ethMark.anchoredPosition.y);
        // xPos = prevProfitValue / 10 * 290;
        // profMark.anchoredPosition = new Vector2(xPos, profMark.anchoredPosition.y);
        yield return null;
    }
    public void testSliders()
    {
        SetValues(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));
    }
}
