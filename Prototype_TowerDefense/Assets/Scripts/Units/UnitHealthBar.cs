using UnityEngine;
using UnityEngine.UI;

public class UnitHealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public Health targetHealth;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(targetHealth.currentHealth, targetHealth.maxHealth);
        }
    }

    private void OnDestroy()
    {
        if (targetHealth != null)
            targetHealth.OnHealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(int currentHP, int maxHP)
    {
        healthSlider.maxValue = maxHP;
        healthSlider.value = currentHP;
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }
}