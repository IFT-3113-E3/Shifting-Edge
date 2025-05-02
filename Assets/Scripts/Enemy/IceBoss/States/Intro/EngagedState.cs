using System.Collections;
using UnityEngine;
using UnityHFSM;

namespace Enemy.IceBoss.States.Intro
{
    // The EngagedState is the state that the boss is in when it is done spawning and is getting ready to fight
    public class EngagedState : StateBase
    {
        private BossContext _ctx;
        private MonoBehaviour _mono;
        
        public EngagedState(MonoBehaviour mono, BossContext ctx) : base(true)
        {
            _ctx = ctx;
            _mono = mono;
        }

        public override void OnEnter()
        {
            // Logic for entering the engaged state
            // For example, play an animation or spawn some enemies
            Debug.Log("EngagedState: Enter");
            GameManager.Instance.GameSession.AddBossFightState(_ctx.bossFightState);
            _mono.StartCoroutine(InteractionSequence());
        }

        public override void OnLogic()
        {
            // This state doesn't do anything in the update, it just waits for the child state to finish
        }
        
        private IEnumerator InteractionSequence()
        {
            // Logic for the interaction sequence
            // _ctx.speechBubbleSpawner.Speak(
            //     "I am the Ice Boss! Prepare to meet your doom!",
            //     2f);
            
            // For example, play an animation or spawn some enemies
            yield return new WaitForSeconds(2f);
            // After the interaction sequence is done, transition to the next state
            fsm.StateCanExit();
        }
    }
}