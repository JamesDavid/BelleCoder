# Belle BLE Protocol — reverse-engineering results

Output of SPEC §11. Everything here was extracted **statically** from the official Hasbro
*Dance Code* Android app (`com.hasbro.dancecode` v1.0.1, 2017-12-15) — a Unity **Mono** build
whose `Assembly-CSharp.dll` decompiles to clean C#. No live GATT probing was needed to get
the protocol; §11.4 live confirmation on the doll is still required before flipping
`BLE_ENABLED 1`.

## Provenance / trust
- Source: APKPure mirror, package `com.hasbro.dancecode`, size 92,788,069 bytes,
  SHA-256 `bc79a90465259c2af2f03f331298dce2f080971e522066b596689e118b5a4794`.
- Signing cert in the APK is **`META-INF/HASBROAN.RSA`** (Hasbro-named key), consistent with
  a genuine Hasbro build rather than a re-signed repack.
- Engine: Unity + Mono (`lib/*/libmono.so`, `assets/bin/Data/Managed/Assembly-CSharp.dll`).
  BLE native plugin: **startechplus** bridge (`startechplus/ble/*`, a Prime31-style plugin) →
  Android `BluetoothGatt` on the Java side, driven by C# above it.
- Decompiled with dnSpyEx `dnSpy.Console.exe` (netframework build) → `re/decompiled/`.

## GATT summary (fills the PAL directly)

| PAL field | Value | Source |
|---|---|---|
| `DOLL_NAME_PREFIX` (adv-name filter) | **`DanceCD`** | `BLEToyAPI.toyNameFilter` |
| Service UUID | **discovered dynamically** — not hardcoded; matched by characteristic. Enumerate services, pick the one containing the write char. | `BLEToyAPI.onDiscoveredCharacteristic` |
| Write char (app→toy) `CMD_CHAR_UUID` | **`51901383-030F-4859-B643-256B0B2F5562`** | `BLEToyAPI.appToToyCharacteristic` |
| Notify char (toy→app) | **`51901382-030F-4859-B643-256B0B2F5562`** | `BLEToyAPI.toyToAppCharacteristic` |
| `WRITE_NO_RESP` | **true** — Write **Without Response** (`withResponse=false`) | `startechplus/ble/BluetoothHandler.cs:99` |
| Bonding / pairing | **None (open GATT, just-works).** `SetPassword`(64)/`SecurityChallengeResponse`(65) opcodes exist but the shipping app's connect path never invokes them. First-time association is a *physical* gesture — press the doll's necklace button — not a crypto pairing. | `BLEToyAPI` connect path; `ToyConnectedRoutine` necklace wait |
| MTU / packet size | Commands are single writes, **≤ 20 bytes** (app warns above 20). | `BLEToyAPI.SendPacketToToy` |

Connect sequence the app uses: scan (filter adv name == `DanceCD`) → connect → discover services/chars
→ subscribe to notify char `…82` → send `AppModeSignal(RCAppModeKeepAlive)` to enter app mode →
stream command packets → on exit send `AppModeSignal(EndAppMode)`. A keep-alive
`AppModeSignal(RCAppModeKeepAlive)` is pinged periodically or the toy drops app mode.

## Packet format
```
[ opcode : 1 byte ] [ params : 0..N bytes ]      // total ≤ 20, one GATT write per command
```
Opcode is `packet[0]`. Multi-byte params are **little-endian** (`BitConverter.GetBytes` on ARM/x86).

## Opcode table (`AppToToyOpCodes`, verified against each encoder)

