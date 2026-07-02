# Preset dance choreographies

Format: each dance is `<name>_dance.json` (ordered `stepN` → `{arm, l, r}`) + `<name>_steps.json`
(`{"steps":[beat indices]}`). See `docs/BLE_PROTOCOL.md` for the field semantics.
- `l`, `r` = left/right wheel unit velocity (sign = direction, magnitude = relative speed):
  `l==r`→drive straight, `l==-r`→spin in place, `l=3,r=0`→pivot on one wheel, `l=0,r=0`→arms-only.
- `arm` = `BLEToyCamPosition`: 0 ArmsDown · 4 ArmsOut · 8 LeftArmOut · 12 LeftArmUp ·
  16 LeftArmDown · 20 ArmsUp · 24 ArmsForward.

## Provenance
**Extracted from the Hasbro app** (`scripts/re/extract_assets.py`, the original 10 Connect dances):
`babette, book, chip, cogsworth, dress, horse, lumiere, mirror, rose, snowflake`.

**Original BelleCoder creations** (authored in Belle's move vocabulary — spins, arm-pose hits,
scoots — a demonstration that new/meme-style routines compose from the same atoms):
- `ymca` — the four letter-poses (Y=ArmsUp, M=ArmsForward, C=LeftArmOut, A=ArmsUp) hit on the
  beat, with a spin flourish between verses.
- `spindrop` — accelerating spins build to a freeze-hit (arms up) then a drop (arms slam down)
  on the beat, a shimmy of alternating pivots, then a scoot-out finish.

Belle is a **wheeled** figure with ~7 discrete arm poses, so these are *interpretations* in her
body, not literal human choreography — and they dance to her onboard audio, since new songs can't
be uploaded (see `docs/BLE_PROTOCOL.md`).
