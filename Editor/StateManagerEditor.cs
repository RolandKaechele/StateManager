using UnityEditor;
using UnityEngine;
using StateManager.Runtime;

namespace StateManager.Editor
{
    /// <summary>
    /// Custom Inspector for <see cref="StateManager.Runtime.StateManager"/>.
    /// Displays the current state, previous state, full stack, and live transition controls.
    /// </summary>
    [CustomEditor(typeof(StateManager.Runtime.StateManager))]
    public class StateManagerEditor : UnityEditor.Editor
    {
        private string _customStateInput = string.Empty;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var sm = (StateManager.Runtime.StateManager)target;
            if (!Application.isPlaying) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("── Live State ──────────────────────────────", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.EnumPopup("Current State", sm.CurrentState);
            EditorGUILayout.EnumPopup("Previous State", sm.PreviousState);
            if (sm.CurrentState == AppState.Custom)
                EditorGUILayout.LabelField("Custom State Id", sm.CustomStateId);
            EditorGUILayout.IntField("Stack Depth", ((System.Collections.Generic.Stack<AppState>)(object)sm.StateStack)?.Count ?? 0);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("── Transitions ──────────────────────────────", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            foreach (AppState s in System.Enum.GetValues(typeof(AppState)))
            {
                if (s == AppState.Custom) continue;
                if (GUILayout.Button(s.ToString(), GUILayout.Height(20)))
                    sm.ChangeState(s);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Pop State")) sm.PopState();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("── Custom State ──────────────────────────────", EditorStyles.boldLabel);
            _customStateInput = EditorGUILayout.TextField("Custom State Id", _customStateInput);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("ChangeState")) sm.ChangeState(_customStateInput);
            if (GUILayout.Button("PushState"))   sm.PushState(_customStateInput);
            EditorGUILayout.EndHorizontal();

            Repaint();
        }
    }
}
