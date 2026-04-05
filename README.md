# StateManager

An application-level pushdown-automaton state machine for Unity.  
Tracks the current app state with a stack that supports modal overlays (Dialogue, Cutscene, MiniGame) on top of a base state (Gameplay, TitleScreen, etc.).  
Supports custom JSON-defined states for modding and full optional integration with GameManager, CutsceneManager, DialogueManager, MiniGameManager, MapLoaderFramework, EventManager, and SaveManager.


## Features

- **Pushdown automaton stack** — `PushState()` / `PopState()` for modal states; `ChangeState()` to replace the whole stack
- **Built-in states** — `Boot`, `TitleScreen`, `Loading`, `Gameplay`, `Paused`, `Dialogue`, `Cutscene`, `MiniGame`, `Inventory`, `Gallery`, `GameOver`, `Victory`, `Custom`
- **Custom states / Modding** — define arbitrary string-keyed states from the Inspector or `StreamingAssets/states.json`; JSON entries are **merged by id**
- **Events** — `OnStateChanged`, `OnStatePushed`, `OnStatePopped`, `OnCustomStateChanged` for reactive system design
- **State queries** — `CurrentState`, `PreviousState`, `HasState(state)`, `IsCustomState(id)`, `StateStack`
- **GameManager integration** — maps `GameState` enum values to `AppState` automatically (activated via `STATEMANAGER_GM`)
- **CutsceneManager integration** — auto-push `Cutscene` state and pop on complete/skip (activated via `STATEMANAGER_CSM`)
- **DialogueManager integration** — auto-push `Dialogue` state and pop on complete (activated via `STATEMANAGER_DM`)
- **MiniGameManager integration** — auto-push `MiniGame` state and pop on complete/abort (activated via `STATEMANAGER_MGM`)
- **MapLoaderFramework integration** — auto-push `Loading` state during map loads and pop on loaded (activated via `STATEMANAGER_MLF`)
- **EventManager integration** — broadcast `state.changed`, `state.pushed`, `state.popped` events (activated via `STATEMANAGER_EM`)
- **SaveManager integration** — persist current state in save data; optionally restore on load (activated via `STATEMANAGER_SM`)
- **EventManager re-broadcast** — EventManager can re-broadcast state events as GameEvents (activated via `EVENTMANAGER_STM`)
- **Custom Inspector** — live state display, stack depth, and one-click transition controls in Play Mode
- **Odin Inspector integration** — `SerializedMonoBehaviour` base for full Inspector serialization of complex types; runtime-display fields marked `[ReadOnly]` in Play Mode (activated via `ODIN_INSPECTOR`)


## Installation

### Option A — Unity Package Manager (Git URL)

1. Open **Window → Package Manager**
2. Click **+** → **Add package from git URL…**
3. Enter:

   ```
   https://github.com/RolandKaechele/StateManager.git
   ```

### Option B — Clone into Assets

```bash
git clone https://github.com/RolandKaechele/StateManager.git Assets/StateManager
```

### Option C — npm / postinstall

```bash
cd Assets/StateManager
npm install
```


## Scene Setup

1. Create a persistent manager GameObject (or reuse your existing manager object).
2. Attach `StateManager`.
3. Set `initialState` (default: `Boot`).
4. Attach any bridge components you need (see below).
5. Add `DontDestroyOnLoad(gameObject)` if the manager should persist across scenes.


## Quick Start

### 1. Add StateManager to your scene

| Field | Default | Description |
| ----- | ------- | ----------- |
| `initialState` | `Boot` | State set on Awake |
| `customStates` | *(empty)* | Optional custom state definitions |
| `loadFromJson` | `false` | Merge custom states from `states.json` |
| `jsonPath` | `"states.json"` | Path relative to `StreamingAssets/` |
| `maxStackDepth` | `16` | Maximum state stack depth |
| `verboseLogging` | `false` | Log all transitions to Console |

### 2. Change, push, and pop states

