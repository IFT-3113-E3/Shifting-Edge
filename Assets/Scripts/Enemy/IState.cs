using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Interface de base pour tous les états
public interface IState
{
    void Enter();
    void Update();
    void Exit();
}

// Classe de base pour la machine à états
public class StateMachine
{
    private IState currentState;

    public void Initialize(IState startingState)
    {
        currentState = startingState;
        currentState.Enter();
    }

    public void ChangeState(IState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void UpdateState()
    {
        if (currentState != null)
            currentState.Update();
    }

    // getter pour l'état courant
    public IState GetCurrentState()
    {
        return currentState;
    }
}