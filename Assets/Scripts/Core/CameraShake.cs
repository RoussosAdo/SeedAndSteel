using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Coroutine shakeRoutine;
    private Vector3 shakeOffset;

    public Vector3 ShakeOffset => shakeOffset;

    public void Shake(float duration, float strength)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, strength));
    }

    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        float timer = 0f;

        while (timer < duration)
        {
            Vector2 randomOffset = Random.insideUnitCircle * strength;
            shakeOffset = new Vector3(randomOffset.x, randomOffset.y, 0f);

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
        shakeRoutine = null;
    }
}