#if STATEMANAGER_SM
using UnityEngine;
using SaveManager.Runtime;

namespace StateManager.Runtime
{
    /// <summary>
    /// Optional bridge between StateManager and SaveManager.
    /// Enable define <c>STATEMANAGER_SM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// <list type="bullet">
    ///   <item>Writes the current <see cref="AppState"/> name (and custom state id when applicable)
    ///   as a custom data entry in SaveManager on every state change.</item>
    ///   <item>Restores the last persisted state on load if <see cref="restoreOnLoad"/> is enabled.</item>
    /// </list>
    /// </para>
    /// <para>Without the scripting symbol this component compiles as a no-op stub.</para>
    /// </summary>
    [AddComponentMenu("StateManager/Save Manager Bridge")]
    [DisallowMultipleComponent]
    public class SaveManagerBridge : MonoBehaviour
    {
        [Tooltip("Key used to store state in SaveManager custom data.")]
        [SerializeField] private string stateKey = "state_manager_current";

        [Tooltip("If true, restore the last persisted state when a save slot is loaded.")]
        [SerializeField] private bool restoreOnLoad = false;

        private StateManager _state;
        private SaveManager.Runtime.SaveManager _save;

        private void Awake()
        {
            _state = GetComponent<StateManager>() ?? FindFirstObjectByType<StateManager>();
            _save  = GetComponent<SaveManager.Runtime.SaveManager>()
                     ?? FindFirstObjectByType<SaveManager.Runtime.SaveManager>();

            if (_state == null) Debug.LogWarning("[StateManager.SaveManagerBridge] StateManager not found.");
            if (_save  == null) Debug.LogWarning("[StateManager.SaveManagerBridge] SaveManager not found.");
        }

        private void OnEnable()
        {
            if (_state != null) _state.OnStateChanged += OnStateChanged;
            if (_save  != null && restoreOnLoad) _save.OnLoaded += OnSaveLoaded;
        }

        private void OnDisable()
        {
            if (_state != null) _state.OnStateChanged -= OnStateChanged;
            if (_save  != null) _save.OnLoaded -= OnSaveLoaded;
        }

        private void OnStateChanged(AppState previous, AppState next)
        {
            if (_save == null) return;
            var value = next == AppState.Custom ? $"Custom:{_state.CustomStateId}" : next.ToString();
            _save.SetCustomData(stateKey, value);
        }

        private void OnSaveLoaded(int slot)
        {
            if (_state == null || _save == null) return;
            var value = _save.GetCustomData(stateKey);
            if (string.IsNullOrEmpty(value)) return;

            if (value.StartsWith("Custom:"))
            {
                var customId = value.Substring(7);
                _state.ChangeState(customId);
            }
            else if (System.Enum.TryParse<AppState>(value, out var parsed))
            {
                _state.ChangeState(parsed);
            }
        }
    }
}
#else
namespace StateManager.Runtime
{
    /// <summary>No-op stub — define <c>STATEMANAGER_SM</c> to activate this bridge.</summary>
    [UnityEngine.AddComponentMenu("StateManager/Save Manager Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class SaveManagerBridge : UnityEngine.MonoBehaviour { }
}
#endif
