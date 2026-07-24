# Visual Style Guide

The single source of truth for the game's look. Every value here is
implemented in code in `Assets/_Project/Scripts/Presentation/DefaultTheme.cs`.
This document is the human-readable companion: designers/decision-makers read
this, the code enforces it. **If you change a value in one, change it in the
other** so they never drift.

Rule of thumb: no color, font size, or spacing value should ever appear as a
literal inside a view script or be set on a component in the Unity Inspector.
It lives here and in `DefaultTheme.cs`, nowhere else.

## Art direction

The tone is dark-comedy dungeon game show: a grim, low-lit "arena" with
punchy, saturated tiles that pop against a near-black stage. The background
should feel like an unlit set; the tiles are the spotlight. Result text is
loud and blunt (VICTORY / ELIMINATED) to match the deadpan game-show emcee
voice.

## Color palette

### Background / stage

| Token             | Hex       | RGB           | Use                          |
|-------------------|-----------|---------------|------------------------------|
| Background        | `#18161C` | 24, 22, 28    | Camera clear color / stage   |

### Tile colors

Chosen for high mutual contrast so the four types are unmistakable, including
for the most common forms of color-blindness (they differ in brightness as
well as hue — don't pick replacements that are distinguishable by hue alone).

| Token   | Hex       | RGB            | Notes                       |
|---------|-----------|----------------|-----------------------------|
| Red     | `#E0524D` | 224, 82, 77    | Warm crimson                |
| Blue    | `#4D8BE0` | 77, 139, 224   | Mid azure                   |
| Green   | `#5FB85C` | 95, 184, 92    | Fresh green                 |
| Yellow  | `#E8C33D` | 232, 195, 61   | Gold, not lemon             |
| (unmapped) | magenta | 255, 0, 255  | Deliberate "you forgot to map a tile type" flag — should never ship visible |

### Feedback / result

| Token             | Hex       | RGB            | Use                         |
|-------------------|-----------|----------------|-----------------------------|
| Matched flash     | `#FFFFFF` | 255, 255, 255  | Brief flash on cleared tiles|
| Victory           | `#6FCF60` | 111, 207, 96   | "VICTORY" banner            |
| Defeat            | `#D64545` | 214, 69, 69    | "ELIMINATED" banner         |

### HUD text

| Token             | Hex       | RGB            | Use                         |
|-------------------|-----------|----------------|-----------------------------|
| HUD text          | `#EDEDED` | 237, 237, 237  | HP / Moves labels           |

## Typography

TextMeshPro for all text. Sizes are in TMP font-size units.

| Token             | Size | Use                                    |
|-------------------|------|----------------------------------------|
| HUD label         | 28   | "Monster HP: x/y", "Moves: n"          |
| Result banner     | 60   | VICTORY / ELIMINATED                    |

Font family is not yet fixed — when a custom font is chosen, add a `HudFont`
token to `ITheme` / `DefaultTheme` and this table, and have `CombatHudView`
apply it, rather than setting the font per-object in the Inspector.

## Layout / spacing

| Token       | Value | Use                                                |
|-------------|-------|----------------------------------------------------|
| Cell size   | 1.0   | World-unit distance between tile centers           |
| Tile scale  | 0.9   | Tile sprite scale; the 0.1 gap reads as a grid line|

## Result strings

Player-facing copy also lives in the theme (so tone/wording is centralized and
localizable later):

| Token           | Text          |
|-----------------|---------------|
| Victory message | `VICTORY`     |
| Defeat message  | `ELIMINATED`  |

## How to change the look

1. Edit the value in `DefaultTheme.cs`.
2. Update the matching row in this document.
3. Press Play — the change appears everywhere that value is used, with no view
   scripts touched and nothing edited in the Inspector.

## How to add a whole new theme (e.g. a "boss arena" palette)

1. Create a new class implementing `ITheme` (copy `DefaultTheme`, rename, change
   values).
2. Set `Theme.Current = new YourTheme();` at the point you want it to take
   effect (e.g. when a boss room loads).
3. Views pick it up automatically the next time they read `Theme.Current`.