```csharp
var sm = FindFirstObjectByType<StateManager.Runtime.StateManager>();

// Replace the whole stack
sm.ChangeState(AppState.Gameplay);

// Push a modal state (overlay)
sm.PushState(AppState.Dialogue);

// Return to previous state
sm.PopState();

// Query
AppState current = sm.CurrentState;     // Dialogue
AppState previous = sm.PreviousState;   // Gameplay
bool inCutscene = sm.HasState(AppState.Cutscene);
```

### 3. React to transitions

```csharp
sm.OnStateChanged      += (prev, next) => Debug.Log($"{prev} → {next}");
sm.OnStatePushed       += state        => Debug.Log($"Pushed: {state}");
sm.OnStatePopped       += state        => Debug.Log($"Popped: {state}");
sm.OnCustomStateChanged += id          => Debug.Log($"Custom: {id}");
```

### 4. Use custom states

```csharp
// JSON or Inspector definition required first
sm.ChangeState("shop");             // AppState.Custom with id "shop"
sm.PushState("map_overview");       // modal custom state
bool inShop = sm.IsCustomState("shop");

CustomStateDefinition def = sm.GetCustomDefinition("shop");
```


## Bridge Components

Add these to the same GameObject as StateManager (or any scene GameObject):

| Component | Define | Effect |
| --------- | ------ | ------ |
| `GameManagerBridge` | `STATEMANAGER_GM` | Maps `GameManager.GameState` → `AppState` |
| `CutsceneManagerBridge` | `STATEMANAGER_CSM` | Push/pop `Cutscene` state |
| `DialogueManagerBridge` | `STATEMANAGER_DM` | Push/pop `Dialogue` state |
| `MiniGameManagerBridge` | `STATEMANAGER_MGM` | Push/pop `MiniGame` state |
| `MapLoaderBridge` | `STATEMANAGER_MLF` | Push/pop `Loading` state |
| `EventManagerBridge` | `STATEMANAGER_EM` | Fire `state.*` GameEvents |
| `SaveManagerBridge` | `STATEMANAGER_SM` | Persist state in save data |


## Scripting Define Symbols

Enable the integrations you want in **Project Settings → Player → Scripting Define Symbols**:

| Symbol | Requires | Effect |
| ------ | -------- | ------ |
| `STATEMANAGER_GM` | GameManager | Map GameState → AppState |
| `STATEMANAGER_CSM` | CutsceneManager | Auto Cutscene push/pop |
| `STATEMANAGER_DM` | DialogueManager | Auto Dialogue push/pop |
| `STATEMANAGER_MGM` | MiniGameManager | Auto MiniGame push/pop |
| `STATEMANAGER_MLF` | MapLoaderFramework | Auto Loading push/pop |
| `STATEMANAGER_EM` | EventManager | Fire state.* events |
| `STATEMANAGER_SM` | SaveManager | Persist state in save |
| `EVENTMANAGER_STM` | EventManager + StateManager | EventManager re-broadcasts state events |
| `ODIN_INSPECTOR` | Odin Inspector (Asset Store) | `SerializedMonoBehaviour`; `[ReadOnly]` on runtime fields |

> **AiManager** and **EnemyManager** also listen to state changes and freeze AI agents / pause spawning during `Cutscene`, `Dialogue`, `Paused`, and `MiniGame` states (activated via `AIMANAGER_STM` and `ENEMYMANAGER_STM` defines on the respective manager sides).


## JSON / Modding

Enable `loadFromJson` and place `states.json` in `StreamingAssets/`:

```json
{
  "states": [
    {
      "id":          "shop",
      "displayName": "Shop",
      "category":    "ui",
      "modal":       true
    },
    {
      "id":          "map_overview",
      "displayName": "Map Overview",
      "category":    "ui",
      "modal":       true
    }
  ]
}
```

JSON entries are **merged by id** with Inspector definitions.


## License

MIT — see [LICENSE](LICENSE).
