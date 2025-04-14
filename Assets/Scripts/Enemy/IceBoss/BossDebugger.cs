using System;
using UnityEditor;

namespace Enemy.IceBoss
{
    using UnityEngine;
    using System.Collections.Generic;

    public class BossDebugger : MonoBehaviour
    {
        public BossController controller;

        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        
        bool isInitialized = false;
        
        private void Start()
        {
            if (controller == null)
            {
                controller = FindFirstObjectByType<BossController>();
                if (controller == null)
                {
                    Debug.LogError("BossController not found in the scene.");
                    return;
                }
            }
        }

        private void Update()
        {
            // if (isInitialized || controller.RootSm == null) return;
            // var loggerObserver = gameObject.AddComponent<BossDebugObserver>();
            // var overlayObserver = gameObject.AddComponent<BossStateOverlay>();
            // controller.RootSm.AddObserver(loggerObserver);
            // controller.RootSm.AddObserver(overlayObserver);
            isInitialized = true;
        }

        void OnGUI()
        {
            if (!controller)
                return;
            
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label);
                _labelStyle.fontSize = 18; // ← Make this bigger!
                _labelStyle.normal.textColor = Color.white;
            }
            
            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(GUI.skin.label);
                _titleStyle.fontSize = 24; // ← Make this bigger!
                _titleStyle.fontStyle = FontStyle.Bold;
                _titleStyle.normal.textColor = Color.cyan;
            }


            var ctx = controller.Context;
            var sm = controller.RootSm;

            GUILayout.BeginArea(new Rect(10, 10, 300, 500), GUI.skin.box);
            GUILayout.Label($"Boss Debug Info", _titleStyle, GUILayout.Height(30));
            GUILayout.Label($"Health: {ctx.entityStatus.CurrentHealth} / {ctx.entityStatus.maxHealth}", _labelStyle);
            GUILayout.Label($"Phase: {ctx.phase}", _labelStyle);
            GUILayout.Label($"Wait: {ctx.waitTimer:0.00} / {ctx.attackWaitCooldown}", _labelStyle);
            GUILayout.Label($"Melee: {ctx.timeSinceLastMeleeAttack:0.00} / {ctx.meleeAttackCooldown}", _labelStyle);
            GUILayout.Label($"Ranged: {ctx.timeSinceLastThrow:0.00} / {ctx.throwCooldown}", _labelStyle);
            GUILayout.Label($"Ground: {ctx.timeSinceLastGroundAttack:0.00} / {ctx.groundAttackCooldown}", _labelStyle);
            GUILayout.Label($"Activated: {ctx.shouldActivate}", _labelStyle);
            GUILayout.Label($"Velocity: {ctx.movementController.gameObject.GetComponent<EntityMovementController>()?.Motor.Velocity}", _labelStyle);
            // EditorGUILayout.PropertyField(new SerializedObject(ctx).FindProperty("shouldActivate"), new GUIContent("Should Activate"));

            var stateBranch = sm.GetActiveHierarchyPath();
            // split by "/"
            var states = stateBranch.Split('/');
            
            if (states.Length > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label("Active State Stack:", _titleStyle);
                foreach (var s in states)
                {
                    GUILayout.Label($"→ {s}", _labelStyle);
                }
            }
            
            // if (stateBranch != null)
            // {
            //     GUILayout.Space(10);
            //     GUILayout.Label("Active State Stack:", _titleStyle);
            //     foreach (var s in stateBranch)
            //     {
            //         GUILayout.Label($"→ {s.GetType().Name}", _labelStyle);
            //     }
            // }

            GUILayout.EndArea();
        }
    }

}