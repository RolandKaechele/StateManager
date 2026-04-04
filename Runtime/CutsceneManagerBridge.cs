#if STATEMANAGER_CSM
using UnityEngine;
using CutsceneManager.Runtime;

namespace StateManager.Runtime
{
    /// <summary>
    /// Optional bridge between StateManager and CutsceneManager.
    /// Enable define <c>STATEMANAGER_CSM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// <list type="bullet">
    ///   <item>Pushes <see cref="AppState.Cutscene"/> when a sequence starts.</item>
    ///   <item>Pops the state when the sequence completes or is skipped.</item>
    /// </list>
    /// </para>
    /// <para>Without the scripting symbol this component compiles as a no-op stub.</para>
    /// </summary>
    [AddComponentMenu("StateManager/Cutscene Manager Bridge")]
    [DisallowMultipleComponent]
    public class CutsceneManagerBridge : MonoBehaviour
    {
        private StateManager _state;
        private CutsceneManager.Runtime.CutsceneManager _cutscene;

        private void Awake()
        {
            _state    = GetComponent<StateManager>() ?? FindFirstObjectByType<StateManager>();
            _cutscene = GetComponent<CutsceneManager.Runtime.CutsceneManager>()
                        ?? FindFirstObjectByType<CutsceneManager.Runtime.CutsceneManager>();

            if (_state    == null) Debug.LogWarning("[StateManager.CutsceneManagerBridge] StateManager not found.");
            if (_cutscene == null) Debug.LogWarning("[StateManager.CutsceneManagerBridge] CutsceneManager not found.");
        }

        private void OnEnable()
        {
            if (_cutscene != null)
            {
                _cutscene.OnSequenceStarted   += OnStarted;
                _cutscene.OnSequenceCompleted += OnEnded;
                _cutscene.OnSequenceSkipped   += OnEnded;
            }
        }

        private void OnDisable()
        {
            if (_cutscene != null)
            {
                _cutscene.OnSequenceStarted   -= OnStarted;
                _cutscene.OnSequenceCompleted -= OnEnded;
                _cutscene.OnSequenceSkipped   -= OnEnded;
            }
        }

        private void OnStarted(string _) => _state?.PushState(AppState.Cutscene);
        private void OnEnded(string _)   => _state?.PopState();
    }
}
#else
namespace StateManager.Runtime
{
    /// <summary>No-op stub — define <c>STATEMANAGER_CSM</c> to activate this bridge.</summary>
    [UnityEngine.AddComponentMenu("StateManager/Cutscene Manager Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class CutsceneManagerBridge : UnityEngine.MonoBehaviour { }
}
#endif
