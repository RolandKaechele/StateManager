#if STATEMANAGER_DM
using UnityEngine;
using DialogueManager.Runtime;

namespace StateManager.Runtime
{
    /// <summary>
    /// Optional bridge between StateManager and DialogueManager.
    /// Enable define <c>STATEMANAGER_DM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// <list type="bullet">
    ///   <item>Pushes <see cref="AppState.Dialogue"/> when a dialogue starts.</item>
    ///   <item>Pops the state when the dialogue completes or is cancelled.</item>
    /// </list>
    /// </para>
    /// <para>Without the scripting symbol this component compiles as a no-op stub.</para>
    /// </summary>
    [AddComponentMenu("StateManager/Dialogue Manager Bridge")]
    [DisallowMultipleComponent]
    public class DialogueManagerBridge : MonoBehaviour
    {
        private StateManager _state;
        private DialogueManager.Runtime.DialogueManager _dialogue;

        private void Awake()
        {
            _state    = GetComponent<StateManager>() ?? FindFirstObjectByType<StateManager>();
            _dialogue = GetComponent<DialogueManager.Runtime.DialogueManager>()
                        ?? FindFirstObjectByType<DialogueManager.Runtime.DialogueManager>();

            if (_state    == null) Debug.LogWarning("[StateManager.DialogueManagerBridge] StateManager not found.");
            if (_dialogue == null) Debug.LogWarning("[StateManager.DialogueManagerBridge] DialogueManager not found.");
        }

        private void OnEnable()
        {
            if (_dialogue != null)
            {
                _dialogue.OnDialogueStarted   += OnStarted;
                _dialogue.OnDialogueCompleted += OnEnded;
            }
        }

        private void OnDisable()
        {
            if (_dialogue != null)
            {
                _dialogue.OnDialogueStarted   -= OnStarted;
                _dialogue.OnDialogueCompleted -= OnEnded;
            }
        }

        private void OnStarted(string _) => _state?.PushState(AppState.Dialogue);
        private void OnEnded(string _)   => _state?.PopState();
    }
}
#else
namespace StateManager.Runtime
{
    /// <summary>No-op stub — define <c>STATEMANAGER_DM</c> to activate this bridge.</summary>
    [UnityEngine.AddComponentMenu("StateManager/Dialogue Manager Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class DialogueManagerBridge : UnityEngine.MonoBehaviour { }
}
#endif
