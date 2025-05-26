using System;
using UnityEngine;

public class HealthComponent {
    private int maxHealth;
    private int currentHealth;

    public HealthComponent(int maxHealth) {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
    }

    public HealthComponent(int maxHealth, int currentHealth) {
        this.maxHealth = maxHealth;
        this.currentHealth = currentHealth;
    }

    public void UpdateHealth(int value) {
        currentHealth = Math.Clamp(currentHealth + value, 0, maxHealth);
    }

    public void UpdateMaxHealth(int value, bool heal=false) {
        var healthPercent = GetCurrentHealthPercentage();
        maxHealth = Math.Clamp(maxHealth + value, 0, maxHealth);
        currentHealth = maxHealth * healthPercent;
    }

    public int GetCurrentHealth() {
        return currentHealth;
    }

    public int GetMaxHealth() {
        return maxHealth;
    }

    public int GetCurrentHealthPercentage() {
        return (int)Mathf.Round((float)currentHealth / (float)maxHealth * 100f);
    }
}
