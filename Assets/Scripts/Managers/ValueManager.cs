using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ValueManager : MonoBehaviour
{
    [Header("Slider References")]
    public GameObject sliderContainer;
    public Slider enjinSlider;
    public Slider competitiveSlider;
    public Slider ethicSlider;
    public Slider profitSlider;

    [Header("Values")]
    public float enjinValue { get; private set; }
    public float competitiveValue { get; private set; }
    public float ethicValue { get; private set; }
    public float profitValue { get; private set; }

    [Header("Settings")]
    public float barDelay;

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

    public void SetValues(float enj, float comp, float ethic, float profit)
    {
        enjinValue = enj;
        competitiveValue = comp;
        ethicValue = ethic;
        profitValue = profit;
        StartCoroutine(UpdateSliderCoroutine());
    }

    public void ChangeValue(float enj, float comp, float ethic, float profit)
    {
        enjinValue += enj;
        competitiveValue += comp;
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
        RectTransform rect = sliderContainer.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-781, -445);
        rect.localScale = new Vector2(1, 1);
    }

    private IEnumerator UpdateSliderCoroutine()
    {
        enjinSlider.value = enjinValue;
        yield return new WaitForSeconds(barDelay);
        competitiveSlider.value = competitiveValue;
        yield return new WaitForSeconds(barDelay);
        ethicSlider.value = ethicValue;
        yield return new WaitForSeconds(barDelay);
        profitSlider.value = profitValue;
    }

    public void daboton()
    {
        SetValues(Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));
    }

}
