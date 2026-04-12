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
        private const string JsonFolderName   = "states";
        private const string JsonSaveFileName = "states.json";

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
                $"StreamingAssets/{JsonFolderName}/",
                EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(50))) Load();
            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(50))) Save();
            EditorGUILayout.EndHorizontal();
        }

        private void Load()
        {
            string folderPath = Path.Combine(Application.streamingAssetsPath, JsonFolderName);
            try
            {
                var list = new List<CustomStateDefinition>();
                if (Directory.Exists(folderPath))
                {
                    foreach (var file in Directory.GetFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly))
                    {
                        var w = JsonUtility.FromJson<StateEditorWrapper>(File.ReadAllText(file));
                        if (w?.states != null) list.AddRange(w.states);
                    }
                }
                else
                {
                    Directory.CreateDirectory(folderPath);
                    File.WriteAllText(Path.Combine(folderPath, JsonSaveFileName), JsonUtility.ToJson(new StateEditorWrapper(), true));
                    AssetDatabase.Refresh();
                }
                _bridge.states = list;
                if (_bridgeEditor != null) { DestroyImmediate(_bridgeEditor); _bridgeEditor = null; }
                _status = $"Loaded {list.Count} states from {JsonFolderName}/.";
                _statusError = false;
            }
            catch (Exception e) { _status = $"Load error: {e.Message}"; _statusError = true; }
        }

        private void Save()
        {
            try
            {
                string folderPath = Path.Combine(Application.streamingAssetsPath, JsonFolderName);
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                var w = new StateEditorWrapper { states = _bridge.states.ToArray() };
                var path = Path.Combine(folderPath, JsonSaveFileName);
                File.WriteAllText(path, JsonUtility.ToJson(w, true));
                AssetDatabase.Refresh();
                _status = $"Saved {_bridge.states.Count} states to {JsonFolderName}/{JsonSaveFileName}.";
                _statusError = false;
            }
            catch (Exception e) { _status = $"Save error: {e.Message}"; _statusError = true; }
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
