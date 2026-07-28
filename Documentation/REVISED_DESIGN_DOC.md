# Revised Design Doc — working title: **DungeonVision**

A campy, PG-13 parody of the deadly game-show genre: a
match-3 / base-building / idle-crafting hybrid. You're a contestant forced to
match-3 your way down an endless dungeon live on **DungeonVision** — the
galaxy's #1 deadly game show — while the "Producers" sell your suffering to
sponsors. Between episodes you hole up in your **Green Room**, scavenging and
crafting to survive one more floor.

*(Title note: "DungeonVision" is both the working game title and the name of the
in-fiction TV show the whole descent is broadcast on.)*

*(All names below are proposals — veto anything.)*

---

## Design pillars

1. **Never hard-stuck without paying.** Worst case is ever a wait, never a wall.
2. **The board feeds the base feeds the board.** Playing well funds your
   crafting; crafting smooths the next floor.
3. **Build the room.** The core long-term chase is growing your Green Room, and
   every station you add is also a reason to log in tomorrow.
4. **Skill is the star.** Crafted help is a head-start; the real firepower is
   the power tiles you earn by matching well.

---

## §1 Foundation

- **One floor at a time.** Play the next floor of the tower ("Episode"); on
  clear, descend one deeper. No world map to manage.
- **Win condition: monster-HP combat.** Each floor is a monster with an HP bar;
  drain it to zero by matching within a move limit. (Reuses the combat engine we
  already built.)
- **No lives, no energy.** Unlimited retries. The *only* gated resource is your
  regrowing crafted boosters. Worst case is a wait.
- **Endless procedural tower.** Floors are generated with difficulty scaling by
  depth (see §7). No fixed end.

## §2 The board

- Standard match-3 grid, four tile colors.
- **Plain 3-matches** deal damage to the monster.
- **4+/L/T matches create power tiles** (in-level boosters: rockets/bombs/etc.).
- **Detonating a power tile does triple duty:** damage the monster + clear
  nearby tiles + drop a small amount of the **ingredient** matching its color.
- **Four ingredients, one per color** (see naming below). Ingredients bank
  **win or lose** — every attempt is progress.
- Ingredients only come from the floor you're currently on (no replaying cleared
  floors), so you always farm the wall in front of you. This prevents hoarding.

Proposed color → ingredient → crafted booster mapping:

| Color  | Ingredient  | Crafted booster   | Effect                              |
|--------|-------------|-------------------|-------------------------------------|
| Red    | Gunpowder   | Stick of Dynamite | 3x3 blast: damage + clear           |
| Green  | Toxic Goo   | Acid Vial         | Clears a full row *and* column      |
| Yellow | Live Wire   | Overcharge        | Converts several tiles to power tiles|
| Blue   | Rations     | Energy Drink      | +3 moves this floor                 |

## §3 Stations & crafting

- **Crafted boosters** are made back in the Green Room, then carried *into* a
  floor (see §4).
- Each **Producer station** makes one booster type and auto-produces it, gated
  by **both ingredients and a timer** (farm-sim style): it needs enough of its
  ingredient *and* real time to finish one. Time is the fair-first pacing (and
  the thing gems/ads can skip).
- Stations hold a **small, upgradeable buffer** (start ~1-2). You return to
  collect finished boosters.
- **Gold ("Salvage")** earned from clearing floors **builds new stations**
  (unlocking new booster types) and **upgrades** existing ones (faster
  production, bigger effect, more storage).

Proposed producer names: **Bomb Bench** (Dynamite), **Goo Lab** (Acid Vial),
**Wire Rig** (Overcharge), **Mess Kit** (Energy Drink).

## §4 Boosters

- Four starter crafted boosters, one per color/ingredient (table above).
- **Loadout: one of each type you own.** Building more stations is how you gain
  booster variety; floors scale to match. (A loadout slot-cap can be added later
  if power creep appears.)
- **Modest head-start.** Boosters ease a floor; the in-level power tiles you earn
  by skilled matching are the real firepower. Keeps difficulty honest.

## §5 Base & progression — The Green Room

- A single room with **fixed station slots**. Building/upgrading a station
  **visibly transforms** it (a leveled Bomb Bench looks bigger and nastier), and
  the room grows from a bare holding cell into a fortified den as you descend.
- This visual payoff is load-bearing: with fair-first monetization, revenue
  leans on **aspiration** — players spending to grow a room they want to grow.
- **Deferred past the demo:** pure-cosmetic decor, and additional wings/zones of
  the hideout. For now, functional stations carry all the visual growth.

## §6 Currencies & the fair-first economy

Three currencies:

- **Salvage (soft / "gold").** First-clear reward per floor, scaling with depth.
  Spent building/upgrading stations. Replays give no Salvage (you can't replay
  cleared floors anyway).
- **Ingredients (four types).** From detonating power tiles on the current
  floor. Fuel crafted-booster production.
- **Sponsor Bucks (hard / premium "gems").** Skip production timers, top up
  ingredients, buy Salvage.

**Fair-first rule:** gems and ads only ever buy *time and convenience* — never
raw power, never unblocking a wall.

- **Gem sinks:** skip production timers, top up ingredients, buy Salvage.
  (Storage/slot expansions are bought with **Salvage**, keeping capacity
  earnable.)
- **Rewarded ads (pure accelerant):** rush one production timer, double a
  floor's ingredient harvest, and a free **daily** gem/booster claim.

