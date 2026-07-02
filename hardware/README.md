# BelleCoder wand — hardware (SPEC §2, §M8)

Turns the working CYD board into the untethered toybox artifact: an onboard LiPo with
charge/boost, a battery-level tap the firmware reads, a rigidly-mounted IMU, and a parametric
handle enclosure with a wrist strap. Power + enclosure are first-class deliverables here — the
cost accepted in the v0.5 platform decision (a dedicated wand instead of an iPad app).

## Bill of materials
| Qty | Part | Notes |
|---|---|---|
| 1 | CYD `ESP32-2432S028R` | 2.8" ILI9341 + XPT2046 (the board this firmware targets) |
| 1 | LiPo cell, 1S 3.7V ~1000 mAh (503450) | size set by `batt_*` params in `wand.scad` |
| 1 | TP4056 charge board **with protection** (DW01+FS8205) | USB-C input; use the protected variant |
| 1 | 3.3 V boost/LDO *or* run the CYD from 5 V boost into its VIN | see Power below |
| 1 | 6-axis IMU breakout (MPU-6050 / LSM6DSO / ICM-42670) | auto-detected on I2C (SPEC §9) |
| 2 | 100 kΩ resistors | battery-voltage divider (2:1) |
| 4 | M3 self-tapping screws | board mount, 78 × 42 mm pitch (cyd.md) |
| 2 | M2 screws | IMU boss |
| 1 | Wrist strap | 12 mm webbing through the handle slot |
| — | Opaque black PETG filament | drop-tough + blocks LCD→LDR light bleed (cyd.md) |

## Power
The stock CYD has **no battery management** — add it:

```
USB-C ──► TP4056 (charge + protection) ──► LiPo 1S
                       │
                       ├─► BOOST/LDO ──► CYD 3V3 rail (or 5V→VIN)
                       │
                       └─► [100k]──┬──[100k]──► GND      (2:1 divider)
                                   │
                                   └──► GPIO 35 (ADC1, input-only)  ← firmware battery tap
```

- **Battery tap:** GPIO 35 is exposed on the Extended-IO connector and free (cyd.md). The 2:1
  divider keeps Vbat (≤4.2 V) under the 3.3 V ADC ceiling. Firmware: `services/Battery.*`
  (`PIN_BATT_ADC 35`, `BATT_DIVIDER 2.0`) samples + smooths and shows a battery/USB icon on Home.
  On bench/USB power with no divider fitted it reads "USB" rather than garbage.
- **Boost:** simplest is a 5 V boost into the CYD's VIN/5V pin (the onboard regulator makes 3.3 V).
  A direct 3.3 V LDO from the cell also works but browns out as the cell sags — prefer the boost.
- Keep a **100–470 µF cap** across the 5 V rail (cyd.md): the WROOM + backlight pull ~150–200 mA
  with spikes during display updates; a thin rail sags and the backlight flickers.

## IMU mounting (critical)
Mount the IMU **rigidly to the board** with a short standoff — no flex, no cable slack — so
gyro/accel track the wand's true motion (SPEC §2). The `imu_boss` in `wand.scad` is a solid boss
with M2 pilots near the board's inertial centre. Wire it to the **P3 "3V3/Temp-humidity"
connector**: `SDA = GPIO 27, SCL = GPIO 22, 3V3, GND` (`Wire.begin(27,22)`, cyd.md). Do **not**
use 5 V on the sensor. Detection + graceful-degradation is automatic (SPEC §9): no IMU → capture
features hidden.

## Enclosure (`wand.scad`)
Parametric OpenSCAD, two printed parts:
- `part="back"` — shell + oval handle (flattened, small-hands, non-roll), LiPo cavity, IMU boss,
  wrist-strap slot, USB-C charge-port cutout, M3 board posts.
- `part="front"` — bezel framing the LCD, touch window, and the **LDR pinhole offset to the
  corner** (the LDR is not centred — cyd.md).

Render/export:
```
openscad -D 'part="back"'  -o wand_back.stl  hardware/wand.scad
openscad -D 'part="front"' -o wand_front.stl hardware/wand.scad
openscad -D 'part="preview"' hardware/wand.scad     # both, exploded
```
Measure your exact board/cell/IMU and adjust the `board_*`, `batt_*`, `imu_*` params before
printing. Print black PETG for drop-toughness and to stop LCD edge-light reaching the LDR.

## Drop-test checklist (SPEC §M8)
- [ ] Battery + boost secured (foam/tape); no strain on the JST leads.
- [ ] TP4056 protection variant confirmed (over-discharge cutoff) before first full drain.
- [ ] IMU boss screws torqued; wiggle-test = zero board flex.
- [ ] Charge port aligns with the shell cutout; cable seats fully.
- [ ] Wrist strap rated for the wand weight; slot edges chamfered.
- [ ] 1 m drop onto carpet ×5 (each face): screen intact, no rattles, still boots.
- [ ] 1 m drop onto hard floor ×1 corner: shell survives, board unseated? re-seat + re-test.
- [ ] Post-drop: `pio device monitor` boots clean; touch still calibrated; IMU still detected.
- [ ] Battery icon tracks a real charge/discharge cycle (compare to a multimeter on Vbat).
```
```
