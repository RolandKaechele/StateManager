using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace StateManager.Runtime
{
    /// <summary>
    /// <b>StateManager</b> is an application-level pushdown-automaton state machine.
    ///
    /// <para><b>Responsibilities:</b>
    /// <list type="number">
    ///   <item>Track the current <see cref="AppState"/> and a history stack of previous states.</item>
    ///   <item>Support modal state pushes (Dialogue, Cutscene, MiniGame) that overlay the base state.</item>
    ///   <item>Provide <see cref="ChangeState"/>, <see cref="PushState"/>, and <see cref="PopState"/>.</item>
    ///   <item>Support custom string-keyed states defined via <c>states.json</c> in <c>StreamingAssets/</c>.</item>
    /// </list>
    /// </para>
    ///
    /// <para><b>Modding / JSON:</b> Enable <c>loadFromJson</c> and place a <c>states.json</c> in
    /// <c>StreamingAssets/</c>. JSON entries are <b>merged by id</b>.</para>
    ///
    /// <para><b>Optional integration defines:</b>
    /// <list type="bullet">
    ///   <item><c>STATEMANAGER_GM</c>  — GameManager: maps <c>GameState</c> changes to <see cref="AppState"/>.</item>
    ///   <item><c>STATEMANAGER_CSM</c> — CutsceneManager: auto-push <c>Cutscene</c> and pop on complete/skip.</item>
    ///   <item><c>STATEMANAGER_DM</c>  — DialogueManager: auto-push <c>Dialogue</c> and pop on complete.</item>
    ///   <item><c>STATEMANAGER_MGM</c> — MiniGameManager: auto-push <c>MiniGame</c> and pop on complete/abort.</item>
    ///   <item><c>STATEMANAGER_MLF</c> — MapLoaderFramework: auto-push <c>Loading</c> and pop on loaded.</item>
    ///   <item><c>STATEMANAGER_EM</c>  — EventManager: broadcast state changes as named GameEvents.</item>
    ///   <item><c>STATEMANAGER_SM</c>  — SaveManager: persist the current custom state id in save data.</item>
    /// </list>
    /// </para>
    /// </summary>
    [AddComponentMenu("StateManager/State Manager")]
    [DisallowMultipleComponent]
#if ODIN_INSPECTOR
    public class StateManager : SerializedMonoBehaviour
#else
    public class StateManager : MonoBehaviour
#endif
    {
        // ─── Inspector ───────────────────────────────────────────────────────────

        [Header("State")]
        [Tooltip("State set on Awake before any bridge wires up.")]
        [SerializeField] private AppState initialState = AppState.Boot;

        [Header("Custom States / Modding")]
        [Tooltip("Custom states defined in the Inspector (merged with JSON if enabled).")]
        [SerializeField] private CustomStateDefinition[] customStates = Array.Empty<CustomStateDefinition>();

        [Tooltip("If true, merge custom state definitions from a JSON file in StreamingAssets/.")]
        [SerializeField] private bool loadFromJson = false;

        [Tooltip("Path relative to StreamingAssets/ (e.g. 'states.json').")]
        [SerializeField] private string jsonPath = "states.json";

        [Header("Stack")]
        [Tooltip("Maximum depth of the state stack. Older entries are discarded.")]
        [SerializeField] private int maxStackDepth = 16;

        [Tooltip("Log every state transition to the Unity Console.")]
        [SerializeField] private bool verboseLogging = false;

        // ─── Events ──────────────────────────────────────────────────────────────

        /// <summary>Fired when the current state changes. Parameters: previous state, new state.</summary>
        public event Action<AppState, AppState> OnStateChanged;

        /// <summary>Fired when a state is pushed onto the stack. Parameter: pushed state.</summary>
        public event Action<AppState> OnStatePushed;

        /// <summary>Fired when a state is popped from the stack. Parameter: state that was popped.</summary>
        public event Action<AppState> OnStatePopped;

        /// <summary>Fired when the active custom state id changes. Parameter: custom state id (or empty).</summary>
        public event Action<string> OnCustomStateChanged;

        // ─── State ───────────────────────────────────────────────────────────────

        private readonly Stack<AppState> _stack = new Stack<AppState>();
        private AppState _previousState = AppState.Boot;
        private string   _customStateId = string.Empty;

        private readonly Dictionary<string, CustomStateDefinition> _customDefs =
            new Dictionary<string, CustomStateDefinition>(StringComparer.OrdinalIgnoreCase);

        // ─── Properties ──────────────────────────────────────────────────────────

        /// <summary>The topmost (current) state on the stack.</summary>
        public AppState CurrentState => _stack.Count > 0 ? _stack.Peek() : initialState;

        /// <summary>The state that was active before the current one.</summary>
        public AppState PreviousState => _previousState;

        /// <summary>
        /// When <see cref="CurrentState"/> is <see cref="AppState.Custom"/>,
        /// this is the id of the active custom state definition.
        /// </summary>
        public string CustomStateId => _customStateId;

        /// <summary>Read-only view of the full state stack (top = current).</summary>
        public IReadOnlyCollection<AppState> StateStack => _stack;

        // ─── Unity lifecycle ─────────────────────────────────────────────────────

        private void Awake()
        {
            if (loadFromJson) LoadJsonDefinitions();
            MergeInspectorDefinitions();
            _stack.Clear();
            _stack.Push(initialState);
        }

        // ─── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Clear the stack and set <paramref name="state"/> as the only active state.
        /// </summary>
        public void ChangeState(AppState state)
        {
            var previous = CurrentState;
            _stack.Clear();
            _customStateId = string.Empty;
            _stack.Push(state);
            _previousState = previous;

            if (verboseLogging)
                Debug.Log($"[StateManager] ChangeState: {previous} → {state}");

            OnStateChanged?.Invoke(previous, state);
        }

        /// <summary>
        /// Clear the stack and set a <see cref="AppState.Custom"/> state identified by
        /// <paramref name="customId"/>. The id must be registered in the Inspector or JSON.
        /// </summary>
        public void ChangeState(string customId)
        {
            if (!_customDefs.ContainsKey(customId))
            {
                Debug.LogWarning($"[StateManager] Unknown custom state id: '{customId}'");
                return;
            }

            var previous = CurrentState;
            _stack.Clear();
            _customStateId = customId;
            _stack.Push(AppState.Custom);
            _previousState = previous;

            if (verboseLogging)
                Debug.Log($"[StateManager] ChangeState: {previous} → Custom({customId})");

            OnStateChanged?.Invoke(previous, AppState.Custom);
            OnCustomStateChanged?.Invoke(customId);
        }

        /// <summary>
        /// Push a modal <paramref name="state"/> on top of the current state without clearing it.
        /// </summary>
        public void PushState(AppState state)
        {
            if (_stack.Count >= maxStackDepth)
            {
                Debug.LogWarning($"[StateManager] Stack depth limit ({maxStackDepth}) reached. PushState ignored.");
                return;
            }

            var previous = CurrentState;
            _stack.Push(state);
            _previousState = previous;

            if (verboseLogging)
                Debug.Log($"[StateManager] PushState: {state} (prev: {previous})");

            OnStatePushed?.Invoke(state);
            OnStateChanged?.Invoke(previous, state);
        }

        /// <summary>
        /// Push a modal <see cref="AppState.Custom"/> state identified by <paramref name="customId"/>.
        /// </summary>
        public void PushState(string customId)
        {
            if (!_customDefs.ContainsKey(customId))
            {
                Debug.LogWarning($"[StateManager] Unknown custom state id: '{customId}'");
                return;
            }

            var previous = CurrentState;
            _customStateId = customId;
            _stack.Push(AppState.Custom);
            _previousState = previous;

            if (verboseLogging)
                Debug.Log($"[StateManager] PushState: Custom({customId}) (prev: {previous})");

            OnStatePushed?.Invoke(AppState.Custom);
            OnStateChanged?.Invoke(previous, AppState.Custom);
            OnCustomStateChanged?.Invoke(customId);
        }

        /// <summary>
        /// Pop the topmost state from the stack, restoring the state below it.
        /// Has no effect when only one state remains.
        /// </summary>
        public void PopState()
        {
            if (_stack.Count <= 1)
            {
                Debug.LogWarning("[StateManager] Cannot pop — only one state on the stack.");
                return;
            }

            var popped    = _stack.Pop();
            var newCurrent = CurrentState;
            _previousState = popped;

            if (popped == AppState.Custom)
                _customStateId = string.Empty;

            if (verboseLogging)
                Debug.Log($"[StateManager] PopState: {popped} → {newCurrent}");

            OnStatePopped?.Invoke(popped);
            OnStateChanged?.Invoke(popped, newCurrent);
        }

        /// <summary>Returns true when <paramref name="state"/> is anywhere in the stack.</summary>
        public bool HasState(AppState state) => _stack.Contains(state);

        /// <summary>Returns true when the state identified by <paramref name="customId"/> is current.</summary>
        public bool IsCustomState(string customId) =>
            CurrentState == AppState.Custom &&
            string.Equals(_customStateId, customId, StringComparison.OrdinalIgnoreCase);

        /// <summary>Returns the <see cref="CustomStateDefinition"/> for <paramref name="customId"/>, or null.</summary>
        public CustomStateDefinition GetCustomDefinition(string customId) =>
            _customDefs.TryGetValue(customId, out var def) ? def : null;

        /// <summary>All registered custom state definitions.</summary>
        public IReadOnlyDictionary<string, CustomStateDefinition> CustomDefinitions => _customDefs;

        // ─── Internal helpers ─────────────────────────────────────────────────────

        private void MergeInspectorDefinitions()
        {
            foreach (var def in customStates)
                if (!string.IsNullOrEmpty(def.id))
                    _customDefs[def.id] = def;
        }

        private void LoadJsonDefinitions()
        {
            var fullPath = Path.Combine(Application.streamingAssetsPath, jsonPath);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"[StateManager] states.json not found at: {fullPath}");
                return;
            }

            try
            {
                var manifest = JsonUtility.FromJson<CustomStateManifestJson>(File.ReadAllText(fullPath));
                if (manifest?.states == null) return;

                foreach (var def in manifest.states)
                    if (!string.IsNullOrEmpty(def.id))
                        _customDefs[def.id] = def;

                if (verboseLogging)
                    Debug.Log($"[StateManager] Loaded {manifest.states.Length} custom state(s) from JSON.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[StateManager] Failed to parse states.json: {ex.Message}");
            }
        }
    }
}