## §7 Retention & difficulty

- **Endless procedural tower.** Each floor generated from a difficulty curve
  driven by depth: monster HP, move limit, board size, and obstacle density.
- **Difficulty tiers as "boss" beats:** most floors are **Regular**; periodic
  **Main Event** (hard) and **Sweeps Week** (super-hard) floors spike the
  challenge and drop **rare ingredients**. (Familiar Candy-Crush-style tiering.)
- **Rising difficulty = numbers + gimmicks:** HP/move pressure climbs, and new
  board obstacles are introduced gradually (locked tiles, crates, armored
  monsters).
- **Core chase = building the room.** A growing roster of stations.

### Two station categories (the unified, scalable model)

Everything in the Green Room is a **Station** — one data object with a category,
an effect, a timer/cooldown, upgrade levels, a Salvage build cost, and a gem
bypass cost. Two categories:

1. **Producers** — make a crafted booster over time from ingredients (§3).
2. **Daily Utilities** — a once-per-day active perk, the daily limit skippable
   with gems. Proposed: **Mess Hall** (start a floor with +moves, 1/day),
   **Cot** (rush one timer, 1/day), **Vending Machine** (a free random booster,
   1/day).

This scales cleanly: a new station is one catalog entry plus art. Every Daily
Utility doubles as a **daily login hook**, so the "build the room" chase and the
retention loop are the same system. Guardrail: keep each perk modest — since the
tower is endless and scaling, perks help you climb deeper rather than
trivializing a fixed ceiling.

## §8 Theme & voice

- **Framing:** a campy, self-aware parody of deadly-game-show fiction. The
  "Producers" run **DungeonVision** — the in-fiction deadly game show the whole
  descent is broadcast on; sponsors run the shops and crafting; your host is
  proposed **Buck Diamond**, a game-show emcee who treats your mortal peril as
  light entertainment.
- **Tone:** sardonic but PG-13 — dark-comic wit, store-safe, no crude edges.
- **Voice hooks:** sponsor-parody item flavor ("This Stick of Dynamite brought
  to you by Grimtooth Insurance — now covering 4% of your afterlife!"),
  backhanded achievement pops, and Buck's between-floor banter.
- Uses the existing code-driven theme system for all colors/typography.

---

## §9 What we keep vs. rebuild (from the current codebase)

**Keep as-is / lightly extend:**
- Match-3 engine (`Board`, `Tile`, `MatchFinder`) — core, unchanged. *Extend*
  `MatchFinder` to detect 4+/L/T shapes for power-tile creation.
- Combat objective (`MonsterCombatObjective`, `MoveOutcome/Builder`,
  `BoardObjectiveDriver`) — still the win condition; extend the driver to also
  emit ingredient harvest.
- Presentation/theme system — unchanged.
- Persistence pattern (`MetaProgress` + `MetaProgressRepository`) — extend to
  hold Salvage, Sponsor Bucks, ingredients, station levels, and cooldowns.
- The data-driven catalog pattern (used for relics/upgrades) — reused for the
  **Station** catalog.
- `GameFlowController` skeleton — becomes the Green Room ↔ Floor orchestrator.

**New systems to build:**
- **Power tiles** (creation on 4+/shape matches, detonation effects) on the
  board.
- **Ingredient harvest** wired to power-tile detonation.
- **Stations** (Producers + Daily Utilities) with ingredient+timer production,
  buffers, cooldowns, and upgrades.
- **Crafted-booster inventory & loadout** (one of each owned), and in-floor
  activation.
- **Green Room base view** (fixed slots, visible upgrades).
- **Procedural floor generation** + difficulty tiers (Regular/Prime-Time/Sweeps).
- **Sponsor Bucks + ads** economy hooks.

**Retire (from the roguelite build):**
- Per-run relic *reward* flow (`RelicRewardView`, `RelicRewardGenerator`,
  `RelicCatalog`) — replaced by crafted boosters/stations. (Some relic *effect*
  math is a useful reference for booster effects.)
- Multi-room `Run` sequencing (`Run`, run-level win/lose) — replaced by
  one-floor-at-a-time descent with persistent depth.

---

## §10 Draft roadmap (to refine together)

Rough re-sequencing toward a playable vertical slice. We'll tune the order and
scope together.

- **Phase A — Board upgrade:** power tiles (4+/shape detection, detonation:
  damage + clear + ingredient drop). Reuses/extends the engine.
- **Phase B — Ingredients & inventory:** harvest, four ingredient types,
  persistent inventory; extend save system.
- **Phase C — Stations (Producers):** one-station-one-booster, ingredient+timer
  production, buffers, build/upgrade with Salvage.
- **Phase D — Loadout & crafted boosters in-floor:** bring one of each, activate
  on the board (Dynamite/Acid/Overcharge/Energy Drink).
- **Phase E — Green Room base view:** fixed slots, visible upgrades, the room as
  the hub the floor launches from.
- **Phase F — Procedural floors & difficulty tiers:** depth-driven generation,
  Regular/Main Event/Sweeps, obstacles.
- **Phase G — Daily Utilities & economy:** Mess Hall/Cot/Vending Machine,
  Sponsor Bucks, rewarded-ad hooks.
- **Phase H — Theme & juice pass:** Buck Diamond voice, sponsor flavor, art,
  sound, feedback.
- **Phase I — Demo hardening:** device build, tuning, playtest.