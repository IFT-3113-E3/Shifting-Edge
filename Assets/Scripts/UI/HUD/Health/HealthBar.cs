using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image healthBar;
    public float maxHealth = 100f;
    private float currentHealth;
    public Gradient healthGradient;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        float healthRatio = currentHealth / maxHealth;
        healthBar.fillAmount = healthRatio;

        healthBar.color = healthGradient.Evaluate(healthRatio);
    }
}