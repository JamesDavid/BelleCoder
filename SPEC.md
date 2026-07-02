# BelleCoder — CYD Dance Programmer

**Spec v0.5**
A standalone ESP32 (CYD) **dance-programmer wand** that lets a user build dance sequences on a touchscreen — or **by physically dancing with the device** — and stream them to the Hasbro *Dance Code featuring Disney Princess Belle* doll over BLE, replacing the discontinued Hasbro app.

*v0.2 adds an onboard IMU and a "Dance to Code" capture mode: wave/spin/bounce the CYD, and the motion is segmented and classified into the same editable dance-step model.*
*v0.3 makes the IMU optional and auto-detected on I2C: one firmware image runs on boards with or without the sensor, enabling or hiding the capture features at boot accordingly.*
*v0.4 adds the BLE reverse-engineering phase (§11) — the upfront investigation that produces the UUIDs/opcodes the PAL needs — as an explicit, self-contained stage of the dev process.*
*v0.5 settles the platform: a dedicated CYD wand (not an iPad-native app or a Web Bluetooth web app). See "Platform decision" below.*

### Platform decision (settled v0.5)
Three targets were evaluated:
- **Web app (Web Bluetooth + DeviceMotion)** — ruled out for the kids: the family devices are iPads, and Safari/WebKit — hence every iOS/iPadOS browser — has no Web Bluetooth. It would only run inside a third-party BLE browser (Bluefy), which is poor UX to hand a child.
- **iPad-native (SwiftUI + CoreBluetooth + CoreMotion)** — technically strong and low-friction on existing hardware, but ties the toy to Apple provisioning (re-signing, no permanent install) and adds "borrow the iPad" screen-time friction.
- **CYD wand (chosen)** — a dedicated physical object: always paired, never expires, no phone/tablet handoff, indestructible toybox artifact, and Apple-independent. The tradeoff accepted is building the enclosure / power / IMU-detection that CoreMotion would otherwise provide for free.

Note: the **reverse-engineering phase (§11) is done on a laptop regardless** — Chrome + Web Bluetooth on the Mac, or BlueZ/`bluetoothctl` on the Linux workstation. The all-Apple consumer devices don't participate in RE.

---

## 1. Goals

- Build, save, and replay dance sequences entirely on-device. No phone, no app store, no cloud.
- Run **fully usable today**, before the doll's BLE protocol is known, via a SIMULATE mode.
- Isolate every unknown (UUIDs, opcodes, timings) in one Protocol Abstraction Layer (PAL) so post-reverse-engineering integration is a ~6-line edit + one `#define` flip.
- Two skill tiers: a **Kid** palette (big icons, no loops) and an **Advanced** palette (full instruction set + repeat blocks). Kid tier is the GridBot-adjacent experience for Theo.
- **Programming by demonstration:** capture motion from an onboard IMU and turn a real dance into an editable sequence — the "your dance becomes code you can see and change" loop. Output is the *same* `Sequence` model as the tap editor, so a danced routine is fully reviewable/tweakable afterward.
- **One binary, capability-detected features:** the IMU is optional. At boot the app probes I2C; if a supported IMU answers, capture features turn on, otherwise they're hidden entirely. No separate build, no crashes on boards without the sensor.

### Non-goals (v0.1)
- Recreating Hasbro's exact graphics or song library.
- Music/phrase *authoring* — we only trigger the doll's onboard songs/phrases by index.
- Multi-doll / multi-central coordination.
- OTA. Flash over USB.

---

## 2. Target hardware

Hardware specifics below follow the empirical reference docs (`../BladeAir/cyd.md`, and the `BladeKey-Overhead` `hal/Board.h` / `hal/LGFX_Config.h` multi-target abstraction), which take precedence over any summary description — these are hard-won findings from shipping on this exact board.

