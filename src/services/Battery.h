#pragma once
// services/Battery.h — LiPo level for the wand (SPEC §M8). Reads the battery voltage through a
// 2:1 divider on a free ADC pin and maps to a percent. If no plausible LiPo voltage is present
// (bench/USB power, no divider fitted) it reports USB so the UI shows a plug, not garbage.
#include <stdint.h>

namespace battery {
  void  begin();
  void  tick();          // periodic sample + smoothing
  bool  onUsb();         // true when no battery detected (bench/USB powered)
  int   percent();       // 0..100 (valid when !onUsb())
  float volts();
}
