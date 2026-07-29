# Revised Roadmap — DungeonVision

Supersedes the original `ROADMAP.md`. Sequenced to prove the core loop first,
on a clean base. Status key: ✅ done · 🔨 in progress · ⬜ not started

**Where we are:** Phases 0–5 complete. The Green Room now runs four Producer
stations (Bomb Bench, Goo Lab, Wire Rig, Mess Kit) off a data-driven Station
Catalog: each has its own build cost, upgrade ladder, and ingredient/booster
pairing, built/upgraded by spending Prize Vouchers. The full loop works: Play
a floor → harvest → win pays Gold + a Prize Voucher (with an ingredient
tally) → Exit to hub; lose → Continue (escalating Gold, +5 moves, same
board) / Retry / Exit.
Offline production catch-up (5d) is in too: stations fast-forward on launch
for however long the app was closed, capped at 8 hours by default.
**Next: Phase 6** — procedural floors & difficulty tiers.
Test suite: 59 EditMode tests green.

Two currencies live: **Gold** (from wins; funds Continues) and **Prize
Voucher** (+1 per win; funds Green Room station builds/upgrades). Deferred:
visible upgrade art (Phase 8 juice pass). Outstanding small polish: rename
the Play button to "Descend" (one-line scene edit, not yet done).

---

## The near-term goal: First Playable Milestone

Everything up to and including **Phase 4** answers one question: *is the core
loop fun?* With placeholder art, a player should be able to, on a single floor:

1. Make power tiles by matching 4+/shapes.
2. Detonate them to damage the monster **and** harvest color-matched ingredients.
3. Have a station auto-craft a booster (Dynamite) from those ingredients + time.
4. Bring it into the next attempt and set it off.

Steps 1–3 are done (Phases 1–3). Step 4 is Phase 4.

---

## Phases

### ✅ Phase 0 — Cleanup
Stripped the retired roguelite systems (relic reward flow, multi-room Run) back
to a single-floor combat base. Kept board, combat, theme.

### ✅ Phase 1 — Power tiles
- MatchFinder returns classified MatchGroups (runs + 2x2 squares).
- Horizontal 4 → column-clearer, vertical 4 → row-clearer, 2x2 → mortar.
- Power tiles are inert until moved (swapped), then detonate; detonations chain.
- Power tiles spawn at the swap cell (stay where dropped). Rendered as
  brighter, shaped tiles.

### ✅ Phase 2 — Ingredients & harvest
- Four ingredients, one per color (Gunpowder/Toxic Goo/Live Wire/Rations).
- Detonating a power tile yields its color's ingredient (chains included).
- Persistent ingredient stash (JSON save), banks win or lose. On-screen counter.

### ✅ Phase 3 — First Producer station (Bomb Bench)
- ProducerStation: Gunpowder + time → Dynamite, buffer cap, Tick-driven.
- Boosters auto-deposit into a persistent stash (no manual collect); buffer cap
  reserved as the future offline-production cap.
- StationView shows production status + stock. BoosterInventory persisted.

### ✅ Phase 4 — Loadout & in-floor boosters  → **FIRST PLAYABLE MILESTONE**
- Bring one Dynamite into a floor; tap to place/detonate.
- Close the loop end to end with placeholder art.
- **Stop and playtest. Decide the loop is fun before proceeding.**

### ✅ Phase 5 — The Green Room (base view)
- 5a: Moved the Bomb Bench to a hub between floors (Green Room/Floor scene
  split); station ticks there in real time.
- 5b: Build/upgrade stations by spending Prize Vouchers (StationCatalog,
  StationProgress); added the other three producers (Goo Lab, Wire Rig, Mess
  Kit).
- 5d: Offline production catch-up (ProducerStation.FastForward, capped
  window).
- Deferred to Phase 8 (juice pass): visible upgrade art.

### ⬜ Phase 6 — Procedural floors & difficulty tiers
- Depth-driven floor generation. Tiers: Regular / Main Event (hard) / Sweeps
  Week (super-hard), with rare-ingredient drops. Board obstacles introduced
  gradually. Gold rewards scale with depth (first clear).

### ⬜ Phase 7 — Daily Utilities & economy
- Daily-Utility stations (Mess Hall, Cot, Vending Machine) with once/day
  cooldowns, gem bypass. Sponsor Bucks (gems): skip timers, top up ingredients,
  buy gold. Rewarded-ad hooks: rush a timer, double a floor's harvest, daily
  free claim.

### ⬜ Phase 8 — Theme & juice pass
- Buck Diamond host voice, sponsor flavor, achievement pops. Animation/particles
  for matches/detonations/damage; sound; music. Real art within the theme.

### ⬜ Phase 9 — Demo hardening
- Device build (Android/iOS), aspect ratios, performance. Economy/difficulty
  tuning; playtest; fix top bugs.

---

## Scope note on timeline

Original target: demo in 4–6 weeks. Realistic for the **First Playable
Milestone (Phases 0–4)** — the core-loop proof. The fuller demo (Green Room,
procedural depth, economy, daily utilities, theme) is more like **~10–14 weeks
part-time** solo. Treat Phases 0–4 as the near-term goal and re-plan the back
half once the loop is proven.

## Process
- Work phase by phase; keep the game playable at the end of each.
- Commit at the end of every phase (summary + description provided each time).
- Every new rule class in `Game.Core` ships with EditMode tests.
- Keep styling in the theme; keep the Core/Presentation/Gameplay split.

