#if STATEMANAGER_EM
using UnityEngine;
using EventManager.Runtime;

namespace StateManager.Runtime
{
    /// <summary>
    /// Optional bridge between StateManager and EventManager.
    /// Enable define <c>STATEMANAGER_EM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Fires the following named events when StateManager transitions:
    /// <list type="bullet">
    ///   <item><c>"state.changed"</c> — <see cref="GameEvent.stringValue"/> = new <see cref="AppState"/> name.</item>
    ///   <item><c>"state.pushed"</c>  — <see cref="GameEvent.stringValue"/> = pushed state name.</item>
    ///   <item><c>"state.popped"</c>  — <see cref="GameEvent.stringValue"/> = popped state name.</item>
    /// </list>
    /// </para>
    /// <para>Without the scripting symbol this component compiles as a no-op stub.</para>
    /// </summary>
    [AddComponentMenu("StateManager/Event Manager Bridge")]
    [DisallowMultipleComponent]
    public class EventManagerBridge : MonoBehaviour
    {
        [Tooltip("Event name fired on any state change.")]
        [SerializeField] private string stateChangedEvent = "state.changed";

        [Tooltip("Event name fired when a state is pushed.")]
        [SerializeField] private string statePushedEvent = "state.pushed";

        [Tooltip("Event name fired when a state is popped.")]
        [SerializeField] private string statePoppedEvent = "state.popped";

        private StateManager _state;
        private EventManager.Runtime.EventManager _events;

        private void Awake()
        {
            _state  = GetComponent<StateManager>() ?? FindFirstObjectByType<StateManager>();
            _events = GetComponent<EventManager.Runtime.EventManager>()
                      ?? FindFirstObjectByType<EventManager.Runtime.EventManager>();

            if (_state  == null) Debug.LogWarning("[StateManager.EventManagerBridge] StateManager not found.");
            if (_events == null) Debug.LogWarning("[StateManager.EventManagerBridge] EventManager not found.");
        }

        private void OnEnable()
        {
            if (_state != null)
            {
                _state.OnStateChanged += OnStateChanged;
                _state.OnStatePushed  += OnStatePushed;
                _state.OnStatePopped  += OnStatePopped;
            }
        }

        private void OnDisable()
        {
            if (_state != null)
            {
                _state.OnStateChanged -= OnStateChanged;
                _state.OnStatePushed  -= OnStatePushed;
                _state.OnStatePopped  -= OnStatePopped;
            }
        }

        private void OnStateChanged(AppState previous, AppState next)
        {
            _events?.Fire(new GameEvent(stateChangedEvent) { stringValue = next.ToString() });
        }

        private void OnStatePushed(AppState pushed)
        {
            _events?.Fire(new GameEvent(statePushedEvent) { stringValue = pushed.ToString() });
        }

        private void OnStatePopped(AppState popped)
        {
            _events?.Fire(new GameEvent(statePoppedEvent) { stringValue = popped.ToString() });
        }
    }
}
#else
namespace StateManager.Runtime
{
    /// <summary>No-op stub — define <c>STATEMANAGER_EM</c> to activate this bridge.</summary>
    [UnityEngine.AddComponentMenu("StateManager/Event Manager Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class EventManagerBridge : UnityEngine.MonoBehaviour { }
}
#endif
