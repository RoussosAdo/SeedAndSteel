using UnityEngine;

public class HitEffectAutoDestroy : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.35f;

    private void OnEnable()
    {
        Destroy(gameObject, lifetime);
    }
}