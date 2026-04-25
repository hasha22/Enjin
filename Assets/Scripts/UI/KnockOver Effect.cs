using System.Collections;
using UnityEngine;

public class KnockOverEffect : MonoBehaviour
{
    public float duration = 0.8f;

    public void Fall()
    {
        StartCoroutine(FallRoutine());
    }

    IEnumerator FallRoutine()
    {
        float time = 0f;

        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(90f, 0f, 0f); // tilt away

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * 2f; // slight drop

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            // Ease-in (starts slow, speeds up = gravity feel)
            t = t * t;

            transform.rotation = Quaternion.Lerp(startRot, endRot, t);
            transform.position = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        gameObject.SetActive(false); // fully gone at end
    }
}
