using System.Collections.Generic;
using UnityEngine;

namespace Enemy.IceBoss
{
    public class BossDebugObserver : MonoBehaviour, IStateObserver<BossContext>
    {
        public void OnStateEnter(State<BossContext> state)
        {
            Debug.Log($"[Enter] {Format(state)}");
        }

        public void OnStateExit(State<BossContext> state)
        {
            Debug.Log($"[Exit]  {Format(state)}");
        }

        public void OnStateUpdate(State<BossContext> state)
        {
            // Optional: useful for visualizers or logging performance issues
        }

        private string Format(State<BossContext> state)
        {
            var path = string.Join(" -> ", state.StateMachine.GetActiveStateBranch()
                .ConvertAll(s => s.GetType().Name));
            return $"State: {path}";
        }
    }
    
    public class BossStateOverlay : MonoBehaviour, IStateObserver<BossContext>
    {
        private List<string> _activeStates = new();
        private BossContext _ctx;

        public void OnStateEnter(State<BossContext> state)
        {
            _ctx = state.ContextDbg;
            UpdateStack(state);
        }

        public void OnStateExit(State<BossContext> state)
        {
            UpdateStack(state);
        }

        public void OnStateUpdate(State<BossContext> state)
        {
            UpdateStack(state);
        }

        void UpdateStack(State<BossContext> state)
        {
            _activeStates.Clear();
            foreach (var s in state.StateMachine.GetActiveStateBranch())
                _activeStates.Add(s.GetType().Name);
        }

        void OnGUI()
        {
            if (_ctx == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200), GUI.skin.box);
            GUILayout.Label("BOSS STATE STACK:");
            foreach (var state in _activeStates)
                GUILayout.Label("→ " + state);
            GUILayout.EndArea();
        }
    }

}