#pragma once
// hal/Board.h — per-variant pins + capability flags (SPEC §2).
//
// Selected by a -D BOARD_* build flag in platformio.ini. ALL board-specific constants
// live here; the rest of the codebase is variant-agnostic and reads only the names below.
// Adding a board = a new block here + a new env. (Pattern + values from ../BladeKey-Overhead
// hal/Board.h and ../BladeAir/cyd.md — the hard-won CYD orientation/colour/touch facts.)

// ===========================================================================
#if defined(BOARD_CYD28_ILI9341)
// ===========================================================================
// 2.8" Cheap Yellow Display — ESP32-2432S028R, ILI9341 over HSPI, XPT2046 resistive
// touch on a SEPARATE VSPI bus. No PSRAM.
//
// The Elegoo "USB-C only" CYD reuses this whole block (same pins/driver) but its ILI9341 is
// portrait-native, so it overrides the orientation + touch-invert via -D flags (env cyd28_elegoo).
// Those defines are #ifndef-guarded so the flags win. (Pattern from ../BladeKey-Overhead.)

#ifdef BOARD_CYD28_ELEGOO
  #define BOARD_NAME            "CYD 2.8\" ILI9341 USB-C (Elegoo)"
#else
  #define BOARD_NAME            "CYD 2.8\" ILI9341 (ESP32-2432S028R)"
#endif

  // Display (ILI9341 over HSPI: GPIO 12/13/14)
  #define PIN_TFT_MOSI          13
  #define PIN_TFT_MISO          12
  #define PIN_TFT_SCLK          14
  #define PIN_TFT_CS            15
  #define PIN_TFT_DC             2
  #define PIN_TFT_RST           -1    // tied to system reset
  #define PIN_TFT_BL            21    // backlight, active HIGH

  // ORIENTATION (cyd.md / Overhead): this ILI9341 is mounted so the chip's MV=1 "landscape"
  // rotations render 90deg-wrong. Drive it LANDSCAPE-NATIVE 320x240 at MV=0. rotation 0 was
  // upright-but-mirrored on these units, so rotation 6 (= rot0 + horizontal flip) is upright.
  // -028R is landscape-native (320x240, MV=0). The Elegoo USB-C variant is portrait-native, so it
  // overrides these to 240x320 + rotation 1 (odd = landscape) via -D flags.
#ifndef TFT_PANEL_WIDTH
  #define TFT_PANEL_WIDTH      320
#endif
#ifndef TFT_PANEL_HEIGHT
  #define TFT_PANEL_HEIGHT     240
#endif
#ifndef DISPLAY_DEFAULT_ROTATION
  #define DISPLAY_DEFAULT_ROTATION 6
#endif
  // Reversed R/B wiring: set rgb_order or warm colours render cyan/blue (cyd.md).
#ifndef CYD_PANEL_RGB_ORDER
  #define CYD_PANEL_RGB_ORDER  1
#endif

  // Logical landscape canvas the UI draws in (unchanged across variants).
  #define SCREEN_W             320
  #define SCREEN_H             240

  // Touch (XPT2046) on its OWN VSPI bus (cyd.md) — separate sclk/mosi/miso
  #define PIN_TOUCH_SCLK        25
  #define PIN_TOUCH_MOSI        32    // T_DIN
  #define PIN_TOUCH_MISO        39    // T_OUT (input-only)
  #define PIN_TOUCH_CS          33
  #define PIN_TOUCH_IRQ         36    // input-only; NOT gated (poll pressure)
  // MV=0 landscape flip point-reflects calibrateTouch's learned map -> invert both axes.
  // The Elegoo's standard MV=1 landscape needs no inversion, so it overrides both to 0.
#ifndef TOUCH_INVERT_X
  #define TOUCH_INVERT_X        1
#endif
#ifndef TOUCH_INVERT_Y
  #define TOUCH_INVERT_Y        1
#endif

  // Backlight (cyd.md: low PWM freq — MOSFET can't switch off above ~10 kHz)
  #define BACKLIGHT_ACTIVE_HIGH  1
  #define BACKLIGHT_PWM_FREQ  5000
  #define BACKLIGHT_VIA_EXPANDER 0

  // I2C for the optional IMU: P3 "3V3 / Temp-humidity" connector. GPIO 21 (hw default SDA)
  // is stolen for the backlight, so the CYD routes GPIO 27 to P3 for I2C (cyd.md).
  #define PIN_I2C_SDA           27
  #define PIN_I2C_SCL           22
  #define I2C_FREQ_HZ           400000

  // Onboard speaker on GPIO 26 (DAC2 / via transistor) — chime feedback.
  #define PIN_SPEAKER           26

  // Wand battery tap (M8): LiPo voltage via a 2:1 divider to a free input-only ADC pin.
  // GPIO 35 is exposed on the Extended-IO connector and free (cyd.md). -1 = no monitor.
  #define PIN_BATT_ADC          35
  #define BATT_DIVIDER          2.0f

  // Capabilities
  #define CAP_HAS_PSRAM          0
  #define CAP_TOUCH_NEEDS_CAL    1   // resistive
  #define CAP_TOUCH_CAPACITIVE   0

// ===========================================================================
#elif defined(BOARD_CROWPANEL_S3_5HMI)
// ===========================================================================
// Elecrow CrowPanel Advance 5.0-HMI — ESP32-S3, 8 MB PSRAM, 800x480 parallel-RGB
// (ST7262), GT911 capacitive touch. (HAL branch present; full bring-up deferred.)

  #define BOARD_NAME            "CrowPanel Advance 5.0-HMI (ESP32-S3)"

  #define TFT_PANEL_WIDTH      800
  #define TFT_PANEL_HEIGHT     480
  #define DISPLAY_DEFAULT_ROTATION 0
  #define SCREEN_W             800
  #define SCREEN_H             480
  #define CYD_PANEL_RGB_ORDER  0

  #define PIN_I2C_SDA          15
  #define PIN_I2C_SCL          16
  #define I2C_FREQ_HZ          400000
  #define PIN_SPEAKER          -1

  #define PIN_BATT_ADC          -1     // battery monitor n/a on this variant
  #define BATT_DIVIDER          2.0f
  #define BACKLIGHT_ACTIVE_HIGH  1
  #define BACKLIGHT_VIA_EXPANDER 1
  #define CAP_HAS_PSRAM          1
  #define CAP_TOUCH_NEEDS_CAL    0
  #define CAP_TOUCH_CAPACITIVE   1

// ===========================================================================
#else
  #error "No board variant selected. Define BOARD_CYD28_ILI9341 or BOARD_CROWPANEL_S3_5HMI."
#endif
