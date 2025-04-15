using System;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// A state part of a hierarchical state machine.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public abstract class State<TContext>
    {
        protected readonly TContext ctx;
        protected readonly StateMachine<TContext> sm;
        protected StateMachine<TContext> parent;

        protected internal TContext ContextDbg => ctx;

        public State<TContext> ActiveState => sm.CurrentState;
        public StateMachine<TContext> StateMachine => sm;

        public bool IsCompleted { get; private set; }

        protected State(TContext context)
        {
            ctx = context;
            sm = new StateMachine<TContext>();
        }

        public void Init(StateMachine<TContext> parent)
        {
            this.parent = parent;
            IsCompleted = false;

            // Initialize the observers
            foreach (var observer in parent.GetObservers())
                sm.AddObserver(observer);
        }

        protected void Set(State<TContext> state) =>
            sm.ChangeState(state);

        public virtual void Enter()
        {
        }

        protected virtual void Update()
        {
        }

        protected virtual void FixedUpdate()
        {
        }

        public void Finish()
        {
            IsCompleted = true;
            Debug.Log("Finish: " + GetType().Name);
        }

        public void UpdateBranch()
        {
            if (IsCompleted) return;
            Update();
            sm.Update();
        }

        public void FixedUpdateBranch()
        {
            if (IsCompleted) return;
            FixedUpdate();
            sm.FixedUpdate();
        }

        public virtual void Exit()
        {
            // Logic for exiting the state
        }
    }

    /// <summary>
    /// A state machine.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class StateMachine<TContext>
    {
        private State<TContext> _currentState;
        private readonly List<IStateObserver<TContext>> _observers = new();

        // event for when a state is completed
        public event Action<State<TContext>> OnComplete;

        public State<TContext> CurrentState => _currentState;

        public void AddObserver(IStateObserver<TContext> observer) =>
            _observers.Add(observer);

        public void RemoveObserver(IStateObserver<TContext> observer) =>
            _observers.Remove(observer);

        public List<IStateObserver<TContext>> GetObservers() => _observers;

        public void ChangeState(State<TContext> newState)
        {
            if (newState == null)
            {
                Debug.Log("StateMachine: New state is null, not changing.");
                return;
            }

            Debug.Log("Entering ChangeState for " + newState.GetType().Name +
                    " with current state " + _currentState?.GetType().Name + "\n" +
                    "_currentState == newState: " + (_currentState == newState) + "\n");
            if (_currentState != null && _currentState == newState && !_currentState.IsCompleted)
            {
                Debug.Log("StateMachine: New state is the same as current state, not changing.");
                return;
            }

            if (_currentState != null)
            {
                foreach (var observer in _observers)
                    observer.OnStateExit(_currentState);

                _currentState.Exit();
            }

            Debug.Log($"StateMachine: Changing state from {_currentState?.GetType().Name} to {newState?.GetType().Name}");

            _currentState = newState;
            if (_currentState == null) return;
            _currentState.Init(this);

            foreach (var observer in _observers)
                observer.OnStateEnter(_currentState);
            _currentState.Enter();
        }

        public void Update()
        {
            foreach (var observer in _observers)
                observer.OnStateUpdate(_currentState);

            if (_currentState == null) return;
            _currentState.UpdateBranch();

            if (_currentState.IsCompleted)
            {
                OnComplete?.Invoke(_currentState);
            }
        }

        public void FixedUpdate()
        {
            _currentState?.FixedUpdateBranch();
        }

        public List<State<TContext>> GetActiveStateBranch(List<State<TContext>> states = null)
        {
            states ??= new List<State<TContext>>();
            if (_currentState == null) return states;
            states.Add(_currentState);
            return _currentState.StateMachine.GetActiveStateBranch(states);
        }
    }
}
