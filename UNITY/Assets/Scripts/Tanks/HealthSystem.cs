using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    #region Fields
    [SerializeField] private float _maxHealth = 100f;

    private float _currentHealth;
    #endregion

    #region Properties
    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public bool IsDead => _currentHealth <= 0;

    public float HealthPercentage => _currentHealth / _maxHealth;
    #endregion

    #region Unity Events
    public event Action OnHealthChanged;
    public event Action OnDeath;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // Apply the player-configured spec (default matches the prefab, so unchanged unless edited).
        _maxHealth = MatchSettings.TankMaxHealth;
        _currentHealth = _maxHealth;
    }
    #endregion

    #region Health Management Methods
    /// <summary>Sets the maximum health and clamps current health into the new range.</summary>
    public void SetMaxHealth(float value)
    {
        _maxHealth = Mathf.Max(1f, value);
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        OnHealthChanged?.Invoke();
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        OnHealthChanged?.Invoke();
        if (IsDead)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        _currentHealth += amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        OnHealthChanged?.Invoke();
    }

    public void RestoreHealth()
    {
        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke();
    }
    #endregion
}
