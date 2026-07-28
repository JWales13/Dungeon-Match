# Revised Roadmap — DungeonVision

Supersedes the original `ROADMAP.md` (which described the retired roguelite
build). Sequenced to **prove the core loop first**, on a **clean base**, per the
revised design doc.

Status key: ✅ done · 🔨 in progress · ⬜ not started

---

## The near-term goal: First Playable Milestone

Everything up to and including **Phase 4** exists to answer one question: *is the
core loop fun?* With placeholder art, a player should be able to, on a single
floor:

1. Make power tiles by matching 4+/shapes.
2. Detonate them to damage the monster **and** harvest color-matched ingredients.
3. Craft a booster (Stick of Dynamite) at one station from those ingredients + a
   timer.
4. Bring it into the next attempt and set it off.

If that loop feels good, the rest is scaffolding worth building. If it doesn't,
we learn it in week one, not week six.

---

## Phases

### ⬜ Phase 0 — Cleanup (clean base)
Remove the retired roguelite systems so we build on a tidy foundation.
- Retire the relic reward flow (`RelicRewardView`, `RelicRewardGenerator`,
  `RelicCatalog`) and multi-room `Run` sequencing (`Run`, run-level win/lose).
- Keep board, combat objective, theme, and the save/catalog patterns.
- Small, self-contained commit.

### ⬜ Phase 1 — Power tiles (board upgrade)
- Extend `MatchFinder` to detect 4+, L, and T matches.
- Create power tiles on those matches; detonation = damage + clear nearby tiles.
- Ingredient-drop hook stubbed for Phase 2.
- EditMode tests for shape detection and detonation.

### ⬜ Phase 2 — Ingredients & harvest
- Four ingredient types, one per color.
- Detonating a power tile yields its color's ingredient.
- Persistent ingredient inventory (extend the save system); banks win or lose.
- Tests for harvest + persistence.

### ⬜ Phase 3 — First Producer station
- One station (Bomb Bench → Dynamite): ingredient + timer production, small
  buffer, collect finished booster.
- Minimal UI (a single button/panel — full base comes in Phase 5).
- Tests for the production rule (ingredients + time gating, buffer cap).

### ⬜ Phase 4 — Loadout & in-floor boosters  → **FIRST PLAYABLE MILESTONE**
- Bring one Dynamite into a floor; tap to place/detonate.
- Close the loop end to end with placeholder art.
- **Stop and playtest. Decide the loop is fun before proceeding.**

### ⬜ Phase 5 — The Green Room (base view)
- Fixed station slots; build/upgrade with Salvage; visible upgrade art.
- Add the remaining three producers (Goo Lab, Wire Rig, Mess Kit) and their
  boosters (Acid Vial, Overcharge, Energy Drink).
- Loadout = one of each owned.

### ⬜ Phase 6 — Procedural floors & difficulty tiers
- Depth-driven floor generation (HP, moves, board, obstacle density).
- Difficulty tiers: Regular / Main Event (hard) / Sweeps Week (super-hard);
  rare-ingredient drops on the hard tiers.
- Introduce board obstacles gradually (locked tiles, crates, armored monsters).
- Salvage rewards scale with depth (first-clear only).

### ⬜ Phase 7 — Daily Utilities & economy
- Daily-Utility stations (Mess Hall, Cot, Vending Machine) with once/day
  cooldowns and gem bypass — unifying with Producers under one Station model.
- Sponsor Bucks (premium): skip timers, top up ingredients, buy Salvage.
- Rewarded-ad hooks: rush a timer, double a floor's harvest, daily free claim.

### ⬜ Phase 8 — Theme & juice pass
- Buck Diamond host voice, sponsor-parody item flavor, achievement pops.
- Animation/particles for matches, detonations, damage; sound; music bed.
- Real art within the existing code-driven theme system.

### ⬜ Phase 9 — Demo hardening
- Device build (Android/iOS), aspect ratios, performance.
- Economy and difficulty tuning; playtest; fix top bugs.

---

## Honest scope note on timeline

The original target was a demo in 4–6 weeks. That target is realistic for the
**First Playable Milestone (Phases 0–4)** — the core-loop proof. The *fuller*
demo (base, procedural depth, economy, daily utilities, theme pass) is a bigger
game than the retired roguelite MVP, and solo is more realistically **~10–14
weeks part-time**. Recommended framing: treat Phases 0–4 as the near-term goal
and the fun gate; re-plan the back half once the loop is proven and you've felt
the real pace.

## Process (unchanged)
- Work phase by phase; keep the game playable at the end of each.
- Commit at the end of every phase (summary + description provided each time).
- Every new rule class in `Game.Core` ships with EditMode tests.
- Keep styling in the theme; keep the Core/Presentation/Gameplay split.