| Opcode (dec / hex) | Name | Packet bytes | Notes |
|---|---|---|---|
| 16 / 0x10 | PlayAudioSequence | `[16, idxLo, idxHi]` | uint16 LE phrase/song index (see audio catalog) |
| 17 / 0x11 | EnqueueSequenceCommand | `[17, <cmd words…>]` | queue motor/LED keyframe sequence (uint16 words) |
| 18 / 0x12 | StopSequenceQueuePlayback | `[18]` | |
| 19 / 0x13 | CheckSequenceQueueFreeSpace | `[19]` | toy replies on notify char |
| 20 / 0x14 | MotorRun | `[20, motor, state, power, dir]` | power 0–255; see motor enums |
| 21 / 0x15 | SetLED | `[21, R, G, B]` | 0–255 each |
| 23 / 0x17 | PlaySequence | `[23, seqId…]` | play a stored/high-level sequence |
| 24 / 0x18 | StopSequence | `[24, flags]` | `BLEToyStopSequenceFlag` |
| 25 / 0x19 | Motor1Goto / CamGotoPosition | `[25, dir, position]` | arms/cam positioning (see cam enum) |
| 32 / 0x20 | RequestInputState | `[32]` | necklace/button state via notify |
| 33 / 0x21 | RequestLVDValue | `[33]` | low-voltage-detect → battery level |
| 34 / 0x22 | CheckToyVolume | `[34]` | |
| 35 / 0x23 | SetToyVolume | `[35, vol]` | vol clamped 0–5 |
| 64 / 0x40 | SetPassword | — | unused by app; security path |
| 65 / 0x41 | SecurityChallengeResponse | — | unused by app; security path |
| 80 / 0x50 | AppModeSignal | `[80, signal]` | signal = `AppModeSignal` enum below |

(Opcode 22 is reserved/unused.)

### Parameter enums
```
BLEToyMotor:        ArmsCam=0, LeftWheel=1, RightWheel=2, BothWheels=3
BLEToyMotorState:   Run=0, Brake=1, Coast=2
BLEToyMotorDirection: Forward=0, Reverse=1
BLEToyCamPosition:  ArmsDown=0, ArmsOut=4, LeftArmOut=8, LeftArmUp=12,
                    LeftArmDown=16, ArmsUp=20, ArmsForward=24
AppModeSignal:      ChecksumROM=128, ChecksumFlash=129, GetFlashVersionCode=130,
                    EndAppMode=140, RCAppModeKeepAlive=141, EnterLowPowerSleepMode=145
```

### Audio catalog
`BLEToyAudioPhrase` enumerates **~421** phrase/song IDs (`eP001…`), passed as the uint16 LE
index to opcode 16 (`PlayAudioSequence`). `BLEToyHLSequences.SequenceForPhrase()` links a phrase
to a coordinated motion (`BLEToyHLSequence`), which is how a phrase plays audio + movement.

### Songs (named — `BLEToyAudioPhraseSong`, indices 251–260)
The songs occupy a dedicated block of the audio index space and the app ships kid-facing names
(`SongSelectionMenu`). The box's "7 songs" = 3 licensed *Beauty and the Beast* songs (each with
an **A** and **B** arrangement) + 4 original instrumental pieces = **10 selectable tracks**:

| Index | Enum | Display name |
|---|---|---|
| 251 | TaleAsOldAsTimeA | Tale As Old As Time A |
| 252 | BeOurGuestA | Be Our Guest A |
| 253 | TaleAsOldAsTimeB | Tale As Old As Time B |
| 254 | BeOurGuestB | Be Our Guest B |
| 255 | SomethingThereA | Something There A |
| 256 | SomethingThereB | Something There B |
| 257 | OriginalSong1 | The Rose Waltz |
| 258 | OriginalSong2 | Fairytale Allegro |
| 259 | OriginalSong6 | The Fleur-de-lis |
| 260 | OriginalSong7 | The Beastly Bourrée |

Songs are **not** played with a bare opcode-16 write — they go through the sequence queue as
looped audio-channel commands (`PlayLoopingSong`): each loop pushes word `0x3000 | songIndex`
then a marker word `0x2820`, via opcode 17. Volume is forced to 5 first.

## Predefined sequences (two kinds)

