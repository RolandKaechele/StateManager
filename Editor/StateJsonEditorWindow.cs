#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using StateManager.Runtime;
using UnityEditor;
using UnityEngine;

namespace StateManager.Editor
{
    // ────────────────────────────────────────────────────────────────────────────
    // App State JSON Editor Window
    // ────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Editor window for creating and editing <c>states.json</c> in StreamingAssets.
    /// Open via <b>JSON Editors → State Manager</b> or via the Manager Inspector button.
    /// </summary>
    public class StateJsonEditorWindow : EditorWindow
    {
        private const string JsonFileName = "states.json";

        private StateEditorBridge        _bridge;
        private UnityEditor.Editor       _bridgeEditor;
        private Vector2                  _scroll;
        private string                   _status;
        private bool                     _statusError;

        [MenuItem("JSON Editors/State Manager")]
        public static void ShowWindow() =>
            GetWindow<StateJsonEditorWindow>("App States JSON");

        private void OnEnable()
        {
            _bridge = CreateInstance<StateEditorBridge>();
            Load();
        }

        private void OnDisable()
        {
            if (_bridgeEditor != null) DestroyImmediate(_bridgeEditor);
            if (_bridge      != null) DestroyImmediate(_bridge);
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (!string.IsNullOrEmpty(_status))
                EditorGUILayout.HelpBox(_status, _statusError ? MessageType.Error : MessageType.Info);

            if (_bridge == null) return;
            if (_bridgeEditor == null)
                _bridgeEditor = UnityEditor.Editor.CreateEditor(_bridge);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            _bridgeEditor.OnInspectorGUI();
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField(
                Path.Combine("StreamingAssets", JsonFileName),
                EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(50))) Load();
            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50))) Save();
            EditorGUILayout.EndHorizontal();
        }

        private void Load()
        {
            var path = Path.Combine(Application.streamingAssetsPath, JsonFileName);
            try
            {
                if (!File.Exists(path))
                {
                    File.WriteAllText(path, JsonUtility.ToJson(new StateEditorWrapper(), true));
                    AssetDatabase.Refresh();
                }

                var w = JsonUtility.FromJson<StateEditorWrapper>(File.ReadAllText(path));
                _bridge.states = new List<CustomStateDefinition>(
                    w.states ?? Array.Empty<CustomStateDefinition>());

                if (_bridgeEditor != null) { DestroyImmediate(_bridgeEditor); _bridgeEditor = null; }

                _status     = $"Loaded {_bridge.states.Count} state definitions.";
                _statusError = false;
            }
            catch (Exception e)
            {
                _status     = $"Load error: {e.Message}";
                _statusError = true;
            }
        }

        private void Save()
        {
            try
            {
                var w    = new StateEditorWrapper { states = _bridge.states.ToArray() };
                var path = Path.Combine(Application.streamingAssetsPath, JsonFileName);
                File.WriteAllText(path, JsonUtility.ToJson(w, true));
                AssetDatabase.Refresh();
                _status     = $"Saved {_bridge.states.Count} states to {JsonFileName}.";
                _statusError = false;
            }
            catch (Exception e)
            {
                _status     = $"Save error: {e.Message}";
                _statusError = true;
            }
        }
    }

    // ── ScriptableObject bridge ──────────────────────────────────────────────
    internal class StateEditorBridge : ScriptableObject
    {
        public List<CustomStateDefinition> states = new List<CustomStateDefinition>();
    }

    // ── Local wrapper mirrors the internal CustomStateManifestJson ───────────
    [Serializable]
    internal class StateEditorWrapper
    {
        public CustomStateDefinition[] states = Array.Empty<CustomStateDefinition>();
    }
}
#endif
