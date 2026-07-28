# Code Style Guide — DungeonVision

Conventions for this project. The goal: any new feature slots in without bending
the architecture, and the riskiest logic stays unit-testable. Based on Robert
C. Martin's Clean Code, adapted for Unity.

## 1. Layered architecture (the most important rule)

Three assemblies (each with its own `.asmdef`), dependencies point *downward*
only. The `.asmdef` references make an illegal dependency a compile error.

```
Game.Core          (pure C#: rules, no MonoBehaviours/views/input)
   ▲
Game.Presentation  (theme/styling; depends on Core for types like TileType)
   ▲
Game.Gameplay      (MonoBehaviours: views, input, composition; depends on both)
```

- **Game.Core** — the domain model and rules. No `MonoBehaviour`, no rendering,
  no input, no styling. Uses UnityEngine only for value types (`Vector2Int`,
  `Mathf`). If a class here needs a `GameObject`, it's in the wrong layer.
- **Game.Presentation** — the theme system (`ITheme`, `DefaultTheme`, `Theme`).
  All visual values and player-facing strings.
- **Game.Gameplay** — everything that touches Unity: views, input, persistence
  repositories, and `GameController` (the composition root).

Never add an upward reference (Core must not reference Gameplay).

## 2. Single Responsibility
Each class has one reason to change. `Board` = grid rules; `MatchFinder` = match
detection; `MonsterCombatObjective` = combat rules; `ProducerStation` = crafting
rules; a view = presentation. When a class grows a second reason to change,
split it (that's why `IngredientHarvester` and `BoardObjectiveDriver` exist as
thin wiring classes rather than living inside `Board`).

## 3. Depend on interfaces, not concretions
`Board` depends on `IMatchFinder`; `InputController` on `IPointerInputSource`;
combat/views on `IBoardObjective`. New rule variants are new implementations, not
edits to existing classes.

## 4. Time is injected, not read from Unity
Anything time-based in Core takes a delta via a `Tick(float seconds)` method
(see `ProducerStation`) instead of reading `Time.deltaTime`. Gameplay calls
`Tick(Time.deltaTime)` each frame. This keeps timing logic deterministic and
unit-testable.

## 5. Styling rules
- No color, font size, or spacing literal in a view script — read it from
  `Theme.Current`.
- No styling set on components in the Inspector. Views apply theme values (color,
  size, scale) in code on init. The Inspector is for layout only (position,
  anchors) and wiring (which prefab, which text object).
- To restyle: edit `DefaultTheme.cs` and update `VISUAL_STYLE_GUIDE.md`.

## 6. Naming
- Types & public members: `PascalCase`. Private fields: `_camelCase`. Locals &
  params: `camelCase`. Constants: `PascalCase`. Interfaces: `I` prefix.
- Names state intent (`matchedCells`, not `list`). No stale-feature names — when
  a system is retired, rename what it left behind (e.g. `RunFlowView` ->
  `FloorResultView`). Use `[FormerlySerializedAs]` when renaming serialized
  fields so scene wiring survives.

## 7. Functions
- Small, single-purpose. Guard clauses over nesting (keep the happy path
  un-indented). No magic numbers (name them). Validate constructor args and
  throw rather than failing silently later.

## 8. Events & lifecycle (Unity)
- Domain objects communicate outward via C# `event`s; presentation subscribes.
  The domain never calls into presentation.
- Every `+=` has a matching `-=` (MonoBehaviours unsubscribe in `OnDestroy`;
  guard against null).
- `GameController` is the only composition root: it `new`s the domain objects
  and wires them to views. Don't scatter construction/wiring elsewhere.

## 9. Testing
- Core logic must be unit-testable without Play mode (no MonoBehaviour/scene
  deps). Prefer deterministic tests: fixed random seeds, explicit-grid `Board`
  constructor, `Tick` deltas, plain integers over driving random systems.
- Every new rule class in `Game.Core` ships with EditMode tests. A feature isn't
  "done" until its Core tests are green.

## 10. Folder & file layout
One public type per file; file name matches the type. Folders group by feature.

```
Scripts/
  Core/                (Game.Core.asmdef)
    Board/             TileType, Tile, IMatchFinder, MatchFinder, Board,
                       PowerTileKind, MatchShape, MatchGroup
    Combat/            ObjectiveStatus, IBoardObjective, MonsterCombatObjective,
                       MoveOutcome, MoveOutcomeBuilder, BoardObjectiveDriver
    Ingredients/       IngredientInventory, IngredientHarvester
    Crafting/          BoosterType, BoosterInventory, ProducerStation
  Presentation/        (Game.Presentation.asmdef) ITheme, DefaultTheme, Theme
  Gameplay/            (Game.Gameplay.asmdef)
    Board/             BoardView, TileView, InputController,
                       IPointerInputSource, UnityPointerInputSource
    UI/                CombatHudView, FloorResultView, IngredientHudView,
                       StationView
    Ingredients/       IngredientInventoryRepository, BoosterInventoryRepository
                       (persistence)
    GameController.cs  (composition root, at the Gameplay root)
Tests/
  EditMode/            all EditMode tests
```

## 11. How to add common things (quick reference)
- **Restyle / new color / font** → edit `DefaultTheme.cs` (+ visual style guide).
- **A new theme** → new `ITheme` implementation; set `Theme.Current`.
- **A new match rule / power tile** → extend `MatchFinder` / `Board` (or a new
  `IMatchFinder`); add EditMode tests.
- **A new ingredient/booster** → add to the enums + theme names; add a producer.
- **A new Producer station** → new `ProducerStation` instance (data-driven
  config); wire it in `GameController` (later, in the Green Room).
- **A new win condition** → new `IBoardObjective` + tests.

If you find yourself editing `Board`, `MatchFinder`, or an existing objective to
add a feature, stop and ask whether an interface should absorb the change.