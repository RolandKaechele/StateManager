#if STATEMANAGER_GM
using UnityEngine;
using GameManager.Runtime;

namespace StateManager.Runtime
{
    /// <summary>
    /// Optional bridge between StateManager and GameManager.
    /// Enable define <c>STATEMANAGER_GM</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// Maps <see cref="GameManager.Runtime.GameState"/> values to <see cref="AppState"/>:
    /// <list type="bullet">
    ///   <item><c>MainMenu</c>  → <see cref="AppState.TitleScreen"/></item>
    ///   <item><c>Loading</c>   → <see cref="AppState.Loading"/></item>
    ///   <item><c>Playing</c>   → <see cref="AppState.Gameplay"/></item>
    ///   <item><c>Paused</c>    → <see cref="AppState.Paused"/></item>
    ///   <item><c>GameOver</c>  → <see cref="AppState.GameOver"/></item>
    ///   <item><c>Victory</c>   → <see cref="AppState.Victory"/></item>
    /// </list>
    /// </para>
    /// <para>Without the scripting symbol this component compiles as a no-op stub.</para>
    /// </summary>
    [AddComponentMenu("StateManager/Game Manager Bridge")]
    [DisallowMultipleComponent]
    public class GameManagerBridge : MonoBehaviour
    {
        private StateManager _state;
        private GameManager.Runtime.GameManager _game;

        private void Awake()
        {
            _state = GetComponent<StateManager>() ?? FindFirstObjectByType<StateManager>();
            _game  = GetComponent<GameManager.Runtime.GameManager>()
                     ?? FindFirstObjectByType<GameManager.Runtime.GameManager>();

            if (_state == null) Debug.LogWarning("[StateManager.GameManagerBridge] StateManager not found.");
            if (_game  == null) Debug.LogWarning("[StateManager.GameManagerBridge] GameManager not found.");
        }

        private void OnEnable()
        {
            if (_game != null) _game.OnStateChanged += OnGameStateChanged;
        }

        private void OnDisable()
        {
            if (_game != null) _game.OnStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState gameState)
        {
            if (_state == null) return;

            switch (gameState)
            {
                case GameState.MainMenu: _state.ChangeState(AppState.TitleScreen); break;
                case GameState.Loading:  _state.ChangeState(AppState.Loading);     break;
                case GameState.Playing:  _state.ChangeState(AppState.Gameplay);    break;
                case GameState.Paused:   _state.ChangeState(AppState.Paused);      break;
                case GameState.GameOver: _state.ChangeState(AppState.GameOver);    break;
                case GameState.Victory:  _state.ChangeState(AppState.Victory);     break;
            }
        }
    }
}
#else
namespace StateManager.Runtime
{
    /// <summary>No-op stub — define <c>STATEMANAGER_GM</c> to activate this bridge.</summary>
    [UnityEngine.AddComponentMenu("StateManager/Game Manager Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class GameManagerBridge : UnityEngine.MonoBehaviour { }
}
#endif
