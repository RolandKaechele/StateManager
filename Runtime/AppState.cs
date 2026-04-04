using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace StateManager.Runtime
{
    // -------------------------------------------------------------------------
    // AppState
    // -------------------------------------------------------------------------

    /// <summary>Built-in application states managed by <see cref="StateManager"/>.</summary>
    public enum AppState
    {
        /// <summary>Initial boot sequence (<see cref="BootStartupManager"/>).</summary>
        Boot,

        /// <summary>Title / main menu screen.</summary>
        TitleScreen,

        /// <summary>A scene or map is loading asynchronously.</summary>
        Loading,

        /// <summary>Normal gameplay — the player is in control.</summary>
        Gameplay,

        /// <summary>Game is paused; <c>Time.timeScale</c> may be 0.</summary>
        Paused,

        /// <summary>A dialogue sequence is active.</summary>
        Dialogue,

        /// <summary>A cutscene sequence is playing.</summary>
        Cutscene,

        /// <summary>A mini-game is active.</summary>
        MiniGame,

        /// <summary>Inventory UI is open.</summary>
        Inventory,

        /// <summary>Gallery / CG viewer is open.</summary>
        Gallery,

        /// <summary>Player lost; game-over screen visible.</summary>
        GameOver,

        /// <summary>Player won; victory screen visible.</summary>
        Victory,

        /// <summary>A custom state defined in <c>states.json</c>.</summary>
        Custom
    }

    // -------------------------------------------------------------------------
    // CustomStateDefinition
    // -------------------------------------------------------------------------

    /// <summary>
    /// A user-defined state loaded from JSON.
    /// Serializable so it works with <c>JsonUtility</c>.
    /// </summary>
    [Serializable]
    public class CustomStateDefinition
    {
        [Tooltip("Unique identifier referenced at runtime, e.g. \"shop\".")]
        public string id;

        [Tooltip("Human-readable name shown in the Inspector and debug UI.")]
        public string displayName;

        [Tooltip("Optional category tag for grouping, e.g. \"ui\" or \"gameplay\".")]
        public string category;

        [Tooltip("Whether this state is modal (pushed onto the stack instead of replacing it).")]
        public bool modal = true;
    }

    // ─── JSON wrapper ─────────────────────────────────────────────────────────

    [Serializable]
    internal class CustomStateManifestJson
    {
        public CustomStateDefinition[] states;
    }
}