- **Primary:** CYD `ESP32-2432S028R` — ESP32-WROOM (4 MB flash, no PSRAM). 2.8" ILI9341 SPI panel, **physically 240×320 portrait**, driven **landscape-native 320×240 at MV=0 (LovyanGFX rotation 6)** — the odd/MV=1 "landscape" rotations render 90° wrong on this mounting and can only differ by a 180° mirror, so they can never reach the orientation we need. XPT2046 resistive touch on a **separate VSPI bus** (`SPI3_HOST`), IRQ **not** gated (`pin_int = -1`, poll pressure — if we wait on IRQ, `getTouch()` may never return).
  - **Colour:** this panel has reversed R/B wiring → set `rgb_order = true` (else warm theme colours render cyan/blue). `invert` is a per-USB-variant compile flag: dual-USB (USB-C) boards need `invertDisplay(true)`, single-USB (micro-USB) don't.
  - **Backlight:** GPIO 21, active-HIGH, PWM at **≤5 kHz** — the onboard MOSFET can't switch off above ~10 kHz, so higher frequencies dim nothing (`cyd.md`).
  - **Touch does not auto-rotate** with the display and needs 4-corner calibration + per-axis inversion (the MV=0 landscape flip point-reflects the learned map → invert both calibrated axes).
- **Variant:** CrowPanel Advance S3 5.0-HMI — ESP32-S3 (16 MB flash, 8 MB PSRAM), 800×480 parallel-RGB (ST7262), GT911 **capacitive** touch (no calibration), backlight behind an I2C expander. Materially different display/touch/power path from the 2.8" CYD, but the BLE half is identical (ESP32-S3 BLE is fine for NimBLE central). Handled as a **separate build env, not a fork**, per the `-D BOARD_*` + `hal/Board.h` + `hal/LGFX_Config.h` pattern from Overhead — adding a board is a new `[env:*]` block and a new HAL branch, nothing above the HAL changes.
- BLE: classic ESP32 / S3 on-chip radio, central role. NimBLE (not Bluedroid) to keep the flash footprint down (`cyd.md`: NimBLE still adds ~240 KB → custom partition table).
- Storage: onboard flash, LittleFS partition for saved sequences. `LittleFS` over `Preferences`/NVS for anything past a few hundred bytes (`cyd.md`).

### IMU (v0.2)

- **6-axis is sufficient and preferred.** Spins = yaw rate (gyro Z, integrated); bounces = vertical accel (accel Z). Both live on any 6-axis part. The magnetometer on a 9-axis part gives absolute heading but is unreliable here — the device is waved next to the doll's motors and speaker magnet plus the CYD's own electronics.
- **6-axis candidates:** ICM-42670-P or LSM6DSO (clean, modern, low-noise) or MPU-6050 (cheap, ubiquitous, fine for this).
- **Go 9-axis only for on-chip fusion convenience:** BNO085 / BNO055 output a stabilized quaternion + gravity-compensated linear acceleration directly, letting you skip a Madgwick/Mahony filter. That's the only reason to spend the extra — not the magnetometer.
- **Bus:** I2C on the **P3 "3V3 / Temp-humidity" JST connector — SDA = GPIO 27, SCL = GPIO 22** (`Wire.begin(27, 22)`). Per `cyd.md`, the chip's hardware-default SDA (GPIO 21) is hijacked for the backlight, so the CYD designers routed GPIO 27 to P3 for I2C use; GPIO 27 and 22 are otherwise completely free. Do **not** use the "Extended IO" connector (GND/35/22/21) — its GPIO 21 is the backlight and GPIO 35 is input-only. Use the connector's **3V3 pin** to power the sensor, never 5V. (Pinouts still vary by revision — confirm against the silkscreen labels, which `cyd.md` says to trust over any wiki.)
- **Auto-detection:** at boot, probe the known I2C addresses and confirm via each chip's WHO_AM_I / chip-ID register so a different device at the same address can't be mistaken for the IMU. Detection result sets a single runtime `imuPresent` flag (see §9). Supported parts and their addresses:

  | Part | I2C addr | ID register / expected |
  |---|---|---|
  | MPU-6050 | 0x68 (0x69 if AD0=1) | WHO_AM_I 0x75 → 0x68 |
  | ICM-42670-P | 0x68 / 0x69 | WHO_AM_I 0x75 → 0x67 |
  | LSM6DSO | 0x6A / 0x6B | WHO_AM_I 0x0F → 0x6C |
  | BNO085 | 0x4A / 0x4B | product-ID report (SHTP) |
  | BNO055 | 0x28 / 0x29 | CHIP_ID 0x00 → 0xA0 |

  *(Verify exact IDs against the datasheet for the part you actually stock; the table is a starting set.)*
