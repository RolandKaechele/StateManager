#if STATEMANAGER_MGM
using UnityEngine;
using MiniGameManager.Runtime;

namespace StateManager.Runtime
{
    /// <summary>
    /// Optional bridge between StateManager and MiniGameManager.
    /// Enable define <c>STATEMANAGER_MGM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// <list type="bullet">
    ///   <item>Pushes <see cref="AppState.MiniGame"/> when a mini-game starts.</item>
    ///   <item>Pops the state when the mini-game completes or is aborted.</item>
    /// </list>
    /// </para>
    /// <para>Without the scripting symbol this component compiles as a no-op stub.</para>
    /// </summary>
    [AddComponentMenu("StateManager/Mini Game Manager Bridge")]
    [DisallowMultipleComponent]
    public class MiniGameManagerBridge : MonoBehaviour
    {
        private StateManager _state;
        private MiniGameManager.Runtime.MiniGameManager _mgm;

        private void Awake()
        {
            _state = GetComponent<StateManager>() ?? FindFirstObjectByType<StateManager>();
            _mgm   = GetComponent<MiniGameManager.Runtime.MiniGameManager>()
                     ?? FindFirstObjectByType<MiniGameManager.Runtime.MiniGameManager>();

            if (_state == null) Debug.LogWarning("[StateManager.MiniGameManagerBridge] StateManager not found.");
            if (_mgm   == null) Debug.LogWarning("[StateManager.MiniGameManagerBridge] MiniGameManager not found.");
        }

        private void OnEnable()
        {
            if (_mgm != null)
            {
                _mgm.OnMiniGameStarted   += OnStarted;
                _mgm.OnMiniGameCompleted += OnCompleted;
                _mgm.OnMiniGameAborted   += OnAborted;
            }
        }

        private void OnDisable()
        {
            if (_mgm != null)
            {
                _mgm.OnMiniGameStarted   -= OnStarted;
                _mgm.OnMiniGameCompleted -= OnCompleted;
                _mgm.OnMiniGameAborted   -= OnAborted;
            }
        }

        private void OnStarted(string _)           => _state?.PushState(AppState.MiniGame);
        private void OnCompleted(string _, object _2) => _state?.PopState();
        private void OnAborted(string _)            => _state?.PopState();
    }
}
#else
namespace StateManager.Runtime
{
    /// <summary>No-op stub — define <c>STATEMANAGER_MGM</c> to activate this bridge.</summary>
    [UnityEngine.AddComponentMenu("StateManager/Mini Game Manager Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class MiniGameManagerBridge : UnityEngine.MonoBehaviour { }
}
#endif