### 1. On-doll high-level sequences — `BLEToyHLSequence` (~454 entries, firmware-resident)
Triggered by index; the doll runs the choreography+audio itself. Encoding is
`PlaySequence`(23): **`[23, 0x02, seqLo, seqHi]`** (the `0x02` = `BLEToySequenceType.HighLevel`).
Named families in the enum:
- `eHLS_Wake_1..5`, `eHLS_Sleep_1..7`, `eHLS_ConnectStart_1..5`
- `eHLS_IdlePhrase_2..27`, `eHLS_IdleInstruct_1..9`
- `eHLS_DanceIntro_1..3`, **`eHLS_DanceName_1..10` (+ `_11_ALT`)** — the 10 named preset dances
- `eHLS_DanceInstruct_*` (per-dance call-outs: First/Then/Final/Like, with ALTA/ALTB variants)

### 2. App-side "Connect" dances — 10 JSON choreographies (in the APK, not the doll)
`Pots_Menu.pageNames` lists ten preset connect-the-dots dances, each named after a *Beauty and
the Beast* character/object:
```
chip, book, cogsworth, dress, horse, lumiere, mirror, rose, snowflake, babette
```
Each is a **Unity TextAsset** at `Resources/Connect/<name>/dance` (+ `/steps`), parsed as JSON
into a list of per-step `{ arm, l, r }` — arm = `BLEToyCamPosition`, `l`/`r` = left/right wheel
values. The app plays them **client-side**, stepping through and issuing `MotorRun`(20) /
`CamGotoPosition`(25) per step (see `PlayDrawnLine.cs`), braking between steps. These are
extractable from the APK's Unity assets → ready-made choreography we can replay or seed
BelleCoder sequences with.

### Extracted preset dances (in-repo)
All 10 Connect dances are extracted to `assets/presets/dances/<name>_{dance,steps}.json`
(via `scripts/re/extract_assets.py`, resolved through the Unity ResourceManager container map).

`<name>_dance.json` — ordered steps, each a wheel/arm command:
```json
{ "step0": { "arm": "0", "l": "-1", "r": "1" }, "step1": { … }, … }
```
- `l`, `r` = left/right wheel **unit velocity** (signed: sign = direction, magnitude = relative
  speed). The app maps these to `MotorRun` power via the doll wheel-speed calibration
  (`PlayDrawnLine`: wheel power ≈ `min(1, |r/l|) · speedMult`).
  - `l == r` (both +) → drive straight; `l == -r` → spin in place; `l=3,r=0` → pivot on one wheel.
- `arm` = target `BLEToyCamPosition` index for that step (0 = ArmsDown in these presets).

`<name>_steps.json` — `{ "steps": [beat indices…] }` = the `tweenSteps` checkpoints (which step
indices are "beats", used to sync the choreography to the connect-the-dots nodes / music).

| Dance | steps | spins | Dance | steps | spins |
|---|---|---|---|---|---|
| babette | 43 | 7 | lumiere | 41 | 15 |
| book | 28 | 4 | mirror | 27 | 4 |
| chip | 29 | 6 | rose | 34 | 9 |
| cogsworth | 48 | 15 | snowflake | 22 | 0 |
| dress | 45 | 14 | horse | 51 | 11 |

These are pure motion data (no audio baked in) — directly convertible into BelleCoder
`Sequence`s to ship a "preset dances" library on day one.

## Phrase-duration table (in-repo)
`data/phrase_durations.csv` — **412** phrase indices → playback seconds, parsed from
`BLEToyAudio.TimeForPhrase()`. Range 0.62 s … 23.1 s. Feeds `durationOf()` for `PLAY_PHRASE`
/ `PLAY_SONG` so the runner's step timing matches the doll. (Song indices 251–260 are present
at ~2–6 s = one loop each; `PlayLoopingSong` repeats a song 8×.)

## Can a kid build a NEW sequencer (music / dances)? — yes for motion, partial for audio
Every primitive is **individually addressable**, which is exactly what a sequencer needs:

- **Motion — fully open.** `MotorRun`(20) drives each wheel and the arm/cam motor independently
  (motor, run/brake/coast, 0–255 power, direction); `Motor1Goto`/`CamGotoPosition`(25) sets
  discrete arm poses; `SetLED`(21) sets RGB. The 10 preset dances are *literally* just ordered
  `{arm,l,r}` tuples — proof that arbitrary new choreography is composable from the same atoms.
  BelleCoder's whole editor model (place steps → run) maps 1:1 onto this.
