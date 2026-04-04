#if STATEMANAGER_MLF
using UnityEngine;
using MapLoaderFramework.Runtime;

namespace StateManager.Runtime
{
    /// <summary>
    /// Optional bridge between StateManager and MapLoaderFramework.
    /// Enable define <c>STATEMANAGER_MLF</c> in Player Settings › Scripting Define Symbols.
    /// <para>
    /// <list type="bullet">
    ///   <item>Pushes <see cref="AppState.Loading"/> when a map/scene begins loading.</item>
    ///   <item>Pops the state when the map is fully loaded.</item>
    /// </list>
    /// </para>
    /// <para>Without the scripting symbol this component compiles as a no-op stub.</para>
    /// </summary>
    [AddComponentMenu("StateManager/Map Loader Bridge")]
    [DisallowMultipleComponent]
    public class MapLoaderBridge : MonoBehaviour
    {
        private StateManager _state;
        private MapLoaderFramework.Runtime.MapLoaderFramework _mlf;

        private void Awake()
        {
            _state = GetComponent<StateManager>() ?? FindFirstObjectByType<StateManager>();
            _mlf   = GetComponent<MapLoaderFramework.Runtime.MapLoaderFramework>()
                     ?? FindFirstObjectByType<MapLoaderFramework.Runtime.MapLoaderFramework>();

            if (_state == null) Debug.LogWarning("[StateManager.MapLoaderBridge] StateManager not found.");
            if (_mlf   == null) Debug.LogWarning("[StateManager.MapLoaderBridge] MapLoaderFramework not found.");
        }

        private void OnEnable()
        {
            if (_mlf != null)
            {
                _mlf.OnChapterChanged += OnChapterChanged;
                _mlf.OnMapLoaded      += OnMapLoaded;
            }
        }

        private void OnDisable()
        {
            if (_mlf != null)
            {
                _mlf.OnChapterChanged -= OnChapterChanged;
                _mlf.OnMapLoaded      -= OnMapLoaded;
            }
        }

        private void OnChapterChanged(int _, int _2) => _state?.PushState(AppState.Loading);
        private void OnMapLoaded(MapData _)          => _state?.PopState();
    }
}
#else
namespace StateManager.Runtime
{
    /// <summary>No-op stub — define <c>STATEMANAGER_MLF</c> to activate this bridge.</summary>
    [UnityEngine.AddComponentMenu("StateManager/Map Loader Bridge")]
    [UnityEngine.DisallowMultipleComponent]
    public class MapLoaderBridge : UnityEngine.MonoBehaviour { }
}
#endif
