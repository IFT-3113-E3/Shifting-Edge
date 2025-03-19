using UnityEngine;

public class TestDamage : MonoBehaviour
{
    public HealthBar healthBar;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            healthBar.TakeDamage(10f);
        }
    }
}