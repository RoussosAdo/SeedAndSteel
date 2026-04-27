using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        healthText.text = $"HP: {currentHealth} / {maxHealth}";
    }
}