- **Power & form factor (committed target):** the device is a self-contained **wand** the child holds and waves — untethered. Requires an onboard **LiPo + charge/boost** (the stock CYD has no battery management; add a TP4056-class charger or an integrated charge/boost board, plus a battery-voltage ADC tap for a level indicator). Housed in a parametric OpenSCAD **handle/wand enclosure** with a wrist strap, sized for small hands and drop-tolerant. The IMU is mounted **rigidly to the board** (no flex, no cable slack) so gyro/accel track the wand's true motion. Enclosure + power are first-class deliverables here, not afterthoughts — they're the cost accepted in the platform decision.

---

## 3. Software stack

| Concern | Choice |
|---|---|
| Display/touch | LovyanGFX (panel + touch configured per board) |
| BLE | NimBLE-Arduino (central) |
| Storage | LittleFS |
| Serialization | Compact JSON (ArduinoJson) for saved sequences |
| IMU driver | Sensor-specific (e.g. SparkFun/Adafruit lib for the chosen part; BNO08x lib if fused) |
| Build | PlatformIO. Per-board platform split (Overhead precedent): registry `espressif32` (arduino-esp32 2.0.x) for the no-PSRAM 2.8" CYD; `pioarduino` (arduino-esp32 3.x) for the S3 CrowPanel, whose parallel-RGB `esp_lcd` driver isn't in the 2.0.x platform. Each board is its own `[env:*]` with a `-D BOARD_*` flag. |

UI is **tap-to-build**, not drag-and-drop — resistive touch makes drag fiddly, and tap-to-insert is more reliable for small hands.

---

## 4. Architecture

```
+------------------+      +-------------------+      +------------------+
|   UI Layer       |----->|  Sequence Model   |----->|  Player / Runner |
| (screens, touch) |      |  (steps, repeat)  |      |  (step pacing)   |
+------------------+      +-------------------+      +---------+--------+
         ^                     ^      |                         |
         |                     |      | save/load               | sendMove()
+--------+---------+           |      v                         v
| Motion Capture & |           |  +-------------------+  +------------------+
| Gesture Classify |-----------+  |  Storage (LFS)    |  |  PAL (protocol)  |
| (IMU -> steps)   |              +-------------------+  +---------+--------+
+--------+---------+                                               |
         |                                          BLE_ENABLED? / SIMULATE
      IMU (I2C)                                                    |
                                                         +---------+--------+
                                                         | NimBLE central   |
                                                         +------------------+
```

The capture pipeline **emits the same `Sequence` model** as the tap editor, so it reuses storage, editing, and the runner unchanged. PAL is untouched by v0.2.

---

## 5. Move catalog (instruction set)

Atomic actions the UI can place into a sequence. Exact set is provisional until RE confirms what the doll exposes; unsupported entries are simply hidden by a capability flag in the PAL.

| Category | Move | Params |
|---|---|---|
| Locomotion | `STEP_FORWARD`, `STEP_BACK` | steps (1–4) |
| Spin | `TWIRL_LEFT`, `TWIRL_RIGHT` | amount (1–4) |
| Arms | `ARM_L_UP/DOWN`, `ARM_R_UP/DOWN`, `ARMS_UP/DOWN` | — |
| Head | `HEAD_LEFT`, `HEAD_RIGHT`, `HEAD_CENTER` | — |
| Sound | `PLAY_SONG` | song id (see catalog) |
| Sound | `PLAY_PHRASE` | phrase index (see catalog) |
| Preset | `PLAY_DANCE` | preset id (see library) |
| Control | `WAIT` | ms |

