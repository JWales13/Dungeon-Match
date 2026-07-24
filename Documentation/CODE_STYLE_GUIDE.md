# Code Style Guide

Conventions for this project. The goal is that any new feature slots in without
bending the architecture, and that the riskiest logic stays testable. Based on
Robert C. Martin's Clean Code principles, adapted for Unity.

## 1. Layered architecture (the most important rule)

Code is split into assemblies (each with its own `.asmdef`) that may only
depend *downward*. The `.asmdef` references make illegal dependencies a compile
error, not a code-review afterthought.

```
Game.Core          (pure C#: rules, no Unity views/input)
   ▲
Game.Presentation  (styling/theme; depends on Core for types like TileType)
   ▲
Game.Gameplay      (MonoBehaviours: views, input, composition; depends on both)
```

- **Game.Core** — the domain model and rules: `Board`, `MatchFinder`,
  `MonsterCombatObjective`, `BoardObjectiveDriver`. No `MonoBehaviour`, no
  rendering, no input, no styling. Uses UnityEngine only for value types
  (`Vector2Int`, `Mathf`). If a class here needs a `GameObject`, it's in the
  wrong layer.
- **Game.Presentation** — the theme system (`ITheme`, `DefaultTheme`, `Theme`).
  All visual values. No game rules, no MonoBehaviours.
- **Game.Gameplay** — everything that touches Unity: views (`BoardView`,
  `TileView`, `CombatHudView`), input (`InputController`, input sources), and
  the composition root (`GameController`).

**Never** add a reference that points upward (Core must not reference Gameplay).
If you feel the need, the logic is probably in the wrong layer.

## 2. Single Responsibility

Each class has one reason to change.

- `Board` changes only if grid mechanics change.
- `MonsterCombatObjective` changes only if combat rules change.
- A view changes only if *presentation* changes.

When a class starts having two reasons to change (e.g. grid logic *and* score
logic), split it — that's how `BoardObjectiveDriver` came to exist instead of
stuffing combat into `Board`.

## 3. Depend on interfaces, not concretions (Dependency Inversion)

Collaborators are referenced through interfaces so they can be swapped or
tested:

- `Board` depends on `IMatchFinder`, not `MatchFinder`.
- `InputController` depends on `IPointerInputSource`, not `UnityEngine.Input`.
- `GameController` and `BoardObjectiveDriver` depend on `IBoardObjective`.

New rule variants (diagonal matches, a score objective, an Input-System input
source) are new implementations of an existing interface — existing code is not
edited.

## 4. Styling rules

- No color, font size, or spacing literal in any view script. Read it from
  `Theme.Current`.
- No styling set on components in the Inspector (colors, sizes, scales). Views
  apply theme values in code at startup.
- Inspector fields are for *wiring* (which prefab, which camera, which text
  object) and *design tuning that isn't visual style* (monster health, move
  limit) — not for appearance.
- To restyle: edit `DefaultTheme.cs` and update `VISUAL_STYLE_GUIDE.md`. Never
  hunt through view scripts.

## 5. Naming

- **Types & public members**: `PascalCase` (`Board`, `TrySwap`, `CurrentHealth`).
- **Private fields**: `_camelCase` with a leading underscore (`_board`,
  `_matchFinder`).
- **Local variables & parameters**: `camelCase` (`matchedCells`, `moveLimit`).
- **Constants**: `PascalCase` (`MinimumMatchLength`).
- **Interfaces**: `I` prefix (`IMatchFinder`, `IBoardObjective`, `ITheme`).
- Names state intent, not type: `matchedCells`, not `list`. No abbreviations
  beyond well-known ones (`x`, `y`, `id`).

## 6. Functions

- Small and single-purpose; if a method does "scan, then decide, then record,"
  split those into named private methods (see `MatchFinder`).
- **Guard clauses over nesting**: return/continue early instead of wrapping the
  body in `if`. Keep the happy path un-indented.
- No magic numbers — name them (`MinimumMatchLength = 3`).
- Validate constructor arguments and throw (`ArgumentOutOfRangeException`,
  `ArgumentNullException`) rather than failing silently later.

## 7. Events & lifecycle (Unity-specific)

- Domain objects (`Board`, `MonsterCombatObjective`) communicate outward via C#
  `event`s. Presentation subscribes; the domain never calls into presentation.
- Every `+=` subscription has a matching `-=`. MonoBehaviours unsubscribe in
  `OnDestroy`; guard against null in case `Initialize` never ran.
- `GameController` is the only composition root: it `new`s up the domain
  objects and wires them to views. Don't scatter `new Board(...)` or manual
  wiring across other MonoBehaviours.

## 8. Testing

- Core logic must be unit-testable without Play mode. That means keeping rules
  free of `MonoBehaviour`/scene dependencies (see `BoardTests`,
  `MonsterCombatObjectiveTests`).
- Prefer deterministic tests: pass a fixed random seed to `Board`, and feed
  objectives plain integers rather than driving a random board.
- Every new rule class in `Game.Core` ships with EditMode tests. Add a matching
  test file under `Assets/_Project/Tests/EditMode/`.
- A feature isn't "done" until its Core tests are green in the Test Runner.

## 9. Folder & file layout

- One public type per file; file name matches the type.
- Folders mirror assemblies: `Scripts/Core`, `Scripts/Presentation`,
  `Scripts/Gameplay`, `Tests/EditMode`.
- Prefabs in `Prefabs/`, art in `Art/`, docs in `Documentation/`.

## 10. How to add common things (quick reference)

- **A new tile color / restyle** → edit `DefaultTheme.cs` (+ visual style guide).
- **A whole new theme** → new `ITheme` implementation; set `Theme.Current`.
- **A new win condition** (score target, collect-X) → new `IBoardObjective`
  implementation + its own tests; `GameController` picks which to instantiate.
- **A new match rule** (diagonals, shapes) → new `IMatchFinder` implementation;
  pass it into `Board`.
- **A different input method** → new `IPointerInputSource` implementation;
  assign it on the Input object.

In every case: add a new class implementing an existing interface. If you find
yourself editing `Board`, `MatchFinder`, or an existing objective to add a
feature, stop and ask whether an interface should absorb the change instead.
