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
        private const string JsonFolderName = "states";

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
                int saved = 0;
                foreach (var entry in _bridge.states)
                {
                    if (string.IsNullOrEmpty(entry.id)) continue;
                    var w = new StateEditorWrapper { states = new[] { entry } };
                    File.WriteAllText(Path.Combine(folderPath, $"{entry.id}.json"), JsonUtility.ToJson(w, true));
                    saved++;
                }
                AssetDatabase.Refresh();
                _status = $"Saved {saved} state file(s) to {JsonFolderName}/";
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