The RE phase (§11) has since pinned the real catalog — the sound/dance indices are **data, checked into the repo**, not guesses:
- **`data/audio_catalog.csv`** — all **412** onboard clips with a kid-facing title, palette group (Songs / Cues / Praise / Greetings / Dances / Chatter / Sleepy / "Belle talks"), and duration. The **10 named songs** are indices 251–260 (`Tale As Old As Time A/B`, `Be Our Guest A/B`, `Something There A/B`, `The Rose Waltz`, `Fairytale Allegro`, `The Fleur-de-lis`, `The Beastly Bourrée`). The palette shows these titles, not raw `eP` numbers. (`data/phrase_labels.csv` and `data/phrase_durations.csv` are the source tables it's built from.)
- **`assets/presets/dances/`** — the **10 preset dances** (`PLAY_DANCE`) extracted from the Hasbro app as `{arm,l,r}` step data, plus original BelleCoder demos (`ymca`, `spindrop`). These seed the M2 preset library directly.

**Reality check from RE (`docs/BLE_PROTOCOL.md`):** Belle is a **wheeled** figure (two drive wheels + one arm/cam motor + RGB LED), so `STEP_*`/`TWIRL_*`/`ARMS_*` are *composed* from timed `MotorRun` + arm-pose commands (start → wait → brake), not single opcodes. `HEAD_*` maps to arm/cam poses (no separate head motor). Audio is trigger-only: any of the 412 onboard clips by index, but no new audio can be uploaded.

Advanced tier adds one control block:
- `REPEAT(n) { ... }` — flat, single-level loop in v0.1 (no nesting). Stored as a span with a repeat count; expanded at run time. (Nested AST/tiers are a later GridBot-style extension.)

---

## 6. Sequence model

```c
struct Step {
  MoveId   move;
  int16_t  p1;        // primary param (steps / index / ms)
  uint8_t  repeatGrp; // 0 = none; >0 groups consecutive steps for REPEAT
};

struct Sequence {
  char     name[24];
  uint8_t  count;          // up to MAX_STEPS (suggest 32)
  Step     steps[MAX_STEPS];
};
```

- Persisted as JSON in `/seq/<name>.json` on LittleFS.
- One "scratch" sequence is the live edit buffer; Save promotes it to a named file.

---

## 7. UI screens

1. **Home / Connect**
   - Scan + connect to doll; show name, RSSI, link state.
   - Big **SIMULATE** toggle (on by default until BLE_ENABLED).
   - Buttons: New, Load, Settings.

2. **Editor**
   - Vertical list of steps (icon + label + param). Tap a step to select.
   - Bottom toolbar: **+ Add** (opens palette), **Insert**, **Delete**, **Move ↑/↓**.
   - Header: sequence name, step count `n/32`, Run button.

3. **Palette** (modal)
   - Tabs by category (Move / Arms / Head / Spin / Sound / Dance / Wait).
   - Kid tier shows only Move, Arms, Spin, Dance with oversized icons.
   - Selecting a move with params opens a small stepper for the value.

4. **Dance to Code** (v0.2) — *shown only when an IMU is detected (§9); hidden entirely otherwise*
   - Big **Record** button; calibration prompt ("hold still").
   - Live recognition feedback: spinning glyph / up-down arrows / tilt arrows as motion is sensed; each committed gesture flashes its move icon + chime.
   - **Stop** → classified steps drop into the scratch sequence and open in the Editor for review.

5. **Run**
   - Play / Pause / Stop. Currently-executing step highlights.
   - Progress `step k of N`. Honors per-move timing from PAL so the screen stays in sync with the doll.

6. **Settings**
   - Mode: Kid / Advanced.
   - BLE target override (name filter / MAC), write-mode, global inter-step gap.
   - **Capture sensitivity** slider; bounce → arms/step toggle; re-calibrate IMU. *(These three appear only when an IMU is detected.)*
   - Re-scan for IMU (bench/debug aid); IMU status readout in Advanced mode.
   - Re-scan BLE, forget device.

---

## 8. Player / Runner

- Expands `REPEAT` groups into a flat execution list.
- For each step: call `pal.sendMove(move, p1)`, then wait `pal.durationOf(move, p1) + globalGap`.
- Non-blocking (millis-based state machine) so touch (Stop) stays responsive.
- Stop aborts after the current in-flight command; optionally sends a `HEAD_CENTER`/neutral "settle" move if PAL defines one.

---

## 9. Motion capture — "Dance to Code" (v0.2, IMU auto-detected v0.3)

Turns physical motion into a `Sequence`. Primary model is **record → segment → classify** (not live mirroring), so the danced routine lands in the editor as discrete, tweakable steps.

### Capability detection & graceful degradation (v0.3)
The entire capture feature is gated on a single runtime flag set once at boot:

```c
// imu.h — thin capability + abstraction layer, mirrors the PAL pattern
enum ImuKind { IMU_NONE, IMU_MPU6050, IMU_ICM42670, IMU_LSM6DSO, IMU_BNO08X, IMU_BNO055 };

ImuKind  imuDetect();      // probe I2C addrs, verify WHO_AM_I, retry a few times; IMU_NONE if absent
bool     imuPresent();     // == (kind != IMU_NONE)
bool     imuBegin();       // init detected part; false → treat as absent (fail safe)
```

Detection rules:
- Probe each candidate address (`beginTransmission`/`endTransmission` ACK), then read the chip-ID register and require the expected value. An ACK alone is not enough — avoids a false positive from an unrelated device sharing an address.
- Retry the scan ~2–3× with a short delay before concluding absent (cold-boot I2C can miss the first attempt).
- If a part is detected but `imuBegin()` (or ID check) fails, fall back to `IMU_NONE` — the app is never worse off than a board with no sensor.

Graceful removal when `!imuPresent()`:
- The **Dance to Code** screen and its Home entry are **hidden, not greyed** — the child never sees a dead button.
- Capture-only Settings (sensitivity, bounce mapping, re-calibrate) are omitted.
- No IMU library is initialized, no capture task is spawned; everything else — tap editor, storage, BLE, runner — behaves identically.
- Advanced mode shows a small diagnostic line on Home (`IMU: none` / `IMU: MPU-6050 @0x68`) for the builder; Kid mode shows nothing.

Everything below assumes `imuPresent()` is true.

### Sensing
- Sample IMU at ~100 Hz on a timer/task.
- Gyro: bias-calibrate at capture start (device held still ~1 s). Integrate yaw rate with dt for spin angle.
- Accel: high-pass to remove static gravity → dynamic/vertical acceleration for bounce peaks; low-frequency component → tilt/lean angle. (If using a fused BNO08x, take linear-accel + gravity + gyro reports directly and skip the manual filtering.)

### Segmentation
- **Stillness delimits gestures.** When motion stays below threshold for ~300–500 ms, the current gesture closes and the gap optionally becomes a `WAIT`. Kid-natural: *do a move, pause, do another.*
- Cap maximum single-gesture duration so a continuous wave doesn't become one giant token.

### Classification (heuristic, no ML in v1)
| Observed motion | Emitted move | Param mapping |
|---|---|---|
| Sustained yaw rotation | `TWIRL_LEFT/RIGHT` | sign of gyro Z → direction; ∫angle ÷ unit (e.g. 180°) → amount |
| Vertical bounce (up-spike then down) | `ARMS_UP/DOWN` or `STEP` (configurable) | bounce count → repeat |
| Sustained roll (side tilt) | `HEAD_LEFT/RIGHT` | held past threshold |
| Sustained pitch (lean fwd/back) | `STEP_FORWARD/BACK` | hold duration → steps |
| Below all thresholds | `WAIT` | gap duration |

Thresholds are tunable in Settings (a **sensitivity** slider); kids vary a lot in how vigorously they move. Calibration step + per-axis thresholds keep false triggers down.

### Live feedback during capture
Immediate, legible feedback so the child connects motion → recognition:
- On-screen indicator reacts in real time — a spinning glyph while rotating, up/down arrows on a bounce, tilt arrows for head.
- Each recognized gesture flashes its move icon + a chime as it's committed.
- (No vibration motor on stock CYD; audio/visual only unless one is added.)

### Output & review
On Stop, the classified steps populate the **scratch sequence** and open in the **Editor**, where the child can delete a stray step, fix a param, add a `PLAY_SONG`, then Run on Belle. This preserves the "I danced it → here it is as code → I can change it" pedagogy.

### Open design choices
- Map bounce to arms vs. a step — make it a Settings toggle; default arms (most visible on the doll).
- Whether to also offer a **live mirror** mode (emit moves in near-real-time) as a separate toy mode later.
- Minimum gesture confidence before committing vs. emitting a "?" step the child resolves in the editor.

---

## 10. Protocol Abstraction Layer (the only RE-dependent file)

`pal.h` / `pal.cpp` holds **all** doll-specific knowledge. **The RE phase (§11) is complete** —
the real UUIDs, opcode table, write type, and framing are documented in
**`docs/BLE_PROTOCOL.md`** and recovered source is in **`re/decompiled/`**. The values below are
now known (not placeholders); what remains is live timing/power tuning before `BLE_ENABLED 1`:

```c
// ---- FILLED FROM RE (docs/BLE_PROTOCOL.md); timing still tuned live ----
#define BLE_ENABLED            0          // 0 = SIMULATE, 1 = real BLE

static const char* DOLL_NAME_PREFIX = "DanceCD";        // adv name filter (confirmed)
// service UUID is discovered dynamically (match by characteristic), not hardcoded
static const BleUuid CMD_CHAR_UUID = "51901383-030F-4859-B643-256B0B2F5562"; // app->toy write
static const BleUuid NTF_CHAR_UUID = "51901382-030F-4859-B643-256B0B2F5562"; // toy->app notify
static const bool    WRITE_NO_RESP = true;              // Write Without Response (confirmed)

// packet = [opcode][params], <=20 bytes, little-endian. Movement is composed from MotorRun.
// opcodes: MotorRun=20, SetLED=21, PlayAudioSequence=16, PlaySequence=23, Motor1Goto=25,
//          SetToyVolume=35, AppModeSignal=80 (see docs/BLE_PROTOCOL.md for the full table)
static const MoveOpcode OPCODES[] = {
  { STEP_FORWARD, {20, /*BothWheels*/3, /*Run*/0, /*power*/200, /*Fwd*/0}, 5 },
  { TWIRL_LEFT,   {20, /*LeftWheel*/1, 0, 200, /*Rev*/1}, 5 },  // + opposite RightWheel packet
  // ...
};

// per-move execution time so the UI/runner pace matches the doll
static uint16_t durationOf(MoveId, int16_t p1);
// --------------------------------------------
```

PAL public surface (stable, UI never changes):
- `bool begin();`
- `bool connect();` / `bool isConnected();`
- `bool sendMove(MoveId, int16_t p1);`
- `uint16_t durationOf(MoveId, int16_t p1);`
- `bool supports(MoveId);` — drives palette greying.

**SIMULATE behavior (`BLE_ENABLED 0`):** `sendMove()` logs `move + bytes` to Serial and surfaces the same on the Run screen. The entire app — editor, save/load, run pacing — is exercisable with no doll present.

---

## 11. BLE reverse-engineering phase (prerequisite for M4)

> **STATUS: DONE (static phase).** The protocol was recovered by decompiling the official app
> (it's Unity **Mono**, so `Assembly-CSharp.dll` gave clean C#). Deliverables in-repo:
> - **`docs/BLE_PROTOCOL.md`** — UUIDs, opcode table, write type, packet framing, move mapping.
> - **`re/decompiled/`** — the recovered C# (source of truth); **`re/README.md`** — overview.
> - **`data/audio_catalog.csv`**, `phrase_labels.csv`, `phrase_durations.csv` — the 412-clip catalog.
> - **`assets/presets/dances/`** — the 10 extracted preset dances (+ `ymca`, `spindrop`).
> - **`scripts/re/`** — reproducible pipeline (`get_apk.sh`, `decompile.sh`, `grep_ble.sh`, `extract_assets.py`).
>
> Only §11.4 live confirmation on the real doll remains (timing/power tuning) before `BLE_ENABLED 1`.
> The steps below record the method used.

This is a **distinct, upfront investigation phase**, not firmware work. It touches no CYD code — its sole output is the data that fills the PAL (§10): the service/characteristic UUIDs, the move→bytes opcode table, the write type, and per-move timing.

**Sequencing:** runs *in parallel* with M0–M3, which use SIMULATE and a generic NimBLE stub and don't need the real doll. **M4 is blocked until this phase produces a validated opcode table.** Requires: the doll (powered, fresh batteries) and a Linux box and/or Android phone.

### 11.1 Obtain the app (APK)
- The Android *Dance Code* app is delisted from Google Play. Two ways to get it: pull it off an old device that still has it installed (`adb backup`, or copy the base APK from `/data/app/...` on a rooted device), or download an archived copy from a mirror (APKMirror / APKPure).
- This is your own app for a toy you own — interoperability RE. **But mirror sites are untrusted:** verify the package name matches Hasbro's listing, check the APK's signing certificate, and scan it before installing. Prefer the pulled-from-device copy when possible.
- iOS builds are far harder to decompile — use the Android APK.

### 11.2 Static analysis — decompile first (highest payoff)
- Run the APK through **`jadx`** (Java/Kotlin decompile) and optionally **`apktool`** (resources/smali).
- Grep the decompiled source for: `BluetoothGatt`, `writeCharacteristic`, `UUID.fromString`, service/characteristic UUID string literals, and `byte[]` / hex constants near move names.
- Target extractions: `SVC_UUID`, `CMD_CHAR_UUID`, write type (with/without response), and ideally the **move → opcode bytes** table straight from constants. This alone often yields ~80% of the PAL.
- Also locate any pairing/bonding logic (PIN, passkey, or just-works) and whether commands are plain writes vs. a framed protocol (headers/checksums).

### 11.3 Live GATT enumeration
- Power the doll; connect with **nRF Connect** (Android) or **`bluetoothctl` / `gatttool`** (Linux + BlueZ).
- Record: advertised name, MAC, all services & characteristics, and each characteristic's properties (Write / Write-No-Response / Notify).
- Cross-check the discovered UUIDs against what §11.2 found.

### 11.4 Confirm the command format
- Write candidate opcode bytes from §11.2 to the writable characteristic and **watch the doll**. Build the `move → bytes` + param-encoding table empirically.
- Measure inter-command timing and per-move durations → feeds `durationOf()`.
- If the decompile was unclear, this becomes *targeted* trial over a **finite, bounded move set** (arms/twirl/step/head/songs/dances) — not blind fuzzing.

### 11.5 Capture real traffic (fallback / verification)
- If §11.2–11.4 leave gaps: sideload the old APK on a spare Android, enable **Developer Options → Bluetooth HCI snoop log**, drive the doll from the app, then open `btsnoop_hci.log` in **Wireshark** to read the exact writes byte-for-byte.
- No sniffer hardware required; an **nRF52840 dongle** (Wireshark BLE sniffer) is an optional upgrade for capturing the connection live.

### 11.6 Hardware aids
- FCC ID **RS4-C3274**: the filing's **internal photos** identify the BLE SoC (predicts bonding behavior and any UART/SWD debug fallback), and the **RF test report** confirms band/modulation. Consult if the software paths stall.

### 11.7 Exit criteria → integrate into PAL
The phase is done when a validated table exists. Then:
1. Fill `SVC_UUID`, `CMD_CHAR_UUID`, `DOLL_NAME_PREFIX`, `WRITE_NO_RESP`.
2. Populate `OPCODES[]` byte sequences (from `jadx` constants, confirmed by GATT writes).
3. Tune `durationOf()` from observed move times.
4. Set `supports()` flags for the moves the doll actually exposes.
5. Flip `#define BLE_ENABLED 1`. Flash. → begins M4.

**Tooling checklist:** `jadx`, `apktool`, `adb`, nRF Connect (or BlueZ `bluetoothctl`/`gatttool`), Wireshark; optional nRF52840 dongle.

---

## 12. Open questions (resolve during RE)

- Pairing/bonding required, or open GATT? (If bonded, PAL gains a key-exchange step.)
- Single command characteristic, or separate ones per subsystem (motion vs audio)?
- Does the doll ACK / report busy, or is it fire-and-forget with fixed timing?
- Max command rate before the doll drops or queues moves.

Capture-side (resolve during build/playtest with Theo):
- Final threshold defaults and whether sensitivity needs an auto-calibrate vs. a manual slider.
- Does 6-axis yaw drift matter over a typical capture length, or is per-gesture reset enough? (Likely enough.)
- Which IMU part to standardize on, and its I2C pins on the chosen CYD revision.

---

## 13. Milestones

- **M0** — Skeleton: LovyanGFX up, screens navigable, PAL in SIMULATE, runner logs to serial.
- **M1** — Editor complete: add/insert/delete/reorder, params, Kid/Advanced palettes.
- **M2** — Persistence: save/load named sequences on LittleFS; seed the preset library from `assets/presets/dances/` and surface the sound catalog from `data/audio_catalog.csv`.
- **M3** — BLE central: scan/connect/status against a generic NimBLE peripheral stub.
- **M4** — RE integration: §11 static RE is **done** (`docs/BLE_PROTOCOL.md`) — plug the recovered opcodes into the PAL, do §11.4 live timing/power tuning on the real doll, then `BLE_ENABLED 1`.
- **M5** — IMU bring-up + auto-detect: I2C probe with WHO_AM_I verify, `imuPresent` gating, capture UI appears/disappears correctly on boards with and without the sensor; live serial plot of gyro/accel.
- **M6** — Dance to Code: segmentation + classifier + live feedback, output to scratch sequence.
- **M7** — Polish: REPEAT blocks, run-step highlight sync, settle-on-stop, sensitivity tuning with Theo.
- **M8** — Wand hardware: LiPo + charge/boost + battery ADC, OpenSCAD handle enclosure (print, fit, strap), rigid IMU mount, drop test. Turns the working board into the toybox artifact.