- **Audio — trigger-only, not authoring.** `PlayAudioSequence`(16) can fire **any** of the ~421
  onboard phrases/songs by uint16 index, and the sequence queue (opcode 17,
  `word = (type<<12)|value`) can **interleave audio + motor + LED + LED-fade + HL** steps into one
  timed routine. So a kid can freely *sequence and remix* the existing sounds against motion.
  **But** the audio samples live in the doll's firmware — there is **no way to upload new audio**.
  "New music" therefore means arranging the built-in phrase/song library, not synthesizing new
  sound. (LED patterns and motion have no such limit — those are truly freeform.)

Net: the doll is a general **motion + LED + indexed-audio playback engine**, so a BelleCoder
sequencer that composes movement/lights freely and treats audio as a palette of ~421 triggerable
clips is fully supported by the real protocol.

## Sequence-command word format (opcode 17 `EnqueueSequenceCommand`)
Packs up to **7** uint16 LE words per write: `[17, w0lo,w0hi, … w6lo,w6hi]` (≤15 bytes; longer
sequences auto-split across writes). Each word is:
```
word = (BLEToySequenceType << 12) | value
BLEToySequenceType: LED=0, Motor=1, HighLevel=2, AudioChannel1=3, AudioChannel2=4, Queue=5
```
e.g. `0x3000 | songIndex` = play a song on audio channel 1. This is how LED/motor/HL/audio steps
are composed into one queued routine on the doll.

## How SPEC §5 moves map onto Belle (she's a **wheeled** doll)

Belle drives on two hidden wheels plus an arms/cam motor — there are no discrete
"twirl/step/arm" opcodes; those are **composed** from `MotorRun` (20) / `Motor1Goto` (25) /
`PlayAudioSequence` (16) / `PlaySequence` (23):

| SPEC move | Belle realization |
|---|---|
| `STEP_FORWARD` / `STEP_BACK` | `MotorRun` BothWheels Run, dir Forward/Reverse, timed |
| `TWIRL_LEFT` / `TWIRL_RIGHT` | `MotorRun` LeftWheel + RightWheel in **opposite** directions |
| `ARMS_UP/DOWN`, `ARM_L/R_*` | `Motor1Goto`(25) to a `BLEToyCamPosition`, or `MotorRun` ArmsCam |
| `HEAD_*` | cam positions (no separate head motor exposed) |
| `PLAY_SONG` / `PLAY_PHRASE` | `PlayAudioSequence`(16) with uint16 index |
| `PLAY_DANCE` (presets) | `PlaySequence`(23) / an `eHLS_*` high-level sequence |
| `WAIT` | runner-side delay (no toy opcode) |

Implication for the PAL: `durationOf()` for locomotion/spin is a **motor-on time**, and those
moves emit a Run packet then a Brake packet (`StopMotors`) after the duration, rather than a
single fire-and-forget opcode. Model twirl/step as (start-motor, wait, brake).

## Still to confirm live on the doll (§11.4)
- Exact wheel power + on-time that yields "1 step" and "90°/180° twirl" — tune to real motion.
- Whether periodic `RCAppModeKeepAlive` cadence matters and the app-mode idle timeout.
- Real per-move durations for `durationOf()`.
- Which `eHLS_*`/phrase indices correspond to the 7 named songs / 10 preset dances on the box.
- Confirm no notify-gated readiness handshake is required before the toy accepts motor commands.

## Reproduce
```
scripts/re/get_apk.sh          # download + integrity/cert check
scripts/re/decompile.sh        # dnSpy.Console.exe Assembly-CSharp.dll -> re/decompiled/
scripts/re/grep_ble.sh         # pull UUIDs / opcodes / write-type out of the decompile
```
Full decompiled BLE surface is checked in under `re/decompiled/` (see `BLEToyAPI.cs`,
`AppToToyOpCodes.cs`, `startechplus_ble/BluetoothHandler.cs`).
