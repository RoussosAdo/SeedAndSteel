using System.Collections;
using UnityEngine;

public class CombatFeedback : MonoBehaviour
{
    public static CombatFeedback Instance { get; private set; }

    [Header("Hit Pause")]
    [SerializeField] private float hitPauseDuration = 0.06f;

    [Header("Camera Shake")]
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private float shakeDuration = 0.08f;
    [SerializeField] private float shakeStrength = 0.08f;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayHitFeedback()
    {
        StartCoroutine(HitPauseRoutine());

        if (cameraShake != null)
            cameraShake.Shake(shakeDuration, shakeStrength);
    }

    private IEnumerator HitPauseRoutine()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitPauseDuration);
        Time.timeScale = 1f;
    }
}