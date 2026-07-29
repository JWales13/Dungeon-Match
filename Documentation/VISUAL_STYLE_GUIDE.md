# Visual Style Guide — DungeonVision

The single source of truth for the game's look. Every value here is implemented
in code in `Assets/_Project/Scripts/Presentation/DefaultTheme.cs`. This document
is the human-readable companion. **If you change a value in one, change it in
the other** so they never drift.

Rule of thumb: no color, font size, or spacing value should appear as a literal
inside a view script or be set on a component in the Unity Inspector. It lives
here and in `DefaultTheme.cs`. The Inspector is only for *layout* (position,
anchors).

## Art direction

Campy, PG-13 parody of the deadly game-show genre. A grim, low-lit dungeon
"stage": near-black background, punchy saturated tiles under the spotlight,
loud blunt result text.

## Color palette

### Background / stage

| Token       | Hex       | RGB          | Use                         |
|-------------|-----------|--------------|-----------------------------|
| Background  | `#18161C` | 24, 22, 28   | Camera clear color / stage  |

### Tile colors

Chosen for high mutual contrast (they differ in brightness as well as hue, so
they read for color-blind players — keep that if you swap them).

| Token      | Hex       | RGB           | Ingredient it yields |
|------------|-----------|---------------|----------------------|
| Red        | `#E0524D` | 224, 82, 77   | Gunpowder            |
| Blue       | `#4D8BE0` | 77, 139, 224  | Rations              |
| Green      | `#5FB85C` | 95, 184, 92   | Toxic Goo            |
| Yellow     | `#E8C33D` | 232, 195, 61  | Live Wire            |
| (unmapped) | magenta   | 255, 0, 255   | Deliberate "you forgot to map a tile" flag |

### Feedback / result

| Token         | Hex       | RGB           | Use                          |
|---------------|-----------|---------------|------------------------------|
| Matched flash | `#FFFFFF` | 255,255,255   | Brief flash on cleared tiles |
| Victory       | `#6FCF60` | 111, 207, 96  | Floor win banner             |
| Defeat        | `#D64545` | 214, 69, 69   | Floor loss banner            |

### HUD text

| Token     | Hex       | RGB           | Use                     |
|-----------|-----------|---------------|-------------------------|
| HUD text  | `#EDEDED` | 237, 237, 237 | All HUD labels          |

## Power tiles

Power tiles render brighter than normal and are shaped to telegraph their
effect (column-clearer = vertical bar, row-clearer = horizontal bar, mortar =
small square).

| Token                | Value | Use                                                    |
|----------------------|-------|--------------------------------------------------------|
| Power tile highlight | 0.45  | How far a power tile's color blends toward white (0-1) |
| Power tile thickness | 0.40  | The thin edge of a power-tile bar (long edge = Tile scale) |

## Crates (board obstacles)

Placeholder look for the demo: a crate darkens toward an "obstacle" brown-gray
regardless of remaining hits (per-hit visual feedback is a Phase 8 juice-pass
item, not implemented yet).

| Token               | Value                | Use                                      |
|---------------------|-----------------------|-------------------------------------------|
| Crate overlay color | `#3C3830` (60,56,48) | The color a crate's tile blends toward    |
| Crate overlay amount| 0.65                 | How far a crate's color blends toward it (0-1) |

## Typography

TextMeshPro for all text. Sizes in TMP font-size units. Applied in code from
the theme (views call the theme on init), so the Inspector never controls text
size/color.

| Token              | Size | Use                                     |
|--------------------|------|-----------------------------------------|
| HUD label          | 28   | Primary labels (Monster HP, Moves)      |
| Caption            | 22   | Secondary labels (ingredient counter, station) |
| Result banner      | 60   | Floor win/lose banner                   |

## Layout / spacing

| Token       | Value | Use                                                 |
|-------------|-------|-----------------------------------------------------|
| Cell size   | 1.0   | World-unit distance between tile centers            |
| Tile scale  | 0.9   | Normal tile sprite scale (the 0.1 gap reads as grid)|

## Ingredient & booster names

Player-facing names, centralized in the theme (`GetIngredientName`,
`GetBoosterName`).

| Color  | Ingredient | Booster it crafts |
|--------|------------|-------------------|
| Red    | Gunpowder  | Dynamite          |
| Green  | Toxic Goo  | Acid Vial         |
| Yellow | Live Wire  | Overcharge        |
| Blue   | Rations    | Energy Drink      |

## Result strings

| Token                | Text          | Use                              |
|----------------------|---------------|----------------------------------|
| Victory message      | `VICTORY`     | Combat HUD result (legacy, dormant) |
| Defeat message       | `ELIMINATED`  | Combat HUD result (legacy, dormant) |
| Floor victory        | `YOU SURVIVED`| Floor result banner              |
| Floor defeat         | `ELIMINATED`  | Floor result banner              |

## How to change the look

1. Edit the value in `DefaultTheme.cs`.
2. Update the matching row here.
3. Press Play — the change appears everywhere that value is used, no view
   scripts touched, nothing edited in the Inspector.

## How to add a whole new theme

1. Create a new class implementing `ITheme` (copy `DefaultTheme`, change values).
2. Set `Theme.Current = new YourTheme();` where you want it to take effect.
3. Views pick it up the next time they read `Theme.Current`.