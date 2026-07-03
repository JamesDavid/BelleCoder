#pragma once
// catalog/Catalog.h — friendly names for indexed audio/dance moves (SPEC §5). Built-in name
// tables (recovered in docs/BLE_PROTOCOL.md); M2 enriches phrases from data/audio_catalog.csv
// on LittleFS. Keeps the UI showing "The Rose Waltz" instead of raw #7.
#include <stdint.h>
#include "../model/Move.h"

namespace catalog {
  void        label(MoveId m, int16_t idx, char* out, int n);   // friendly text for a step
  int         songCount();
  int         danceCount();
  const char* songName(int i1);      // 1-based
  const char* danceName(int i1);     // 1-based

  // LED colour palette (LED_COLOR p1 = 0-based index)
  int         ledCount();
  const char* ledName(int i0);
  uint16_t    ledColor565(int i0);   // for UI swatches
  void        ledRGB(int i0, uint8_t& r, uint8_t& g, uint8_t& b);  // for the doll
}
