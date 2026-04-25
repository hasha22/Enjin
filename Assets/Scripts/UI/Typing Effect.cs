using System.Collections;
using TMPro;
using UnityEngine;

public class TypingEffect : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    [TextArea] public string fullText;
    public float typingSpeed = 0.05f;
    public float delay = 0f;

    public GameObject image;

    void Start()
    {
        StartCoroutine(TypeText());
        if (image != null)
            image.SetActive(true);
    }

    IEnumerator TypeText()
    {
        yield return new WaitForSeconds(delay);
        textComponent.text = "";

        foreach (char c in fullText)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
