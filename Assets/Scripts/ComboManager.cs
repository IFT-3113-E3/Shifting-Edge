using UnityEngine;
using System;

public class ComboManager : MonoBehaviour
{
    public event Action<int> OnComboStep;
    public event Action<bool, string> OnComboEnd;

    [Tooltip("Time (in seconds) that the next input can be accepted for the combo.")]
    public float comboWindowDuration = 2f;
    public int maxCombo = 3;

    private bool _canQueueNext = false;
    private bool _attackQueued = false;
    private bool _isAttacking = false;
    private float _comboWindowEndTime = 0f;
    private int _currentCombo = 0;
    private bool _waitingOnWindow = false;
    private float _cooldownTime = 0f;
    
    public int CurrentCombo => _currentCombo;
    public bool IsAttacking => _isAttacking;
    
    void Update()
    {
        if (Time.time < _cooldownTime)
            return;
        
        if (_currentCombo > 0 && !_isAttacking && _waitingOnWindow && Time.time > _comboWindowEndTime)
        {
            _waitingOnWindow = false;
            if (!_attackQueued)
            {
                ResetCombo("Combo window ended");
            }
        }
    }

    public void TryAttack()
    {
        if (Time.time < _cooldownTime)
        {
            Debug.Log($"Attack on cooldown: {Time.time - _cooldownTime}");
            return;
        }
        
        if (_isAttacking && _canQueueNext)
        {
            _attackQueued = true;
        }
        else if (!_isAttacking && !_attackQueued && (_waitingOnWindow || _currentCombo == 0))
        {
            if (_currentCombo >= maxCombo)
            {
                EndOfCombo();
                return;
            }
            
            _currentCombo++;
            _isAttacking = true;
            _canQueueNext = true;
            _attackQueued = false;
            _waitingOnWindow = false;
            OnComboStep?.Invoke(_currentCombo);
        }
    }

    public void OnAttackEnd()
    {
        if (_attackQueued)
        {
            if (_currentCombo >= maxCombo)
            {
                EndOfCombo();
                return;
            }
            
            _currentCombo++;
            OnComboStep?.Invoke(_currentCombo);
            _attackQueued = false;
            _isAttacking = true;
            _canQueueNext = true;
        }
        else
        {
            _comboWindowEndTime = Time.time + comboWindowDuration;
            _isAttacking = false;
            _waitingOnWindow = true;
        }
    }

    public void ResetCombo(string debug = "")
    {
        if (_currentCombo > 0)
        {
            _cooldownTime = Time.time + 0.5f;
        }

        _currentCombo = 0;
        _isAttacking = false;
        _attackQueued = false;
        _canQueueNext = false;
        _waitingOnWindow = false;

        OnComboEnd?.Invoke(true, debug);
    }
    
    public void EndOfCombo()
    {
        _cooldownTime = Time.time + 1.5f;
        
        _currentCombo = 0;
        _isAttacking = false;
        _attackQueued = false;
        _canQueueNext = false;
        _waitingOnWindow = false;
        
        OnComboEnd?.Invoke(false, "Combo finished");
    }
}